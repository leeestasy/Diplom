using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WTrailPacker.Models;
using System.Linq;
using System;
using WTrailPacker.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace WTrailPacker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TrailPackerDbContext _context;
        private readonly IFoodCalculationService _foodCalculationService;

        public HomeController(
            ILogger<HomeController> logger,
            TrailPackerDbContext context,
            IFoodCalculationService foodCalculationService)
            //IViewDataLoader @object,
            //IMealRecipeService object1,
            //IHikeResultSaver object2)
        {
            _logger = logger;
            _context = context;
            _foodCalculationService = foodCalculationService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Hike hike)
        {
            _logger.LogInformation("������ ������ Index (HttpPost)");

            // �������� TripTypeID
            if (hike.TripTypeID == 0)
            {
                ModelState.AddModelError("TripTypeID", "�������� ��� ������.");
            }
            else if (!await _context.TripTypes.AnyAsync(t => t.TripTypeID == hike.TripTypeID))
            {
                ModelState.AddModelError("TripTypeID", "�������� ���������� ��� ������");
            }

            if (ModelState.IsValid)
            {
                _context.Hikes.Add(hike);
                await _context.SaveChangesAsync();
                return RedirectToAction("ResultFirst", new { hikeId = hike.HikeID });
            }

            ViewBag.TripTypes = new SelectList(
                 await _context.TripTypes.ToListAsync(),
                 "TripTypeID",
                 "TypeName",
                 hike.TripTypeID // ��������� ��������� ��������
             );

            ViewBag.DietaryRestrictions = new SelectList(
                await _context.DietaryRestrictions.ToListAsync(),
                "RestrictionID",
                "RestrictionName",
                hike.RestrictionID
            );

            return View(await CreateHikeViewModelAsync(hike));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var tripTypes = await _context.TripTypes.ToListAsync();
                var dietaryRestrictions = await _context.DietaryRestrictions
                    .Select(r => new DietaryRestriction 
                    {
                        RestrictionID = r.RestrictionID,
                        RestrictionName = r.RestrictionName,
                        Description = r.Description
                    })
                    .ToListAsync();

                ViewBag.TripTypes = new SelectList(tripTypes, "TripTypeID", "TypeName");
                ViewBag.DietaryRestrictions = dietaryRestrictions; 

                return View(await CreateHikeViewModelAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "������");
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }

        private async Task<HikeViewModel> CreateHikeViewModelAsync(Hike hike = null)
        {
            return new HikeViewModel
            {
                Hike = hike ?? new Hike(),
                Recipes = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Product)
                    .ToListAsync(),
                TripTypes = await _context.TripTypes.ToListAsync(),
                DietaryRestrictions = await _context.DietaryRestrictions.ToListAsync()
            };
        }

        [HttpGet]
        public async Task<IActionResult> ResultFirst(int hikeId)
        {
            var hike = await _context.Hikes
                .Include(h => h.TripType)
                .Include(h => h.Restrictions)
                .Include(h => h.HikeProducts)
                    .ThenInclude(hp => hp.Product)
                .FirstOrDefaultAsync(h => h.HikeID == hikeId);

            if (hike == null) return NotFound();

            // ���� �������� ��� �� ����������
            if (!hike.HikeProducts.Any())
            {
                var calculationResult = _foodCalculationService.CalculateFood(hike);
                var hikeProducts = ((Dictionary<string, decimal>)calculationResult["totalFood"])
                    .Select(kvp => new HikeProduct
                    {
                        ProductID = _context.Products.First(p => p.ProductName == kvp.Key).ProductID,
                        Quantity = kvp.Value,
                        TotalWeight = kvp.Value / 1000m,
                        Packages = (int)Math.Ceiling((decimal)(kvp.Value / _context.Products
                            .First(p => p.ProductName == kvp.Key).WeightPerUnit))
                    }).ToList();

                hike.HikeProducts = hikeProducts;
                await _context.SaveChangesAsync();
            }

            return View(new FoodCalculationResult
            {
                Hike = hike,
                Products = (List<HikeProduct>)hike.HikeProducts,
                TotalFoodWeight = hike.HikeProducts.Sum(p => p.TotalWeight)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResultFirst(HikeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await ReloadViewBagsAsync(model.Hike);
                return View(model);
            }

            try
            {
                var calculationResult = _foodCalculationService.CalculateFood(model.Hike);

                var hikeProducts = ((Dictionary<string, decimal>)calculationResult["totalFood"])
                    .Select(kvp => new HikeProduct
                    {
                        ProductID = _context.Products.First(p => p.ProductName == kvp.Key).ProductID,
                        Quantity = kvp.Value,
                        TotalWeight = kvp.Value / 1000m,
                        HikeID = model.Hike.HikeID
                    }).ToList();

                var existingHike = await _context.Hikes
                    .Include(h => h.HikeProducts)
                    .FirstAsync(h => h.HikeID == model.Hike.HikeID);

                // ������� ������������ ���������
                existingHike.HikeProducts.Clear();

                // ���������� �����
                foreach (var hp in hikeProducts)
                {
                    existingHike.HikeProducts.Add(hp);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ResultFirst), new { hikeId = model.Hike.HikeID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "������ ����������");
                ModelState.AddModelError("", ex.Message);
                await ReloadViewBagsAsync(model.Hike);
                return View(model);
            }
        }

        // ��������������� ����� ��� �������� ��������� ������
        private async Task ReloadViewBagsAsync(Hike hike)
        {
            ViewBag.TripTypes = await _context.TripTypes.ToListAsync();
            ViewBag.Restrictions = await _context.DietaryRestrictions.ToListAsync();
            ViewBag.MealPlans = await _context.MealPlans
                .Include(mp => mp.MealPlanProducts)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int hikeId)
        {
            var hike = await _context.Hikes
                .Include(h => h.TripType)
                .Include(h => h.Restrictions) 
                .Include(h => h.HikeProducts)
                    .ThenInclude(hp => hp.Product)
                .FirstOrDefaultAsync(h => h.HikeID == hikeId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Products");

            // ��������� �������
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
            if (System.IO.File.Exists(imagePath))
            {
                var image = worksheet.AddPicture(imagePath)
                     .MoveTo((IXLCell)worksheet.Range("A1:A2")) 
                     .Scale(0.3);
            }

            // ����� ���������
            worksheet.Cell("B1").Value = "�������� ������ ���-�����������";
            worksheet.Cell("B2").Value = "��� ����������� '���������' ���������";
            worksheet.Cell("B3").Value = "�������������� ������";

            // ����������
            int startRow = 5;
            worksheet.Cell(startRow, 1).Value = "������� � ������:";
            worksheet.Cell(startRow, 2).Value = hike.NumPeople;
            worksheet.Cell(startRow + 1, 1).Value = "������������ ������:";
            worksheet.Cell(startRow + 1, 2).Value = hike.NumDays;
            worksheet.Cell(startRow + 2, 1).Value = "��� ������:";
            worksheet.Cell(startRow + 2, 2).Value = hike.TripType?.TypeName;
            worksheet.Cell(startRow + 3, 1).Value = "����������� �������:";
            worksheet.Cell(startRow + 3, 2).Value = hike.Restrictions?.RestrictionName ?? "���";

            // ��������� �������
            int tableStartRow = startRow + 5;
            worksheet.Cell(tableStartRow, 1).Value = "�������";
            worksheet.Cell(tableStartRow, 2).Value = "���������� ��������";
            worksheet.Cell(tableStartRow, 3).Value = "��� (��)";

            // ����� ��� ����������
            var headerStyle = workbook.Style;
            headerStyle.Font.Bold = true;
            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(tableStartRow, 1, tableStartRow, 3).Style = headerStyle;

            // ������
            int row = tableStartRow + 1;
            foreach (var hp in hike.HikeProducts)
            {
                worksheet.Cell(row, 1).Value = hp.Product.ProductName;
                worksheet.Cell(row, 2).Value = hp.Packages;
                worksheet.Cell(row, 3).Value = hp.TotalWeight;
                row++;
            }

            // ������������� ������ ��������
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"hike_{hikeId}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportToPDF(int hikeId)
        {
            var hike = await _context.Hikes
                .Include(h => h.TripType)
                .Include(h => h.Restrictions) 
                .Include(h => h.HikeProducts)
                    .ThenInclude(hp => hp.Product)
                .FirstOrDefaultAsync(h => h.HikeID == hikeId);

            var pdf = new Document(PageSize.A4, 20, 20, 40, 30);
            var memoryStream = new MemoryStream();
            PdfWriter.GetInstance(pdf, memoryStream);

            pdf.Open();

            // ������
            var baseFont = BaseFont.CreateFont(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "arial.ttf"),
                BaseFont.IDENTITY_H,
                BaseFont.EMBEDDED
            );
            var font = new Font(baseFont, 12);
            var boldFont = new Font(baseFont, 14, Font.BOLD);

            // �������
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");
            if (System.IO.File.Exists(imagePath))
            {
                var logo = Image.GetInstance(imagePath);
                logo.ScaleToFit(80f, 80f); // ������ ��������
                logo.Alignment = Image.ALIGN_LEFT;
                pdf.Add(logo);
            }

            // ��������� ���������
            var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
            headerTable.AddCell(new Phrase("�������� ������ ���-�����������\n��� ����������� '���������'\n��������� �������������� ������", boldFont));
            headerTable.AddCell(new Phrase(" ")); // ������ ������ ��� ������������
            pdf.Add(headerTable);

            // ����������
            var metadata = new PdfPTable(2)
            {
                WidthPercentage = 100,
                SpacingBefore = 20f
            };
            metadata.AddCell(new Phrase("������� � ������:", boldFont));
            metadata.AddCell(new Phrase(hike.NumPeople.ToString(), font));
            metadata.AddCell(new Phrase("������������ ������:", boldFont));
            metadata.AddCell(new Phrase(hike.NumDays.ToString(), font));
            metadata.AddCell(new Phrase("��� ������:", boldFont));
            metadata.AddCell(new Phrase(hike.TripType?.TypeName ?? "", font));
            metadata.AddCell(new Phrase("����������� �������:", boldFont));
            metadata.AddCell(new Phrase(hike.Restrictions?.RestrictionName ?? "���", font));
            pdf.Add(metadata);

            // ������� ���������
            pdf.Add(new Paragraph("\n"));
            var table = new PdfPTable(3)
            {
                WidthPercentage = 100,
                SpacingBefore = 20f
            };

            // ��������� �������
            table.AddCell(new Phrase("�������", boldFont));
            table.AddCell(new Phrase("���������� ��������", boldFont));
            table.AddCell(new Phrase("��� (��)", boldFont));

            // ������
            foreach (var hp in hike.HikeProducts)
            {
                table.AddCell(new Phrase(hp.Product.ProductName, font));
                table.AddCell(new Phrase(hp.Packages.ToString(), font));
                table.AddCell(new Phrase(hp.TotalWeight.ToString("0.00"), font));
            }

            pdf.Add(table);
            pdf.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"hike_{hikeId}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ResultSecond(int hikeId)
        {
            try
            {
                var hike = await _context.Hikes
                    .Include(h => h.TripType)
                    .Include(h => h.Restrictions)
                    .Include(h => h.HikeProducts)
                        .ThenInclude(hp => hp.Product)
                    .FirstOrDefaultAsync(h => h.HikeID == hikeId);

                if (hike == null) return NotFound();

                var mealPlan = await _context.MealPlans
                    .Include(mp => mp.MealPlanProducts)
                        .ThenInclude(mpp => mpp.Product)
                    .FirstOrDefaultAsync(mp => mp.TripTypeID == hike.TripTypeID);

                var model = new MealSchedule
                {
                    Days = Enumerable.Range(1, hike.NumDays).Select(day => new DayMeals
                    {
                        DayNumber = day,
                        Breakfast = GetMealRecipes(mealPlan, "Breakfast"),
                        Lunch = GetMealRecipes(mealPlan, "Lunch"),
                        Dinner = GetMealRecipes(mealPlan, "Dinner"),
                        Snacks = GetMealRecipes(mealPlan, "Snack")
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ResultSecond");
                return View("Error");
            }
        }

        private List<Recipe> GetMealRecipes(MealPlan mealPlan, string mealType)
        {
            if (mealPlan == null) return new List<Recipe>();

            var productIds = mealPlan.MealPlanProducts
                .Where(mpp => mpp.MealType == mealType)
                .Select(mpp => mpp.ProductID)
                .ToList();

            return _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Product)
                .Where(r => r.RecipeIngredients.Any(ri => productIds.Contains((int)ri.ProductID)))
                .Distinct()
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int hikeId)
        {
            var hike = await _context.Hikes
                .Include(h => h.TripType)
                .Include(h => h.Restrictions)
                .Include(h => h.HikeProducts)
                    .ThenInclude(hp => hp.Product)
                .FirstOrDefaultAsync(h => h.HikeID == hikeId);

            var pdf = new Document();
            var memoryStream = new MemoryStream();
            PdfWriter.GetInstance(pdf, memoryStream);

            pdf.Open();

            // ������ � �����
            var baseFont = BaseFont.CreateFont(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"),
                BaseFont.IDENTITY_H,
                BaseFont.EMBEDDED
            );
            var font = new Font(baseFont, 12);
            var boldFont = new Font(baseFont, 14, Font.BOLD);

            // ���������
            pdf.Add(new Paragraph($"���� ������� ��� ������ #{hikeId}", boldFont));
            pdf.Add(new Paragraph($"������: {hike.NumPeople} ���., ����: {hike.NumDays}", font));
            pdf.Add(Chunk.Newline);

            // ��������� �������� �� ����
            for (int day = 1; day <= hike.NumDays; day++)
            {
                pdf.Add(new Paragraph($"���� {day}", boldFont));

                var table = new PdfPTable(4) { WidthPercentage = 100 };
                table.AddCell(new Phrase("�������", font));
                table.AddCell(new Phrase("����", font));
                table.AddCell(new Phrase("�������", font));
                table.AddCell(new Phrase("����", font));

                pdf.Add(table);
                pdf.Add(Chunk.Newline);
            }

            pdf.Close();
            return File(memoryStream.ToArray(), "application/pdf", $"meal_plan_{hikeId}.pdf");
        }

        private async Task SaveHikeResultsAsync(Hike hike, List<HikeProduct> products)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (hike.HikeID == 0)
                {
                    await _context.Hikes.AddAsync(hike);
                    await _context.SaveChangesAsync();
                }

                var existingProducts = await _context.HikeProducts
                    .Where(hp => hp.HikeID == hike.HikeID)
                    .ToListAsync();

                if (existingProducts.Any())
                {
                    _context.HikeProducts.RemoveRange(existingProducts);
                    await _context.SaveChangesAsync();
                }

                foreach (var product in products)
                {
                    var dbProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductID == product.ProductID);

                    if (dbProduct == null)
                    {
                        _logger.LogWarning($"������� � ID {product.ProductID} �� ������.");
                        continue;
                    }

                    var hikeProduct = new HikeProduct
                    {
                        HikeID = hike.HikeID,
                        ProductID = product.ProductID,
                        Quantity = product.Quantity,
                        UnitType = dbProduct.UnitType ?? "�",
                        WeightPerUnit = (decimal)dbProduct.WeightPerUnit,
                        TotalWeight = product.TotalWeight,
                        CaloriesPer100g = dbProduct.CaloriesPer100g,
                        CategoryName = dbProduct.Category?.CategoryName ?? "������",
                        IsPerishable = dbProduct.IsPerishable ?? false,
                        IsSublimated = dbProduct.IsSublimated ?? false,
                        ShelfLifeDays = dbProduct.ShelfLifeDays ?? 0
                    };

                    await _context.HikeProducts.AddAsync(hikeProduct);
                    _logger.LogInformation($"�������� �������: {hikeProduct.ProductID}");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "������ ���������� �����������");
                throw;
            }
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<HikeViewModel> GetHikeViewModelAsync(string searchTerm)
        {
            var recipesQuery = _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Product).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                recipesQuery = recipesQuery.Where(r => r.RecipeName.Contains(searchTerm));
            }

            var recipes = await recipesQuery.ToListAsync();

            return new HikeViewModel
            {
                Hike = new Hike
                {
                    RestrictionID = null,
                    HikeProducts = new List<HikeProduct>()
                },
                Recipes = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Product)
                    .ToListAsync(),
            };
        }

        public async Task<IActionResult> Recipes(string searchTerm = null)
        {
            var query = _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r =>
                    r.RecipeName != null &&
                    r.RecipeName.Contains(searchTerm)
                );
            }

            var recipes = await query.ToListAsync();

            return View(new HikeViewModel { Recipes = recipes });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecipesPost(string searchTerm)
        {
            var viewModel = await GetHikeViewModelAsync(searchTerm);
            return View("Recipes", viewModel);
        }

        public IActionResult RecipeView(int id)
        {
            var recipe = _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Product)
                .FirstOrDefault(r => r.RecipeID == id);

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}