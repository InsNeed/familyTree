using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FamilyTree.Models.ViewModel
{
	public class PersonView
	{
		public Person person {  get; set; }
		public string Province { get; set; }
		public string City { get; set; }
		public string Area { get; set; }
		public string Street { get; set; }
		public string Village { get; set; }
	}
}
