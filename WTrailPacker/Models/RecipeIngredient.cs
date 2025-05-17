using System;
using System.Collections.Generic;

namespace WTrailPacker.Models;

public partial class RecipeIngredient
{
    public int RecipeIngredientID { get; set; }

    public int? RecipeID { get; set; }

    public int? ProductID { get; set; }

    public decimal Quantity { get; set; }

    public string? Unit { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Recipe? Recipe { get; set; }
}
