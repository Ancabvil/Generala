using generala.Models.Database.Repositories.Implementations;
using generala;
using generala.Models.Database;
using generala.Models.Mappers;
using generala.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// configuracion directorio
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

// Inyectamos el DbContext
builder.Services.AddScoped<GeneralaContext>();
builder.Services.AddScoped<UnitOfWork>();

// Inyección de todos los repositorios
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ImageRepository>();

// Inyección de Mappers
builder.Services.AddScoped<UserMapper>();
builder.Services.AddScoped<ImageMapper>();

// Inyección de Servicios
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ImageService>();

//------------------------------------------------

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin() // Permitir cualquier origen
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Permite CORS
app.UseCors("AllowAllOrigins");

// wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
});


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