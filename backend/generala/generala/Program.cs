using generala.Models.Database.Repositories.Implementations;
using generala;
using generala.Models.Database;
using generala.Models.Mappers;
using generala.Services;

var builder = WebApplication.CreateBuilder(args);

// configuracion directorio
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

// Inyectamos el DbContext
builder.Services.AddScoped<GeneralaContext>();
builder.Services.AddScoped<UnitOfWork>();

// Inyección de todos los repositorios
builder.Services.AddScoped<UserRepository>();

// Inyección de Mappers
builder.Services.AddScoped<UserMapper>();

// Inyección de Servicios
builder.Services.AddScoped<UserService>();

//------------------------------------------------

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await SeedDataBaseAsync(app.Services);


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


// metodo para el seeder
static async Task SeedDataBaseAsync(IServiceProvider serviceProvider)
{
    using IServiceScope scope = serviceProvider.CreateScope();
    using GeneralaContext dbContext = scope.ServiceProvider.GetService<GeneralaContext>();

    // Si no existe la base de datos, la creamos y ejecutamos el seeder
    if (dbContext.Database.EnsureCreated())
    {
        Seeder seeder = new Seeder(dbContext);
        await seeder.SeedAsync();
    }
}