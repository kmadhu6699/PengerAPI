using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PengerAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Configure AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Penger API", 
        Version = "v1",
        Description = "Financial management API for Penger application"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Initialize the database with seed data
if (app.Environment.IsDevelopment())
{
    // Seed the database
    DbInitializer.InitializeAsync(app.Services).Wait();
}

app.Run();