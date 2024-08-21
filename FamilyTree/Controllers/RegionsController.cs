using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FamilyTree.Data;
using FamilyTree.Models;
using FamilyTree.Models.RegionModel;

namespace FamilyTree.Controllers
{
    public class RegionsController : Controller
    {
        private readonly FamilyTreeContext _context;

        public RegionsController(FamilyTreeContext context)
        {
            _context = context;
        }

		// GET: Regions
		public async Task<IActionResult> Index(
					string currentFilter, // 搜索栏
					string searchString,
					int? pageNumber)
		{
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
			var regions = from s in _context.Regions.AsNoTracking()
						  select s;
			if (!String.IsNullOrEmpty(searchString))
			{
                regions = regions.Where(r => r.CityName.Contains(searchString)
                                            || r.DistrictName.Contains(searchString)
                                            || r.ProvinceName.Contains(searchString)
                                            || r.RegionId.Equals(Convert.ToInt32(searchString)));
			}

            //设置分页
			int pageSize = 20;
			var paginatedList = await PaginatedList<Region>
				.CreateAsync(regions, pageNumber ?? 1, pageSize);


			return View(paginatedList);
		}

		// GET: Regions/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (region == null)
            {
                return NotFound();
            }

            return View(region);
        }

        // GET: Regions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Regions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProvinceName,CityName,DistrictName")] Region region)
        {
            if (ModelState.IsValid)
            {
                _context.Add(region);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(region);
        }

        // GET: Regions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions.FindAsync(id);
            if (region == null)
            {
                return NotFound();
            }
            return View(region);
        }

        // POST: Regions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProvinceName,CityName,DistrictName")] Region region)
        {
            if (id != region.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(region);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegionExists(region.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(region);
        }

        // GET: Regions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var region = await _context.Regions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (region == null)
            {
                return NotFound();
            }
            return View(region);
        }

        // POST: Regions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            _context.Regions.Remove(region);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegionExists(int id)
        {
            return _context.Regions.Any(e => e.Id == id);
        }


        /// <summary>
        /// 检索出省份信息
        /// </summary>
        /// <returns>
        /// Json形式返回
        /// </returns>
		public IActionResult GetProvinces()
		{
			var provinces = _context.Provinces
				.Select(r => r.Name)
				.ToList();
			return Json(provinces);
		}

		/// <summary>
		/// 根据指定的省份名称，检索该省份下所有不同的城市名称列表。
		/// </summary>
		/// <param name="province">省份名称。</param>
		/// <returns>
		/// 以 JSON 格式返回的城市名称列表。
		/// </returns>
		[HttpGet]
		public IActionResult GetCities(string province)
		{
            var SelectedProvince = _context.Provinces.Where(p => p.Name == province).FirstOrDefault();
            var cities = _context.Cities
                .Where(c => c.ProvinceCode == SelectedProvince.Code)
                .Select(c => c.Name)
                .ToList();
			return Json(cities);
		}

		[HttpGet]
		public IActionResult GetAreas(string province,string city)
		{
            if (province == null || city == null) { return null; }
            var SelectedProvince = _context.Provinces.Where(p => p.Name == province).FirstOrDefault();
			var SelectedCity = _context.Cities.Where(c => c.ProvinceCode == SelectedProvince.Code && c.Name == city).FirstOrDefault();
			var areas = _context.Areas
                .Where(a => a.ProvinceCode == SelectedProvince.Code && a.CityCode == SelectedCity.Code)
                .Select(c => c.Name)
				.ToList();
			return Json(areas);
		}

		[HttpGet]
		public IActionResult GetStreets(string province, string city,string area)
		{
            var SelectedProvince = _context.Provinces.Where(p => p.Name == province).FirstOrDefault();
            var SelectedCity = _context.Cities.Where(c => c.Name == city).FirstOrDefault();
            var SelectedArea = _context.Areas.Where(a => a.Name == area).FirstOrDefault();

			var streets = _context.Streets
				.Where(s => s.AreaCode == SelectedArea.Code && s.ProvinceCode == SelectedProvince.Code && s.AreaCode == SelectedArea.Code)
				.Select(c => c.Name)
				.ToList();
			return Json(streets);
		}

		[HttpGet]
		public IActionResult GetVillages(string province, string city, string area,string street)
		{
            var SelectedProvince = _context.Provinces.Where(p => p.Name == province).FirstOrDefault();
            var SelectedCity = _context.Cities.Where(c => c.Name == city && c.ProvinceCode == SelectedProvince.Code).FirstOrDefault();
            var SelectedArea = _context.Areas.Where(a => a.Name == area && a.CityCode == SelectedCity.Code).FirstOrDefault();
            var SelectedStreet = _context.Streets.Where(s => s.Name == street && s.AreaCode == SelectedArea.Code).FirstOrDefault();
			var villages = _context.Villages
				.Where(v => v.StreetCode == SelectedStreet.Code && v.ProvinceCode==SelectedProvince.Code && v.AreaCode == SelectedArea.Code)
				.Select(c => c.Name)
				.ToList();
			return Json(villages);
		}
	}
}
