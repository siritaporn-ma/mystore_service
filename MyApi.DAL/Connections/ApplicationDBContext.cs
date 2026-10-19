using System;
using Microsoft.EntityFrameworkCore;
using MyApi.DTO.Models.MainModels;
namespace MyApi.DAL.Connections;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
    {

    }
    #region DbSets
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Stock> Stocks { get; set; } = null!;
    public DbSet<ShoppingCart> ShoppingCarts { get; set; } = null!;
    #endregion DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ShoppingCart ใช้ composite primary key (1 แถวต่อสินค้า 1 ชิ้นต่อ 1 order) ไม่มี FK
        modelBuilder.Entity<ShoppingCart>().HasKey(x => new { x.OrderNo, x.ProductCode });

        // HasData ต้องใช้ค่าคงที่ จึงกำหนด CreatedBy/CreatedDate เองแทนค่า default (DateTime.UtcNow)
        var seedDate = new DateTime(2026, 10, 19, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Product>().HasData(
            new Product { ProductCode = "P01", ProductName = "Pen", Price = 15, CreatedBy = "system", CreatedDate = seedDate },
            new Product { ProductCode = "P02", ProductName = "Notebook", Price = 45, CreatedBy = "system", CreatedDate = seedDate },
            new Product { ProductCode = "P03", ProductName = "Eraser", Price = 9, CreatedBy = "system", CreatedDate = seedDate });

        modelBuilder.Entity<Stock>().HasData(
            new Stock { ProductCode = "P01", Balance = 50, CreatedBy = "system", CreatedDate = seedDate },
            new Stock { ProductCode = "P02", Balance = 20, CreatedBy = "system", CreatedDate = seedDate },
            new Stock { ProductCode = "P03", Balance = 99, CreatedBy = "system", CreatedDate = seedDate });
    }
}

