using System;
using System.Collections.Generic;

namespace WTrailPacker.Models;

public partial class Product
{
    public int ProductID { get; set; }

    public string ProductName { get; set; } = null!;

    public int CategoryID { get; set; }

    public decimal CaloriesPer100g { get; set; }

    public decimal ProteinPer100g { get; set; }

    public decimal CarbsPer100g { get; set; }

    public decimal FatPer100g { get; set; }

    public decimal? WeightPerUnit { get; set; }

    public string? UnitType { get; set; }

    public bool? IsSublimated { get; set; }

    public bool? IsPerishable { get; set; }

    public int? ShelfLifeDays { get; set; }

    public virtual ICollection<AlternativeProduct> AlternativeProductAlternativeProductNavigations { get; set; } = new List<AlternativeProduct>();

    public virtual ICollection<AlternativeProduct> AlternativeProductOriginalProducts { get; set; } = new List<AlternativeProduct>();

    public virtual FoodCategory Category { get; set; } = null!;

    public virtual ICollection<MealPlanProduct> MealPlanProducts { get; set; } = new List<MealPlanProduct>();

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

    public virtual ICollection<DietaryRestriction> Restrictions { get; set; } = new List<DietaryRestriction>();

    public ICollection<ProductRestriction> ProductRestrictions { get; set; }

    public virtual ICollection<HikeProduct> HikeProducts { get; set; } = new List<HikeProduct>();
}

public class ProductRecommendation
{
    public required string ProductName { get; set; }
    public required string CategoryName { get; set; }
    public decimal TotalQuantity { get; set; }
    public int Packages { get; set; }
    public decimal? WeightPerUnit { get; set; }
    public decimal TotalWeight { get; set; }
    public required string UnitType { get; set; }
}