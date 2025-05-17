using System;
using System.Collections.Generic;

namespace WTrailPacker.Models;

public partial class Recipe
{
    public int RecipeID { get; set; }

    public string RecipeName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Instructions { get; set; }

    public int? CategoryRecipeID { get; set; }

    public virtual CategoryRecipe? CategoryRecipe { get; set; }

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    public int? PlanID { get;  set; }
    public virtual MealPlan MealPlan { get; set; }

}
