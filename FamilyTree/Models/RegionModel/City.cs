using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FamilyTree.Models.RegionModel
{
	public class City
	{
		[Key]
		[Column(TypeName = "varchar(255)")]
		public string Code { get; set; }

		[Column(TypeName = "varchar(255)")]
		public string Name { get; set; }


		[Column(TypeName = "varchar(255)")]
		public string ProvinceCode { get; set; }
	}
}
