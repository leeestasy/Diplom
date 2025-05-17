using System.ComponentModel.DataAnnotations.Schema;

namespace WTrailPacker.Models
{
    //[Table("ProductRestriction")]
    public class ProductRestriction
    {
        public int ProductID { get; set; }
        public int RestrictionID { get; set; }

        public Product Product { get; set; }
        public DietaryRestriction Restriction { get; set; }
    }
}
