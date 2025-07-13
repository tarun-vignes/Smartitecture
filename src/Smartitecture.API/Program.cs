using Microsoft.OpenApi.Models;
using Smartitecture.Core.DependencyInjection;
using Smartitecture.Core.Configuration;
using Smartitecture.Core.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSmartitectureCore(builder.Configuration);
builder.Services.AddSmartitectureSecurity();
builder.Services.AddSmartitectureOpenAI(builder.Configuration);
builder.Services.AddSmartitectureHealthChecks();
builder.Services.AddSmartitectureBackgroundJobs();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smartitecture API",
        Version = "v1",
        Description = "API for Smartitecture application",
        Contact = new OpenApiContact
        {
            Name = "Smartitecture Team",
            Email = "team@smartitecture.com"
        }
    });

    // Add security definitions
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Add XML comments for API documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smartitecture API V1");
        options.RoutePrefix = string.Empty;
    });
}

// Add middleware
    // Add middleware in order
    app.UseRequestLogging();
    app.UseRateLimiting();
    app.UseSecurityHeaders();
    app.UseHttpsRedirection();
    app.UseCors(builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
    app.UseAuthorization();
    app.UseApiVersioning();
    app.UseErrorHandler();
    app.UseHealthChecks("/health");

    // Add API endpoints
    app.MapControllers();
    app.MapHealthChecks("/health");

app.Run();
