using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FamilyTree.Models.ViewModel
{
    public class PersonEventView
    {
        [Key]
        public int ID;
        public IEnumerable<Person> Persons { get; set; }
        public IEnumerable<Event> Events { get; set; }
        
    }
}
