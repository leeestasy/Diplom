using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WTrailPacker.Models;

public partial class TrailPackerDbContext : DbContext
{
    public TrailPackerDbContext()
    {
    }

    public TrailPackerDbContext(DbContextOptions<TrailPackerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AlternativeProduct> AlternativeProducts { get; set; }

    public virtual DbSet<CategoryRecipe> CategoryRecipes { get; set; }

    public virtual DbSet<DietaryRestriction> DietaryRestrictions { get; set; }

    public virtual DbSet<FoodCategory> FoodCategories { get; set; }

    public virtual DbSet<Hike> Hikes { get; set; }

    public virtual DbSet<HikeProduct> HikeProducts { get; set; }

    public virtual DbSet<MealPlan> MealPlans { get; set; }

    public virtual DbSet<MealPlanProduct> MealPlanProducts { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductRestriction> ProductRestrictions { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeIngredient> RecipeIngredients { get; set; }

    public virtual DbSet<TripType> TripTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { 
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        //=> optionsBuilder.UseSqlServer("Server=DESKTOP-2250EVR\\SQLEXPRESS;Database=WTrailPacker;Trusted_Connection=True;TrustServerCertificate=True;");
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-2250EVR\\SQLEXPRESS;Database=WTrailPacker;Trusted_Connection=True;TrustServerCertificate=True;");
        }
}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlternativeProduct>(entity =>
        {
            entity.HasKey(e => new { e.OriginalProductID, e.AlternativeProductID }).HasName("PK__Alternat__01A43D6CB0914440");

            entity.ToTable("AlternativeProduct");

            entity.Property(e => e.CompatibilityScore).HasDefaultValue(100);
            entity.Property(e => e.Notes).HasMaxLength(255);

            entity.HasOne(d => d.AlternativeProductNavigation).WithMany(p => p.AlternativeProductAlternativeProductNavigations)
                .HasForeignKey(d => d.AlternativeProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Alternati__Alter__3B75D760");

            entity.HasOne(d => d.OriginalProduct).WithMany(p => p.AlternativeProductOriginalProducts)
                .HasForeignKey(d => d.OriginalProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Alternati__Origi__3A81B327");
        });

        modelBuilder.Entity<CategoryRecipe>(entity =>
        {
            entity.HasKey(e => e.CategoryRecipeID).HasName("PK__Category__4A40FF87A8E9FFD1");

            entity.ToTable("CategoryRecipe");

            entity.Property(e => e.CategoryRecipeName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DietaryRestriction>(entity =>
        {
            entity.HasKey(e => e.RestrictionID).HasName("PK__DietaryR__529D869AE38FDF3D");

            entity.ToTable("DietaryRestriction");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RestrictionName).HasMaxLength(50);
        });

        modelBuilder.Entity<FoodCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryID).HasName("PK__FoodCate__19093A2BC9A4935D");

            entity.ToTable("FoodCategory");

            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Hike>(entity =>
        {
            // Установка первичного ключа
            entity.HasKey(e => e.HikeID)
                  .HasName("PK_Hike");

            // Настройка таблицы
            entity.ToTable("Hike");

            // Валидационные правила
            entity.Property(e => e.NumDays)
                  .IsRequired()
                  .HasAnnotation("Range", new { Minimum = 1, Maximum = 365, ErrorMessage = "Длительность похода от 1 до 365 дней" });

            entity.Property(e => e.NumPeople)
                  .IsRequired()
                  .HasAnnotation("Range", new { Minimum = 1, Maximum = 30, ErrorMessage = "Группа от 1 до 30 человек" });

            entity.Property(e => e.TripTypeID)
                  .IsRequired();

            // Связь с TripType
            entity.HasOne(d => d.TripType)
                  .WithMany(p => p.Hikes)
                  .HasForeignKey(d => d.TripTypeID)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_Hike_TripType");

            // Связь с DietaryRestriction
            entity.HasOne(d => d.Restrictions)
                  .WithMany(p => p.Hikes)
                  .HasForeignKey(d => d.RestrictionID)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_Hike_DietaryRestriction");
        });

        modelBuilder.Entity<HikeProduct>(entity =>
        {
            entity.HasKey(e => e.HikeProductID)
                .HasName("PK_HikeProduct");

            entity.Property(e => e.CategoryName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Quantity)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            entity.Property(e => e.WeightPerUnit)
                .IsRequired() 
                .HasColumnType("decimal(10,2)");

            entity.Property(e => e.CaloriesPer100g)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            entity.Property(e => e.TotalWeight)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            entity.Property(e => e.TotalCalories)
                .IsRequired() 
                .HasColumnType("decimal(10,2)");

            entity.Property(e => e.OriginalProductName)
                .HasMaxLength(100);

            // Внешние ключи
            entity.HasOne(e => e.Hike)
                .WithMany(p => p.HikeProducts)
                .HasForeignKey(e => e.HikeID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HikeProduct_Hike");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.HikeProducts)
                .HasForeignKey(e => e.ProductID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_HikeProduct_Product");

            entity.HasIndex(e => e.HikeID).HasName("IX_HikeProduct_HikeID");
            entity.HasIndex(e => e.ProductID).HasName("IX_HikeProduct_ProductID");
            entity.HasIndex(e => e.IsAlternative).HasName("IX_HikeProduct_IsAlternative");
        });

        modelBuilder.Entity<MealPlan>(entity =>
        {
            entity.HasKey(e => e.PlanID).HasName("PK__MealPlan__755C22D7D55BB390");

            entity.ToTable("MealPlan");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.PlanName).HasMaxLength(100);

            entity.HasOne(d => d.TripType).WithMany(p => p.MealPlans)
                .HasForeignKey(d => d.TripTypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MealPlan__TripTy__32E0915F");
        });

        modelBuilder.Entity<MealPlanProduct>(entity =>
        {
            entity.HasKey(e => new { e.PlanID, e.ProductID }).HasName("PK__MealPlan__BE1CEEB914835107");

            entity.ToTable("MealPlanProduct");

            entity.Property(e => e.MealType).HasMaxLength(20);
            entity.Property(e => e.QuantityPerPerson).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Plan).WithMany(p => p.MealPlanProducts)
                .HasForeignKey(d => d.PlanID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MealPlanP__PlanI__35BCFE0A");

            entity.HasOne(d => d.Product).WithMany(p => p.MealPlanProducts)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MealPlanP__Produ__36B12243");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductID).HasName("PK__Product__B40CC6ED3D1C8417");

            entity.ToTable("Product");

            entity.Property(e => e.CaloriesPer100g).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CarbsPer100g).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FatPer100g).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsPerishable).HasDefaultValue(false);
            entity.Property(e => e.IsSublimated).HasDefaultValue(false);
            entity.Property(e => e.ProductName).HasMaxLength(100);
            entity.Property(e => e.ProteinPer100g).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitType).HasMaxLength(20);
            entity.Property(e => e.WeightPerUnit).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__Categor__2A4B4B5E");

            //entity.HasMany(d => d.Restrictions).WithMany(p => p.Products)
            //    .UsingEntity<Dictionary<string, object>>(
            //        "ProductRestriction",
            //        r => r.HasOne<DietaryRestriction>().WithMany()
            //            .HasForeignKey("RestrictionID")
            //            .OnDelete(DeleteBehavior.ClientSetNull)
            //            .HasConstraintName("FK__ProductRe__Restr__2E1BDC42"),
            //        l => l.HasOne<Product>().WithMany()
            //            .HasForeignKey("ProductID")
            //            .OnDelete(DeleteBehavior.ClientSetNull)
            //            .HasConstraintName("FK__ProductRe__Produ__2D27B809"),
            //        j =>
            //        {
            //            j.HasKey("ProductID", "RestrictionID").HasName("PK__ProductR__01251E841EB1FD11");
            //            j.ToTable("ProductRestriction");
            //        });
            // Настройка связи многие-ко-многим
            entity.HasMany(p => p.Restrictions)
                .WithMany(d => d.Products)
                .UsingEntity<ProductRestriction>(
                    j => j
                        .HasOne(pr => pr.Restriction)
                        .WithMany(d => d.ProductRestrictions)
                        .HasForeignKey(pr => pr.RestrictionID)
                        .OnDelete(DeleteBehavior.Cascade),

                    j => j
                        .HasOne(pr => pr.Product)
                        .WithMany(p => p.ProductRestrictions)
                        .HasForeignKey(pr => pr.ProductID)
                        .OnDelete(DeleteBehavior.Cascade),

                    j =>
                    {
                        j.ToTable("ProductRestriction");
                        j.HasKey(pr => new { pr.ProductID, pr.RestrictionID });
                    }
                );
        });

        modelBuilder.Entity<ProductRestriction>(entity =>
        {
            entity.ToTable("ProductRestriction");

            entity.HasKey(pr => new { pr.ProductID, pr.RestrictionID })
                .HasName("PK_ProductRestriction");

            // Настройка отношений (можно опустить если уже настроено через UsingEntity)
            entity.HasOne(pr => pr.Product)
                .WithMany(p => p.ProductRestrictions)
                .HasForeignKey(pr => pr.ProductID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pr => pr.Restriction)
                .WithMany(d => d.ProductRestrictions)
                .HasForeignKey(pr => pr.RestrictionID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        //modelBuilder.Entity<ProductRestriction>(entity =>
        //{
        //    entity.ToTable("ProductRestriction");
        //    // Составной первичный ключ
        //    entity.HasKey(e => new { e.ProductID, e.RestrictionID })
        //          .HasName("PK_ProductRestriction");

        //    // Внешний ключ к таблице Product
        //    entity.HasOne(d => d.Product)
        //          .WithMany(p => p.ProductRestrictions)
        //          .HasForeignKey(d => d.ProductID)
        //          .OnDelete(DeleteBehavior.Cascade)
        //          .HasConstraintName("FK_ProductRestriction_Product");

        //    // Внешний ключ к таблице DietaryRestriction
        //    entity.HasOne(d => d.Restriction)
        //          .WithMany(p => p.ProductRestrictions)
        //          .HasForeignKey(d => d.RestrictionID)
        //          .OnDelete(DeleteBehavior.Cascade)
        //          .HasConstraintName("FK_ProductRestriction_DietaryRestriction");
        //});

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.RecipeID).HasName("PK__Recipe__FDD988D0278E4884");

            entity.ToTable("Recipe");

            entity.Property(e => e.RecipeName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.CategoryRecipe).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.CategoryRecipeID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__RecipeIng__Recip__44FF419A");

            entity.HasOne(d => d.MealPlan).WithMany(p => p.Recipes)
               .HasForeignKey(d => d.PlanID)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_Recipe_MealPlan");
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(e => e.RecipeIngredientID).HasName("PK__RecipeIn__A2C3427681EFAB98");

            entity.ToTable("RecipeIngredient");

            entity.Property(e => e.Quantity).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Product).WithMany(p => p.RecipeIngredients)
                .HasForeignKey(d => d.ProductID)
                .HasConstraintName("FK__RecipeIng__Produ__45F365D3");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeIngredients)
                .HasForeignKey(d => d.RecipeID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__RecipeIng__Recip__44FF419A");
        });

        modelBuilder.Entity<TripType>(entity =>
        {
            entity.HasKey(e => e.TripTypeID).HasName("PK__TripType__06A5B58317187F22");

            entity.ToTable("TripType");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
