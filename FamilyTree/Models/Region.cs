using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyTree.Models
{
	public class Region
	{
		
		public int Id { get; set; }
		[Display(Name = "地区编码")]
		public int RegionId { get; set; }

        [Display(Name = "省份")]
        public string ProvinceName { get; set; }

        [Display(Name = "地级市")]
        public string CityName{ get; set; }

        [Display(Name = "区/县")]
        public string DistrictName { get; set; }

	}
}
