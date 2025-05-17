using System;
using System.Collections.Generic;

namespace WTrailPacker.Models;

public partial class MealPlan
{
    public int PlanID { get; set; }

    public string PlanName { get; set; } = null!;

    public int TripTypeID { get; set; }

    public int DurationDays { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<MealPlanProduct> MealPlanProducts { get; set; } = new List<MealPlanProduct>();

    public virtual TripType TripType { get; set; } = null!;
    //public virtual ICollection<HikeProduct> HikeProducts { get; set; }
    public virtual List<Recipe> Recipes { get; set; }

}
