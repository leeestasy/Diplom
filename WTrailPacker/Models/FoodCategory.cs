using System;
using System.Collections.Generic;

namespace WTrailPacker.Models;

public partial class FoodCategory
{
    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
