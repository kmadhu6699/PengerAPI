using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PengerAPI.Data;
using PengerAPI.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Configure specific repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
builder.Services.AddScoped<IOTPRepository, OTPRepository>();

// Configure Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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