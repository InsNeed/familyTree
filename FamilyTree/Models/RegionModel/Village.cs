using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FamilyTree.Models.RegionModel
{
	public class Village
	{
		[Key]
		[Column(TypeName = "nvarchar(50)")]
		public string Code { get; set; }

		[Column(TypeName = "nvarchar(100)")]
		public string Name { get; set; }


		[Column(TypeName = "nvarchar(50)")]
		public string CityCode { get; set; }


		[Column(TypeName = "nvarchar(50)")]
		public string ProvinceCode { get; set; }


		[Column(TypeName = "nvarchar(50)")]
		public string AreaCode { get; set; }


		[Column(TypeName = "nvarchar(50)")]
		public string StreetCode { get; set; }
	}
}
