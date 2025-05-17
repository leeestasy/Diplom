using System;
using System.Collections.Generic;

namespace WTrailPacker.Models;

public partial class MealPlanProduct
{
    public int PlanID { get; set; }

    public int ProductID { get; set; }

    public decimal QuantityPerPerson { get; set; }

    public int? DayNumber { get; set; }

    public string? MealType { get; set; }

    public virtual MealPlan Plan { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

}
