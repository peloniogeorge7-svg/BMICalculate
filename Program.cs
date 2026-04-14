using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.MapPost("/calculate", async (HttpContext context) =>
{
    try
    {
        var form = await context.Request.ReadFormAsync();
        
        double weight = double.Parse(form["weight"].ToString()!);
        double height = double.Parse(form["height"].ToString()!) / 100;
        
        double bmi = weight / (height * height);
        
        var (category, range, advice, progressClass) = AnalyzeBMI(bmi);
        
        var resultHtml = $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>BMI Calculator - No JS</title>
            <link rel="stylesheet" href="/css/style.css">
        </head>
        <body>
            <div class="container">
                <div class="calculator-card">
                    <header>
                        <h1>BMI Calculator</h1>
                    </header>
                    
                    <form action="/calculate" method="post" class="bmi-form">
                        <div class="input-group">
                            <label for="weight">Weight (kg)</label>
                            <input type="number" id="weight" name="weight" step="0.1" min="20" max="300" value="{{weight}}" required>
                        </div>

                        <div class="input-group">
                            <label for="height">Height (cm)</label>
                            <input type="number" id="height" name="height" step="0.1" min="50" max="250" value="{{height}}" required>
                        </div>

                        <button type="submit" class="calc-button">
                            Calculate BMI
                        </button>
                    </form>
                    
                    <div class="results show" id="results">
                        <div class="bmi-result">
                            <span class="bmi-number" id="bmiValue">{{bmi:F1}}</span>
                            <span class="bmi-label">Your BMI</span>
                        </div>
                        
                        <div class="status {{progressClass}}" id="status">
                            <h2 id="category">{{category}}</h2>
                            <p id="rangeText">Range: {{range}}</p>
                            <p class="advice" id="adviceText">{{advice}}</p>
                        </div>
                        
                        <div class="progress-bar">
                            <div class="progress-fill {{progressClass}}-fill" id="progressFill"></div>
                        </div>
                </div>
            </div>
        </body>
        </html>
        """;
        
        return Results.Text(resultHtml, "text/html");
    }
    catch
    {
        return Results.Text("Invalid input! Please enter valid numbers.", "text/html", statusCode: 400);
    }
});

app.Run("http://localhost:5000");

static (string category, string range, string advice, string progressClass) AnalyzeBMI(double bmi)
{
    return bmi switch
    {
        < 18.5 => ("Underweight", "< 18.5", " Eat more healthy food!", "underweight"),
        < 25 => ("Normal", "18.5 - 24.9", " Perfect! Keep it up!", "normal"),
        < 30 => ("Overweight", "25 - 29.9", " Exercise + healthy diet!", "overweight"),
        _ => ("Obese", "30+", " Consult a doctor", "obese")
    };
}
