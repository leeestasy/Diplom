using System;
using System.Collections.Generic;

namespace WTrailPacker.Models;

public partial class DietaryRestriction
{
    public int RestrictionID { get; set; }

    public string RestrictionName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Hike> Hikes { get; set; } = new List<Hike>();
    public ICollection<ProductRestriction> ProductRestrictions { get; set; }

}
