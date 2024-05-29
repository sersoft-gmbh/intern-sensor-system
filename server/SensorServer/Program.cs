using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SensorServer;
using SensorServer.Helpers;
using SensorServer.Repositories;

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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Sensor Server", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "Custom",
        Scheme = "Bearer",
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();
app.UseFileServer();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
