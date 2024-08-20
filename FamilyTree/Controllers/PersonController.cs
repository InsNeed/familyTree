using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FamilyTree.Data;
using FamilyTree.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security;
using Microsoft.CodeAnalysis;
using FamilyTree.Models.RegionModel;

namespace FamilyTree.Controllers
{

	public class PersonController : Controller
	{
		private ILogger<PersonController> _logger;
		private readonly FamilyTreeContext _context;

		public PersonController(FamilyTreeContext context, ILogger<PersonController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// GET: People
		public async Task<IActionResult> Index(
			string sortOrder, // 排序
			string currentFilter,
			string searchString,
			int? pageNumber,
			int? id)//被选中的人的Id
		{
			// 设置视图数据
			ViewData["CurrentSort"] = sortOrder;
			ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

			//搜索栏
			if (searchString != null)
			{
				pageNumber = 1;
			}
			else
			{
				searchString = currentFilter;
			}

			ViewData["CurrentFilter"] = searchString;

			// 获取并过滤数据
			var persons = from s in _context.Persons.Include(e => e.Events).AsNoTracking()
						  select s;
			if (!String.IsNullOrEmpty(searchString))
			{
				persons = persons.Where(s => s.LastName.Contains(searchString)
											|| s.FirstName.Contains(searchString));
			}

			//排序
			switch (sortOrder)
			{
				case "name_desc":
					persons = persons.OrderByDescending(s => s.LastName);
					break;
				case "Date":
					persons = persons.OrderBy(s => s.Birthday);
					break;
				case "date_desc":
					persons = persons.OrderByDescending(s => s.Birthday);
					break;
				default:
					persons = persons.OrderBy(s => s.LastName);
					break;
			}

			// 创建分页列表
			int pageSize = 20;
			var paginatedList = await PaginatedList<Person>
				.CreateAsync(persons, pageNumber ?? 1, pageSize);

			//构造家族树
			if (id != null)
			{
				ViewData["PersonID"] = id.Value;
				ViewData["FamilyTree"] = await BulidFamilyTree(id.Value);
			}
			// 返回视图，并传递分页后的数据
			return View(paginatedList);
		}


		// GET: People/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var person = await _context.Persons
				.FirstOrDefaultAsync(m => m.ID == id);
			if (person == null)
			{
				return NotFound();
			}

			//查父母ID
			Person p1 = GetPersonById(person.Father);
			Person p2 = GetPersonById(person.Mother);
			if (p1 != null)
				ViewData["FatherName"] = p1.LastName + p1.FirstName;
			if (p2 != null)
				ViewData["MotherName"] = p2.LastName + p2.FirstName;

			//地区
			ViewData["Province"] = GetProvince(person);
			ViewData["City"] = GetCity(person);
			ViewData["Area"] = GetArea(person);
			ViewData["Street"] = GetStreet(person);
			ViewData["Village"] = GetVillage(person);

			return View(person);
		}

		// GET: People/Create
		public IActionResult Create()
		{
			return View();
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("ID,LastName,FirstName,Gender,Birthday,Age,PhotoUrl")] Person person
			, string FatherName, string MotherName
			, IFormFile? Photo
			, string? province
			, string? city
			, string? area
			, string? street
			, string? village)
		{
			if (ModelState.IsValid)
			{
				//开启事务
				using (var transaction = await _context.Database.BeginTransactionAsync())
				{
					try
					{
						//每步成功才提交
						bool succ = true;

						await UpLoadPhoto(person, Photo);

						succ = succ == true ? await SetParent(person, FatherName, MotherName) : succ;

						succ = succ == true ? await SetRegionCode(person, province, city, area, street, village) : succ;


						if (succ)
						{
							_context.Add(person);
							await _context.SaveChangesAsync();
							await transaction.CommitAsync(); // 提交事务!
							TempData["SuccessMessage"] = "添加成功";
						}
						else
						{
							await transaction.RollbackAsync();
							ModelState.AddModelError("", "添加失败");
							return View(person);
						}
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						ModelState.AddModelError("", "An error occurred while saving data.");
						_logger.LogError(ex, "An error occurred while creating a person.");
						return View(person);
					}
					return View(person);
				}
			}
			return View(person);
		}

