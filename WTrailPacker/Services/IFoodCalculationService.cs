using WTrailPacker.Models;

namespace WTrailPacker.Services
{
    public interface IFoodCalculationService
    {
        //FoodCalculationResult CalculateFoodRequirements(Hike hike);
        //List<MealPlan> GetAvailableMealPlans(int tripTypeId);
        //List<HikeProduct> CalculateRequiredProducts(MealPlan mealPlan, int numPeople, int numDays);
        //List<HikeProduct> ApplyDietaryRestrictions(List<HikeProduct> products, int restrictionId);
        //void CalculatePackagingInfo(List<HikeProduct> products);
        Dictionary<string, object> CalculateFood(Hike hike);

    }
    //public interface IViewDataLoader
    //{
    //    Task ReloadViewBagsAsync(Hike hike);
    //}

    //public interface IMealRecipeService
    //{
    //    List<Recipe> GetMealRecipes(MealPlan mealPlan, string mealType);
    //}

    //public interface IHikeResultSaver
    //{
    //    Task SaveHikeResultsAsync(Hike hike, List<HikeProduct> products);
    //}
}
