using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using FamilyTree.Models.RegionModel;

namespace FamilyTree.Models
{
	public class Person
	{
		public int ID { get; set; }

		[StringLength(50)]
		[Display(Name = "姓")]
		public string? LastName { get; set; }


		[StringLength(50)]
		[Column("FirstName")]
		[Display(Name = "名")]
		public string? FirstName { get; set; }

		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		[Display(Name = "出生日期")]
		public DateTime? Birthday { get; set; }

		[Display(Name = "性别")]
        [GenderValidation]
        public string? Gender { get; set; }

		[Display(Name = "区县编码")]
		public int? RegionId { get; set; }

		[Column(TypeName = "nvarchar(50)")]
		[Display(Name = "区县编码")]
        public string? RegionCode { get; set; }

        [Display(Name = "年龄")]
		public int? Age { get; set; }

		[Display(Name = "父")]
		public int? Father { get; set; }
		[Display(Name = "母")]
		public int? Mother { get; set; }

        [Display(Name = "照片")]
		public string? PhotoUrl { get; set; }

		//事件
        public ICollection<Event> Events { get; set; }

		//籍贯信息
		public string? ProvinceId { get; set; }
		public string? CityId { get; set; }
		public string? AreaId { get; set; }
		public string? StreetId { get; set; }
		public string? VillageId { get; set; }
		//[NotMapped]
		//public byte[]? Photo { get; set; }

	}
}
