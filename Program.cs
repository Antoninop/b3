using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder();

// Configuration des services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin() // Autoriser toutes les origines
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

// Utilisation du middleware CORS
app.UseCors();


app.MapPost("/upload", async (HttpContext context) =>
{

    
    var form = await context.Request.ReadFormAsync();
    var file = form.Files.GetFile("file");

    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("Aucun fichier n'a été envoyé.");
    }

    using (var reader = new StreamReader(file.OpenReadStream()))
    {
        var content = await reader.ReadToEndAsync();
        return Results.Ok(content);
    }
});

app.Run();