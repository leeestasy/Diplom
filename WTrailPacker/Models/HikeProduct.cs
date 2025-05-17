using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WTrailPacker.Models;
[Table("HikeProduct")]
public class HikeProduct
{
    [Key]
    [Column("HikeProductID")]
    public int HikeProductID { get; set; }

    [ForeignKey("Hike")]
    public int HikeID { get; set; }

    [ForeignKey("Product")]
    public int ProductID { get; set; }

    [Required]
    [MaxLength(50)]
    public string CategoryName { get; set; } = "DefaultCategory";

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Quantity { get; set; }

    public int? Packages { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal WeightPerUnit { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal CaloriesPer100g { get; set; }
    public bool IsSublimated { get; set; }
    public bool IsPerishable { get; set; }
    public int? ShelfLifeDays { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalWeight { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalCalories { get; set; }

    public bool IsAlternative { get; set; }
    public int? OriginalProductId { get; set; }

    [MaxLength(100)]
    public string? OriginalProductName { get; set; } /*= string.Empty;*/

    public int? CompatibilityScore { get; set; }

    // Навигационные свойства
    public virtual Hike Hike { get; set; }

    public virtual Product Product { get; set; }

    //[Required] 
    [MaxLength(20)]
    public string? UnitType { get; set; }

}