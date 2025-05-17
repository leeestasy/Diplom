using System;
using System.Collections.Generic;

namespace WTrailPacker.Models;

public partial class TripType
{
    public int TripTypeID { get; set; }

    public string TypeName { get; set; } = null!;

    public string? Description { get; set; }

    public int BaseCaloriesPerDay { get; set; }

    public int WaterRequirementML { get; set; }

    public virtual ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();
    public virtual ICollection<Hike> Hikes { get; set; } = new List<Hike>();

}
