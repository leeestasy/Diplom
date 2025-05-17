namespace WTrailPacker.Models
{
    // Результат расчета продуктов для похода
public class FoodCalculationResult
{
         public FoodCalculationResult()
        {
            Products = new List<HikeProduct>();
        }

    public int HikeID { get; set; }
    public int TotalPeople { get; set; }
    public int TotalDays { get; set; }
    public string TripType { get; set; }
    public string MealPlanName { get; set; }
    public int BaseCaloriesPerDay { get; set; }
    public int WaterRequirementML { get; set; }
    public string DietaryRestriction { get; set; }
    public decimal TotalFoodWeight { get; set; } // кг
    public int TotalPackages { get; set; }
    public decimal TotalCalories { get; set; }
        public List<HikeProduct> Products { get; set; } = new List<HikeProduct>();
        //public List<ProductViewModel> Products { get ; set ; }
        public Dictionary<int, Dictionary<string, List<string>>> MealsByDay { get; set; }
        public MealSchedule MealSchedule { get; set; } = new MealSchedule();
    public Hike Hike { get;  set; }
}

}
