using Microsoft.OpenApi.Models;
using WebApiRetriever.Middleware;
using System.Collections.Generic; // Add this for the List<string> fix

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.IncludeFields = true; // serializes public fields too
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // 1. Define the Scheme
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        Description = "Enter your API Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    };

    c.AddSecurityDefinition("ApiKey", securityScheme);

    // 2. Apply the Requirement
    // Your error CS1503 says it wants a List<string>, not a string array.
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new List<string>() // Fix for Argument 2 error
        }
    });
});

var app = builder.Build();

// Standard middleware
//app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.UsePathBase("/DataRetriever");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();