using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FamilyTree.Models
{
	public class Detail
	{
		[Key]
		public int Id { get; set; }

		public int PersonId { get; set; }

		[Required]
		[Display(Name = "时间")]
		public DateTime Date { get; set; }

		[Required]
		[Display(Name = "地区")]
		public string Location { get; set; }

		[Required]
		[Display(Name = "事件")]
		public string Activitie { get; set; }


	}
}
