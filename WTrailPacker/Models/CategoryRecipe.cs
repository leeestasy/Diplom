using System;
using System.Collections.Generic;

namespace WTrailPacker.Models;

public partial class CategoryRecipe
{
    public int CategoryRecipeID { get; set; }

    public string CategoryRecipeName { get; set; } = null!;

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