		// GET: People/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var person = await _context.Persons.FindAsync(id);
			if (person == null)
			{
				return NotFound();
			}

			//父母
			ViewData["Father"] = person.Father;
			ViewData["Mother"] = person.Mother;

			//籍贯
			ViewData["Province"] = person.ProvinceId == null ? "SELECT" : GetProvince(person);
			ViewData["City"] = person.CityId == null ? "SELECT" : GetCity(person);
			ViewData["Area"] = person.AreaId == null ? "SELECT" : GetArea(person);
			ViewData["Street"] = person.StreetId == null ? "SELECT" : GetStreet(person);
			ViewData["Village"] = person.VillageId == null ? "SELECT" : GetVillage(person);

			return View(person);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id
			, [Bind("ID,LastName,FirstName,Gender,Birthday,PlaceOfOrigin,Age")] Person person
			, string Father
			, string Mother
			, IFormFile? Photo
			, string? province
			, string? city
			, string? area
			, string? street
			, string? village)
		{
			if (id != person.ID)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				using (var transaction = await _context.Database.BeginTransactionAsync())
				{
					try
					{
						//是否全部成功
						bool succ = true;

						Person OriginalPerson = _context.Persons.AsNoTracking().FirstOrDefault(p => p.ID == id);

						//处理照片
						string OriginalPhotoUrl = "";
						if (Photo != null)
						{
							//IFormFile Photo不为空就删除原始照片并上传新照片
							DeletePhoto(OriginalPerson);
							await UpLoadPhoto(person, Photo);
						}
						else
						{
							//未上传新照片,保存原路径,覆盖update后的路径
							OriginalPhotoUrl = OriginalPerson.PhotoUrl;
						}



						//处理地区
						var OriginalProvince = OriginalPerson.ProvinceId;
						var OriginalCity = OriginalPerson.CityId;
						var OriginalArea = OriginalPerson.AreaId;
						var OriginalStreet = OriginalPerson.StreetId;
						var OriginalVillage = OriginalPerson.VillageId;
						if (province != null)
						{
							succ = succ == true ? await SetRegionCode(person, province, city, area, street, village) : succ;
						}

						_context.Update(person);


						//父母
						succ = succ == true ? await
							SetParent(person, Convert.ToString(Father), Convert.ToString(Mother)) : succ;
						//判断保存父母情况
						if (succ == false)
						{
							ModelState.AddModelError("", "保存失败");
							return View(person);
						}

						//用原数据覆盖UPDATE后的数据
						person.PhotoUrl = OriginalPhotoUrl == "" ? person.PhotoUrl : OriginalPhotoUrl;

						if(person.ProvinceId == null)
						{
							person.ProvinceId = OriginalProvince;
							person.CityId = OriginalCity;
							person.AreaId = OriginalArea;
							person.StreetId = OriginalStreet;
							person.VillageId = OriginalVillage;
						}
							

						await _context.SaveChangesAsync();
						await transaction.CommitAsync(); // 提交事务!!!!!!!!!
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						ModelState.AddModelError("", "An error occurred while saving data.");
						_logger.LogError(ex, "An error occurred while updating a person.");
						return View(person);
						throw;
					}
					return RedirectToAction(nameof(Index));
				}
			}
			return View(person);
		}

		// GET: People/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var person = await _context.Persons
				.FirstOrDefaultAsync(m => m.ID == id);
			if (person == null)
			{
				return NotFound();
			}

			Person p1 = GetPersonById(person.Father);
			Person p2 = GetPersonById(person.Mother);
			if (p1 != null)
				ViewData["FatherName"] = p1.FirstName + p1.LastName;
			if (p2 != null)
				ViewData["MotherName"] = p2.FirstName + p2.LastName;

			return View(person);
		}

		// POST: People/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			//删除和此人相关的事件
			var deleteEvent = _context.Events
				.Where(e => e.Person.ID == id).ToList();
			_context.Events.RemoveRange(deleteEvent);

			var person = await _context.Persons.FindAsync(id);
			_context.Persons.Remove(person);

			DeletePhoto(person);

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));

		}

		private bool PersonExists(int id)
		{
			return _context.Persons.Any(e => e.ID == id);
		}

		/// <summary>
		/// 按全名查找人
		/// </summary>
		/// <param name="FullName"></param>
		/// <returns>返回找到的人的列表</returns>
		private async Task<List<Person>> GetPersonByName(string FullName)
		{
			if (string.IsNullOrWhiteSpace(FullName))
			{
				return new List<Person>();
			}

			return await _context.Persons
				.AsNoTracking()
				.Where(n => n.LastName + n.FirstName == FullName)
				.ToListAsync();
		}

		/// <summary>
		/// 按ID查找人
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Person对象</returns>
		private Person GetPersonById(int? id)
		{
			if (id == null)
			{
				return null;
			}
			Person p = _context.Persons.FirstOrDefault(n => n.ID == id);
			return p;
		}

		/// <summary>
		///  根据父母标识设置指定人的父/母 
		/// </summary>
		/// <param name="person">要更新的人</param>
		/// <param name="Father0OrMother1">
		///     如果为 <c>false</c>，则表示父亲；如果为 <c>true</c>，则表示母亲。
		/// </param>
		/// <param name="ParentName"></param>
		/// <returns>找到对应的父母并成功设置，返回 <c>true</c>；否则返回 <c>false</c></returns>
		private async Task<bool> SetParentByNameAsync(Person person, bool Father0OrMother1, string ParentName)
		{

			List<Person> ParentList = await GetPersonByName(ParentName);
			//排除重名和未找到情况
			if (ParentList.Count == 0)
			{
				ModelState.AddModelError("", "未找到此人 父或母");
				return false;
			}
			else if (ParentList.Count > 1)
			{
				ViewData["DuplicateParents"] = ParentList;
				ModelState.AddModelError("", "父或母与他人重名，用ID添加");
				return false;
			}
			//根据标识设置父母
			if (Father0OrMother1 == false)
			{
				person.Father = ParentList[0].ID;
			}
			else
			{
				person.Mother = ParentList[0].ID;
			}

			return true;
		}



		/// <summary>
		/// 根据Id设置父母
		/// </summary>
		/// <param name="person"></param>
		/// <param name="FatherId"></param>
		/// <param name="MotherId"></param>
		/// <returns>成功为true</returns>
		public async Task<bool> SetParentsByIdAsync(Person person, bool Father0OrMother1, int ParentId)
		{

			var Parent = await _context.Persons
				.AsNoTracking()
				.FirstOrDefaultAsync(n => n.ID == ParentId);
			//根据标识设置父母
			if (Parent != null)
			{
				if (Father0OrMother1 == false)
				{
					person.Father = Parent.ID;
				}
				else
				{
					person.Mother = Parent.ID;
				}
			}
			else
			{
				ModelState.AddModelError("", "Parent ID 不存在");
				return false;
			}

			return true;

		}

		public async Task<TreeNode> BulidFamilyTree(int id)
		{
			//防止自己生自己导致循环，使用哈希表记录
			var HashMap = new HashSet<Person>();

			//得到根节点及其children
			var person = GetPersonById(id);
			Person rootPerson = null;
			while (person != null)
			{

				//得到根
				rootPerson = person;
				//如果出现自己生自己
				if (HashMap.Contains(rootPerson))
				{
					ViewData["ErrorMessage"] = "该家族树中存在自我循环，导致树结构错误";
					return null;
				}
				HashMap.Add(person);

				person = GetPersonById(person.Mother);
			}
			//根节点
			TreeNode TreeRoot = new TreeNode(rootPerson);

			HashMap.Clear();

			await Recurs(TreeRoot, rootPerson.ID);
			return TreeRoot;

			//递归构造树，node为根节点，parentId为根节点Person的Id
			async Task Recurs(TreeNode node, int parentId)
			{
				var person = GetPersonById(parentId);


				if (HashMap.Contains(person))
				{
					ViewData["ErrorMessage"] = "该家族树中存在自我循环，导致树结构错误";

					return;
				}
				HashMap.Add(person);

				//得到此人的所有孩子
				var Children = await _context.Persons
					.Where(n => n.Father == parentId || n.Mother == parentId)
					.AsNoTracking()
					.ToListAsync();


				//计数spouse的数量
				int counter = 0;
				foreach (var child in Children)
				{
					try
					{
						//将每个孩子作为当前节点的子节点
						TreeNode childNode = new TreeNode(child);
						node.AddChild(childNode);
						//配偶Id为子的父/母且非本人

						//配偶ID为 当前节点Person是孩子的父亲的 ? 孩子的母亲ID：孩子父亲ID
						int? spouseIdTemp = node.person.ID == child.Father ? child.Mother : child.Father;
						//此人有配偶
						if (spouseIdTemp.HasValue)
						{
							//同时将此人Id和姓名加入 类Node 的 spouseId、spouseName列表
							node.spouseId.Add(spouseIdTemp.Value);
							Person spousePerson = GetPersonById(node.spouseId[counter++]);
							if (spousePerson != null)
								node.spouseName.Add(spousePerson.LastName + spousePerson.FirstName);
						}

						//将孩子列表的每个孩子都递归
						await Recurs(childNode, child.ID);
					}
					catch (Exception ex)
					{
						throw;
					}
				}
				return;
			}
		}


		public class TreeNode
		{
			public Person person { get; set; }
			public List<int>? spouseId { get; set; }
			public List<string>? spouseName { get; set; }

			public List<TreeNode> Children { get; set; }

			public TreeNode(Person value)
			{
				person = value;
				Children = new List<TreeNode>();
				spouseName = new List<string>();
				spouseId = new List<int>();
			}

			public void AddChild(TreeNode child)
			{
				Children.Add(child);
			}
		}

		public async Task<bool> SetParent(Person person, string FatherName, string MotherName)
		{
			//判断是否能转换为数字
			Func<string, bool> isNumeric = str => int.TryParse(str, out _);
			bool succ = true;

			//不为空就判断参数是ID还是字符串，ID就SetParentsById，字符串就SetParentByName
			//false为设定父亲,true为设定母亲
			if (FatherName != null && FatherName != "")
			{
				if (isNumeric(FatherName))
				{
					succ = succ == true ?
						await SetParentsByIdAsync(person, false, Convert.ToInt32(FatherName)) : succ;
				}
				else
				{
					succ = succ == true ?
						await SetParentByNameAsync(person, false, FatherName) : succ;
				}
			}
			if (MotherName != null && MotherName != "")
			{
				if (isNumeric(MotherName))
				{
					succ = succ == true ?
						await SetParentsByIdAsync(person, true, Convert.ToInt32(MotherName)) : succ;
				}
				else
				{
					succ = succ == true ?
						await SetParentByNameAsync(person, true, MotherName) : succ;
				}
			}

			if (!succ)
			{
				ModelState.AddModelError("", "An error occurred while seting parent.");
				return false;
			}
			else
			{
				//_context.Add(person);
				await _context.SaveChangesAsync();
				return true;

			}
		}




		public void DeletePhoto(Person person)
		{
			if (person.PhotoUrl != null)
			{
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", person.PhotoUrl);
				if (System.IO.File.Exists(filePath))
				{
					// 删除文件
					System.IO.File.Delete(filePath);
				}
			}
		}

		public IActionResult GetPhoto(int Id, string fileName)
		{
			var person = GetPersonById(Id);

			var SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", person.PhotoUrl);

			if (System.IO.File.Exists(SavePath))
			{
				var fileBytes = System.IO.File.ReadAllBytes(SavePath);

				if (fileBytes != null)
					return File(fileBytes, "image/jpeg");
				else return NotFound();
				//return File(fileBytes, "image/jpeg"); // 根据实际文件类型调整 MIME 类型
			}
			else return NotFound();
		}


		/// <summary>
		/// 上传照片,设置唯一guid,并设定PhotoUrl
		/// </summary>
		/// <param name="person"></param>
		/// <param name="Photo"></param>
		/// <returns>PhotoUrl,上传失败为 "" </returns>
		public async Task<string> UpLoadPhoto(Person person, IFormFile Photo)
		{

			if (Photo != null && Photo.Length > 0)
			{
				var OriginalFileName = Path.GetFileName(Photo.FileName);

				string fileExtension = Path.GetExtension(OriginalFileName);
				string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);
				//var filePath = Path.Combine(SavePath, fileName);

				var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

				if (!Directory.Exists(uploadsPath))
				{
					Directory.CreateDirectory(uploadsPath);
				}

				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await Photo.CopyToAsync(fileStream);
				}

				person.PhotoUrl = $"uploads/{uniqueFileName}";
				return person.PhotoUrl;
			}
			return "";
		}


		public async Task<bool> SetRegionCode(Person person, string? province, string? city, string? area, string? street, string? village)
		{
			try
			{
				person.ProvinceId = _context.Provinces
					.Where(p => p.Name == province)
					.Select(p => p.Code)
					.FirstOrDefault();

				person.CityId = _context.Cities
					.Where(c => c.Name == city && c.ProvinceCode == person.ProvinceId)
					.Select(c => c.Code)
					.FirstOrDefault();

				person.AreaId = _context.Areas
					.Where(a => a.Name == area && a.CityCode == person.CityId)
					.Select(a => a.Code)
					.FirstOrDefault();

				person.StreetId = _context.Streets
					.Where(st => st.Name == street && st.AreaCode == person.AreaId && st.CityCode == person.CityId) 
					.Select(s => s.Code)
					.FirstOrDefault();

				person.VillageId = _context.Villages
					.Where(v => v.Name == village && v.StreetCode == person.StreetId && v.AreaCode == person.AreaId && v.CityCode == person.CityId)
					.Select(v => v.Code)
					.FirstOrDefault();
				return true;

			}
			catch (Exception ex)
			{
				return false;
			}
		}


		public string? GetProvince(Person person)
		{
			if (person.ProvinceId != null)
			{
				return _context.Provinces
					.Where(p => p.Code == person.ProvinceId).Select(p => p.Name).First();
			}
			return null;
		}

		public string? GetCity(Person person)
		{
			if (person.CityId != null)
			{
				return _context.Cities
					.Where(c => c.Code == person.CityId).Select(c => c.Name).FirstOrDefault();
			}
			return null;
		}

		public string? GetArea(Person person)
		{
			if (person.AreaId != null)
			{
				return _context.Areas
					.Where(a => a.Code == person.AreaId).Select(a => a.Name).FirstOrDefault();
			}
			return null;
		}

		public string? GetStreet(Person person)
		{
			if (person.StreetId != null)
			{
				return _context.Streets
					.Where(s => s.Code == person.StreetId).Select(s => s.Name).FirstOrDefault();
			}
			return null;
		}

		public string? GetVillage(Person person)
		{
			if (person.VillageId != null)
			{
				return _context.Villages
					.Where(v => v.Code == person.VillageId).Select(v => v.Name).FirstOrDefault();
			}
			return null;
		}


	}
}










