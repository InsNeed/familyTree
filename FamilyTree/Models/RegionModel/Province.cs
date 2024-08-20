using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyTree.Models.RegionModel
{
    public class Province
    {
        [Key]
        [Column(TypeName = "varchar(255)")]
        public string Code { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string Name { get; set; }

    }
}
