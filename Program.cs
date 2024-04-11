using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Globalization;



var builder = WebApplication.CreateBuilder();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin() 
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();


app.MapPost("/upload", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var file = form.Files.GetFile("file");

    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("Aucun fichier n'a été envoyé.");
    }

    List<object> students = new List<object>();

    using (var reader = new StreamReader(file.OpenReadStream()))
    {
        string content = await reader.ReadToEndAsync();
        string[] lines = content.Split('\n');

        bool isFirstLine = true;

        foreach (var line in lines)
        {
            string[] columns = line.Split(';');

            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }

            if (!int.TryParse(columns[0], out _))
            {
                continue;
            }

            List<float> notes = new List<float>();

            var startNoteIndex = 14;

            for (int i = 0; i < 17; i++)
            {
                if (!string.IsNullOrWhiteSpace(columns[startNoteIndex + i]))
                {
                    float note;
                    if (float.TryParse(columns[startNoteIndex + i].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out note))
                    {
                        notes.Add(note);
                    }
                    else
                    {
                        Console.WriteLine($"Erreur {startNoteIndex + i} {columns[5]} {columns[6]}");
                    }
                }
            }

            students.Add(new
            {
                Nom = columns[5],
                Prénom = columns[6],
                Notes = notes
            });
        }
    }

    return Results.Ok(students);
});



app.Run();