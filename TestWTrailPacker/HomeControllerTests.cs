using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using WTrailPacker.Controllers;
using WTrailPacker.Models;
using WTrailPacker.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace TestWTrailPacker
{
    public class HomeControllerTests : IDisposable
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger = new();
        private readonly Mock<IFoodCalculationService> _mockFoodService = new();
        private readonly TrailPackerDbContext _context;
        private readonly Mock<IViewDataLoader> _mockViewDataLoader = new();
        private readonly Mock<IMealRecipeService> _mockMealRecipeService = new();
        private readonly Mock<IHikeResultSaver> _mockHikeResultSaver = new();

        public HomeControllerTests()
        {
            var options = new DbContextOptionsBuilder<TrailPackerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new TrailPackerDbContext(options);
            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.TripTypes.Add(new TripType { TripTypeID = 1, TypeName = "TestType" });
            _context.DietaryRestrictions.Add(new DietaryRestriction { RestrictionID = 1, RestrictionName = "TestRestriction" });
            _context.Products.Add(new Product { ProductID = 1, ProductName = "TestProduct", WeightPerUnit = 100 });
            _context.SaveChanges();

        }

        public void Dispose() => _context.Dispose();

        private HomeController CreateController(TrailPackerDbContext context = null)
        {
            return new HomeController(
                _mockLogger.Object,
                context ?? _context,
                _mockFoodService.Object,
                _mockViewDataLoader.Object,
                _mockMealRecipeService.Object,
                _mockHikeResultSaver.Object
            );
        }

        [Fact]
        public async Task RecipeView_ValidId_ReturnsView()
        {
            // Arrange
            _context.Recipes.Add(new Recipe
            {
                RecipeID = 1,
                RecipeName = "Test Recipe",
                RecipeIngredients = new List<RecipeIngredient>
                {
                    new RecipeIngredient { ProductID = 1 }
                }
            });
            await _context.SaveChangesAsync();

            var controller = CreateController();

            // Act
            var result = controller.RecipeView(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public void RecipeView_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.RecipeView(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }


        [Fact]
        public async Task RecipesPost_EmptySearch_ReturnsAllRecipes()
        {
            // Arrange
            _context.Recipes.AddRange(
                new Recipe { RecipeID = 1, RecipeName = "Recipe 1" },
                new Recipe { RecipeID = 2, RecipeName = "Recipe 2" }
            );
            await _context.SaveChangesAsync();

            var controller = CreateController();

            // Act
            var result = await controller.RecipesPost("");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HikeViewModel>(viewResult.Model);
            Assert.Equal(2, model.Recipes.Count);
        }

        [Fact]
        public void Privacy_ReturnsView()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsCorrectView()
        {
            // Arrange
            var controller = CreateController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(model.RequestId);
            Assert.True(model.ShowRequestId); 
        }

        [Fact]
        public async Task ResultFirst_Post_InvalidModel_ReloadsView()
        {
            // Arrange
            var invalidHike = new Hike { HikeID = 1, NumPeople = -1 };
            var controller = CreateController();

            // Act
            var result = await controller.ResultFirst(new HikeViewModel { Hike = invalidHike });

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(viewResult.ViewData.ModelState.IsValid);
        }

        [Fact]
        public async Task ResultFirst_Post_ExceptionHandling()
        {
            // Arrange
            _mockFoodService.Setup(s => s.CalculateFood(It.IsAny<Hike>()))
                .Throws(new Exception("Test exception"));

            var controller = CreateController();

            // Act
            var result = await controller.ResultFirst(new HikeViewModel { Hike = new Hike() });

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task Index_Post_InvalidTripTypeID_ReturnsViewWithErrors()
        {
            // Arrange
            var invalidHike = new Hike { TripTypeID = 999 };
            var controller = CreateController();

            // Act
            var result = await controller.Index(invalidHike);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(viewResult.ViewData.ModelState.IsValid);
        }

        [Fact]
        public async Task Index_Get_ReturnsViewWithValidSelectLists()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<SelectList>(viewResult.ViewData["TripTypes"]);
            Assert.IsType<List<DietaryRestriction>>(viewResult.ViewData["DietaryRestrictions"]);
        }

        [Fact]
        public async Task Index_Get_ReturnsViewWithData()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["TripTypes"]);
            Assert.NotNull(viewResult.ViewData["DietaryRestrictions"]);
        }

        [Fact]
        public async Task Index_Post_ValidModel_RedirectsToResultFirst()
        {
            // Arrange
            var hike = new Hike { TripTypeID = 1, NumPeople = 2, NumDays = 3 };
            var controller = CreateController();

            // Act
            var result = await controller.Index(hike);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ResultFirst", redirectResult.ActionName);
            Assert.Single(_context.Hikes);
        }

        [Fact]
        public async Task ResultFirst_Get_CalculatesProducts()
        {
            // Arrange
            var hike = new Hike { HikeID = 1, TripTypeID = 1 };
            _context.Hikes.Add(hike);
            await _context.SaveChangesAsync();

            _mockFoodService.Setup(s => s.CalculateFood(It.IsAny<Hike>()))
                .Returns(new Dictionary<string, object>
                {
                    ["totalFood"] = new Dictionary<string, decimal> { { "TestProduct", 1000 } }
                });

            var controller = CreateController();

            // Act
            var result = await controller.ResultFirst(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, _context.HikeProducts.Count());
        }

        [Fact]
        public async Task ExportToExcel_ReturnsFile()
        {
            // Arrange
            using var separateContext = CreateNewContext();
            var tripType = new TripType { TripTypeID = 1, TypeName = "TestType" };
            var product = new Product { ProductID = 1, ProductName = "TestProduct", WeightPerUnit = 100 };
            var restriction = new DietaryRestriction { RestrictionID = 1, RestrictionName = "TestRestriction" };

            var hike = new Hike
            {
                HikeID = 1,
                TripTypeID = 1,
                NumPeople = 2,
                NumDays = 3,
                RestrictionID = 1
            };

            separateContext.AddRange(tripType, restriction, product);
            separateContext.Add(hike);
            separateContext.Add(new HikeProduct
            {
                HikeID = 1,
                ProductID = 1,
                Packages = 2,
                TotalWeight = 0.2m,
                Product = product
            });

            await separateContext.SaveChangesAsync();

            var controller = CreateController(separateContext);

            // Act
            var result = await controller.ExportToExcel(1);

            // Assert
            Assert.IsType<FileContentResult>(result);
        }

        private TrailPackerDbContext CreateNewContext()
        {
            var options = new DbContextOptionsBuilder<TrailPackerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TrailPackerDbContext(options);
        }

        [Fact]
        public async Task Recipes_ReturnsFilteredResults()
        {
            // Arrange
            _context.Recipes.Add(new Recipe { RecipeID = 1, RecipeName = "TestRecipe" });
            await _context.SaveChangesAsync();

            var controller = CreateController();

            // Act
            var result = await controller.Recipes("Test");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HikeViewModel>(viewResult.Model);
            Assert.Single(model.Recipes);
        }

        [Fact]
        public async Task CalculateFood_NegativePeople_ReturnsError()
        {
            var controller = CreateController();
            var hike = new Hike { NumPeople = -2, NumDays = 3 };

            var result = await controller.Index(hike);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(viewResult.ViewData.ModelState.IsValid);
        }



        [Fact]
        public async Task RecipesSearch_NonExistingTerm_ReturnsEmpty()
        {
            _context.Recipes.Add(new Recipe { RecipeName = "TestRecipe" });
            await _context.SaveChangesAsync();

            var controller = CreateController();
            var result = await controller.Recipes("NonExisting");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HikeViewModel>(viewResult.Model);
            Assert.Empty(model.Recipes);
        }


        [Fact]
        public async Task IndexPost_InvalidRestriction_ReturnsError()
        {
            var controller = CreateController();
            var hike = new Hike { RestrictionID = 999 };

            var result = await controller.Index(hike);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(viewResult.ViewData.ModelState.IsValid);
        }
    }
}