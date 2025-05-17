namespace WTrailPacker.Models
{
    public class HikeViewModel
    {
        public HikeViewModel()
        {
            //Hike = new Hike
            //{
            //    //TripTypeID = 0,
            //    //HikeProducts = new List<HikeProduct>(), // или другое значение по умолчанию
            //    RestrictionID = null
            //};
            Hike = new Hike();
            Recipes = new List<Recipe>();
            Breakfast = new List<List<string>>();
            Lunch = new List<List<string>>();
            Dinner = new List<List<string>>();
            Snacks = new List<List<string>>();
        }

        public Hike Hike { get; set; }
        public List<TripType> TripTypes { get; set; }
        public List<DietaryRestriction> DietaryRestrictions { get; set; }
        public List<Recipe> Recipes { get; set; }
        public List<RecipeIngredient> RecipeIngredients { get; set; } = new();
        public List<List<string>> Breakfast { get; set; }
        public List<List<string>> Lunch { get; set; }
        public List<List<string>> Dinner { get; set; }
        public List<List<string>> Snacks { get; set; }
    }
}