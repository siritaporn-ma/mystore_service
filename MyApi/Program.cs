using Microsoft.EntityFrameworkCore;
using MyApi.BSL.IService;
using MyApi.BSL.Service;
using MyApi.DAL.Connections;
using MyApi.DAL.Data;
using MyApi.DAL.IData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Add DB Context
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection"));
});
#endregion Add DB Context

#region CORS
// อนุญาตเฉพาะ origin ของหน้าบ้านที่ระบุใน appsettings.json (Cors:AllowedOrigins)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
#endregion CORS

#region Service
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
#endregion

#region Data
builder.Services.AddScoped<IProductData, ProductData>();
builder.Services.AddScoped<IShoppingCartData, ShoppingCartData>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.MapControllers();

app.Run();
