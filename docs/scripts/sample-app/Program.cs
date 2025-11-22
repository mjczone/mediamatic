// Sample ASP.NET Core app to generate OpenAPI specification for documentation
using MJCZone.MediaMatic.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddMediaMatic();

// Add OpenAPI/Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "MediaMatic REST API",
            Version = "v1",
            Description = "Media management REST API using MediaMatic",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "MJCZone Inc.",
                Url = new Uri("https://github.com/mjczone/mediamatic"),
            },
            License = new Microsoft.OpenApi.Models.OpenApiLicense
            {
                Name = "LGPL v3",
                Url = new Uri("https://www.gnu.org/licenses/lgpl-3.0.html"),
            },
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Always enable Swagger for documentation generation
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

// Map MediaMatic endpoints
app.UseMediaMatic();

app.Run();
