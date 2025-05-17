using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WTrailPacker.Models;

public partial class Hike
{
    public Hike()
    {
        HikeProducts = new List<HikeProduct>();
        RestrictionID = null; 
    }

    [Key]
    [Column("HikeID")] 
    public int HikeID { get; set; }

    [Required(ErrorMessage = "Укажите количество дней")]
    [Range(1, 365, ErrorMessage = "Длительность похода от 1 до 365 дней")]
    public int NumDays { get; set; }

    [Required(ErrorMessage = "Укажите количество человек")]
    [Range(1, 30, ErrorMessage = "Группа от 1 до 30 человек")]
    public int NumPeople { get; set; }

    [Required(ErrorMessage = "Выберите вид похода")]
    [Column("TripTypeID")] // Указываем имя столбца, если в БД используется подчеркивание
    public int TripTypeID { get; set; }

    [Column("RestrictionID")] // Синхронизация с БД
    public int? RestrictionID { get; set; }

    // Навигационные свойства
    [ForeignKey("TripTypeID")]
    [ValidateNever]
    public virtual TripType TripType { get; set; }

    [ForeignKey("RestrictionID")]
    [ValidateNever]
    public virtual DietaryRestriction? Restrictions { get; set; }

    public virtual ICollection<HikeProduct> HikeProducts { get; set; }

    // public List<Recipe> Recipes { get; set; } 
}