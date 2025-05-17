using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using WTrailPacker.Controllers;
using WTrailPacker.Models;

namespace WTrailPacker.Services
{
    public class FoodCalculationService : IFoodCalculationService
    {
        private readonly TrailPackerDbContext _dbContext;

        public FoodCalculationService(TrailPackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Dictionary<string, object> CalculateFood(Hike hike)
        {
            var result = new Dictionary<string, object>();

            var tripType = _dbContext.TripTypes.FirstOrDefault(tt => tt.TripTypeID == hike.TripTypeID);
            if (tripType == null)
                throw new ArgumentException("Invalid Trip Type ID");

            result["totalDays"] = hike.NumDays;
            result["totalPeople"] = hike.NumPeople;
            result["tourType"] = tripType.TypeName;
            result["baseCaloriesPerDay"] = tripType.BaseCaloriesPerDay;

            var mealPlan = _dbContext.MealPlans
                .Include(mp => mp.MealPlanProducts)
                .FirstOrDefault(mp => mp.TripTypeID == hike.TripTypeID);

            if (mealPlan == null)
                throw new InvalidOperationException("No meal plan found for the specified trip type.");


            var mealPlanProducts = _dbContext.MealPlanProducts
            .Where(mp => mp.PlanID == tripType.TripTypeID && mp.DayNumber <= hike.NumDays)
                .Include(mpp => mpp.Product)
                .ToList();

            if (hike.RestrictionID.HasValue)
            {
                var restrictedProductIds = _dbContext.ProductRestrictions
                    .Where(pr => pr.RestrictionID == hike.RestrictionID)
                    .Select(pr => pr.ProductID)
                    .ToList();

                var adjustedMealPlanProducts = new List<MealPlanProduct>();
                foreach (var mpp in mealPlanProducts)
                {
                    if (restrictedProductIds.Contains(mpp.ProductID))
                    {
                        var alternative = _dbContext.AlternativeProducts
                            .Where(ap => ap.OriginalProductID == mpp.ProductID)
                            .OrderByDescending(ap => ap.CompatibilityScore)
                            .FirstOrDefault();

                        if (alternative != null)
                        {
                            var alternativeProduct = _dbContext.Products.Find(alternative.AlternativeProductID);
                            if (alternativeProduct != null)
                            {
                                adjustedMealPlanProducts.Add(new MealPlanProduct
                                {
                                    ProductID = alternativeProduct.ProductID,
                                    QuantityPerPerson = mpp.QuantityPerPerson,
                                    DayNumber = mpp.DayNumber,
                                    MealType = mpp.MealType
                                });
                            }
                        }
                    }
                    else
                    {
                        adjustedMealPlanProducts.Add(mpp);
                    }
                }
                mealPlanProducts = adjustedMealPlanProducts;
            }

            var totalFood = new Dictionary<string, decimal>();
            var packageCounts = new Dictionary<string, int>();
            decimal totalWeightKg = 0;

            foreach (var mpp in mealPlanProducts)
            {
                var product = _dbContext.Products.FirstOrDefault(p => p.ProductID == mpp.ProductID);
                if (product == null) continue;

                decimal totalQuantity = mpp.QuantityPerPerson * hike.NumPeople * (mpp.DayNumber.HasValue ? 1 : hike.NumDays);
                if (product.WeightPerUnit <= 0)
                    continue;

                int packages = (int)Math.Ceiling((double)(totalQuantity / product.WeightPerUnit));
                decimal totalWeight = totalQuantity / 1000m;

                if (totalFood.ContainsKey(product.ProductName))
                {
                    totalFood[product.ProductName] += totalQuantity;
                    packageCounts[product.ProductName] += packages;
                }
                else
                {
                    totalFood[product.ProductName] = totalQuantity;
                    packageCounts[product.ProductName] = packages;
                }

                totalWeightKg += totalWeight;
            }

            result["totalFood"] = totalFood;
            result["packageCounts"] = packageCounts;
            result["totalWeightKg"] = totalWeightKg;

            var mealsByDay = GenerateMealStructure(mealPlanProducts, hike.NumDays);
            result["meals"] = mealsByDay;

            return result;
        }

        private Dictionary<int, Dictionary<string, List<string>>> GenerateMealStructure(List<MealPlanProduct> mealPlanProducts, int numDays)
        {
            if (mealPlanProducts == null || numDays <= 0)
            {
                return new Dictionary<int, Dictionary<string, List<string>>>();
            }

            var mealsByDay = new Dictionary<int, Dictionary<string, List<string>>>();

            // Логирование проблемных записей
            var problematicEntries = mealPlanProducts.Where(mpp => mpp.Product == null).ToList();
            if (problematicEntries.Any())
            {
                //_logger.LogWarning($"Found {problematicEntries.Count} meal plan products without linked Product");
            }

            for (int day = 1; day <= numDays; day++)
            {
                var dailyMeals = new Dictionary<string, List<string>>();

                var productsForDay = mealPlanProducts
                    .Where(mpp => (!mpp.DayNumber.HasValue || mpp.DayNumber == day)
                               && mpp.Product != null)
                    .GroupBy(mpp => mpp.MealType)
                    .ToDictionary(
                        g => g.Key ?? "General",
                        g => g.Select(mpp => mpp.Product?.ProductName ?? "Unknown Product")
                              .Where(name => !string.IsNullOrEmpty(name)) // Дополнительная фильтрация
                              .ToList()
                    );

                foreach (var mealType in productsForDay)
                {
                    dailyMeals[mealType.Key] = mealType.Value;
                }

                mealsByDay[day] = dailyMeals;
            }

            return mealsByDay;
        }
    }
}
// Расписание питания
public class MealSchedule
{
    public List<DayMeals> Days { get; set; } = new List<DayMeals>(); // Питание по дням
    public Hike Hike { get; set; }

}

// Питание на один день 
public class DayMeals
{
    public int DayNumber { get; set; } // Номер дня
    public List<Recipe> Breakfast { get; set; } = new List<Recipe>(); // Завтрак
    public List<Recipe> Lunch { get; set; } = new List<Recipe>(); // Обед
    public List<Recipe> Dinner { get; set; } = new List<Recipe>(); // Ужин
    public List<Recipe> Snacks { get; set; } = new List<Recipe>(); // Перекусы
}