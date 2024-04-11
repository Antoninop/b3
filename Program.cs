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

    List<string> nomsEtudiants = new List<string>();
    List<string> prenomsEtudiants = new List<string>();
    List<List<float>> notesEtudiants = new List<List<float>>();
    List<List<float>> coefsEtudiants = new List<List<float>>();

    using (var reader = new StreamReader(file.OpenReadStream()))
    {
        string content = await reader.ReadToEndAsync();
        string[] lignes = content.Split('\n');

        bool premiereLigne = true;

        foreach (var ligne in lignes)
        {
            string[] colonnes = ligne.Split(';');

            if (premiereLigne)
            {
                premiereLigne = false;
                continue;
            }

            if (!int.TryParse(colonnes[0], out _))
            {
                if (colonnes.Length > 3 && colonnes[3] == "Coef.")
                {
                    var coefs = colonnes.Skip(4).Where(s => float.TryParse(s, out _)).Select(float.Parse).ToList();
                    coefsEtudiants.Add(coefs);
                    Console.WriteLine("Coefs : " + string.Join(", ", coefs));
                }
                continue;
            }

            nomsEtudiants.Add(colonnes[5]);
            prenomsEtudiants.Add(colonnes[6]);

            List<float> notes = new List<float>();

            var debutNote = 14;

            for (int i = 0; i < 17; i++)
            {
                if (!string.IsNullOrWhiteSpace(colonnes[debutNote + i]))
                {
                    float note;
                    if (float.TryParse(colonnes[debutNote + i].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out note))
                    {
                        notes.Add(note);
                    }
                    else
                    {
                        Console.WriteLine($"Erreur  {debutNote + i}  {colonnes[5]} {colonnes[6]}");
                    }
                }
            }

            notesEtudiants.Add(notes);
        }
    }

    StringBuilder responseBuilder = new StringBuilder();
    for (int i = 0; i < nomsEtudiants.Count; i++)
    {
        responseBuilder.AppendLine($"Nom: {nomsEtudiants[i]}, Prénom: {prenomsEtudiants[i]}, Notes: {string.Join(", ", notesEtudiants[i])}");
    }

    return Results.Ok(responseBuilder.ToString());
});



app.Run();