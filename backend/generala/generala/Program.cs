using generala.Models.Database.Repositories.Implementations;
using generala;
using generala.Models.Database;
using generala.Models.Mappers;
using generala.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using generala.Network;
using generala.Middleware;

var builder = WebApplication.CreateBuilder(args);

// configuracion directorio
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

// Inyectamos el DbContext
builder.Services.AddScoped<GeneralaContext>();
builder.Services.AddScoped<UnitOfWork>();

// Inyección de todos los repositorios
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ImageRepository>();
builder.Services.AddScoped<FriendRequestRepository>();
builder.Services.AddScoped<FriendshipRepository>();

// Inyección de Mappers
builder.Services.AddScoped<UserMapper>();
builder.Services.AddScoped<ImageMapper>();
builder.Services.AddScoped<FriendRequestMapper>();

// Inyección de Servicios
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ImageService>();


builder.Services.AddSingleton<WebSocketNotificationService>();
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<WebSocketNetwork>();

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
// Configuración de autenticacion
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        Settings settings = builder.Configuration.GetSection(Settings.SECTION_NAME).Get<Settings>();
        string key = settings.JwtKey;

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

var app = builder.Build();

// Habilitar WebSockets
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
app.UseWebSockets(webSocketOptions);

// Middleware WebSocket
app.UseMiddleware<WebSocketTokenMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Permite CORS
app.UseCors("AllowAllOrigins");

app.UseAuthentication();


// wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars"))
});

var staticFilesPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(staticFilesPath),
    RequestPath = ""
});

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Path}");
    await next();
});

app.UseStaticFiles();

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