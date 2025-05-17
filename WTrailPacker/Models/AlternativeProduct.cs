using System;
using System.Collections.Generic;

namespace WTrailPacker.Models;

public partial class AlternativeProduct
{
    public int OriginalProductID { get; set; }

    public int AlternativeProductID { get; set; }

    public int? CompatibilityScore { get; set; }

    public string? Notes { get; set; }

    public virtual Product AlternativeProductNavigation { get; set; } = null!;

    public virtual Product OriginalProduct { get; set; } = null!;
}
