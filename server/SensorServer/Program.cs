using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using SensorServer;
using SensorServer.Helpers;
using SensorServer.OpenApi.DocumentTransformers;
using SensorServer.OpenApi.OperationTransformers;
using SensorServer.Repositories;
using SensorServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEntityFrameworkSqlite();
builder.Services.AddDbContextFactory<ApplicationDbContext>();
builder.Services.AddScoped<MeasurementsRepository>();

builder.Services.AddHttpLogging(_ => {});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.WithOrigins("*"));
});

builder.Services
    .AddAuthentication("Bearer")
    .AddScheme<SimpleTokenAuthenticationOptions, SimpleTokenAuthenticationHandler>("Bearer", options =>
    {
        options.AllowedTokens = builder.Configuration.GetSection("AllowedTokens").Get<IReadOnlySet<string>>() ??
                                new SortedSet<string>();
    });

builder.Services
    .AddControllers(options =>
        options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())))
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter()));

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo { Title = "Sensor Server", Version = "v1" };
        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<AuthorizationDocumentTransformer>();
    options.AddOperationTransformer<AuthorizedOperationTransformer>();
});

builder.Services.AddHostedService<DbCheckpointService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await using (var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
    {
        await dbContext.Database.MigrateAsync();
        await dbContext.Database.EnableWal(app.Configuration.GetValue<long?>("DbMaxJournalSize"));
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Servers =
        [
            new ScalarServer("http://localhost:5076", "Localhost"),
        ];
    });
    app.MapGet("/swagger", () => Results.Redirect("/scalar/v1"))
        .ExcludeFromDescription();
    app.MapGet("/scalar", () => Results.Redirect("/scalar/v1"))
        .ExcludeFromDescription();
}

app.UseHttpLogging();
app.UseFileServer();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
