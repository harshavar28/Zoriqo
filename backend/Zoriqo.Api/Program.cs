using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zoriqo.Infrastructure.Data;
using Zoriqo.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("ZoriqoDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'ZoriqoDatabase' is missing.");

builder.Services.AddDbContext<ZoriqoDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        postgres => postgres.MigrationsHistoryTable(
            "__EFMigrationsHistory",
            "zoriqo_identity")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("ExpoDevelopment", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8081",
                "http://127.0.0.1:8081",
                "http://192.168.1.21:8081")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = null;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        // Five failed attempts temporarily lock the account.
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(5);

    })
    .AddEntityFrameworkStores<ZoriqoDbContext>()
    .AddSignInManager();

builder.Services
    .AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(
        IdentityConstants.BearerScheme,
        options =>
        {
            options.BearerTokenExpiration =
                TimeSpan.FromMinutes(1);

            options.RefreshTokenExpiration =
                TimeSpan.FromDays(7);
        });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Zoriqo API v1");

        options.RoutePrefix = "swagger";
    });

    app.MapGet("/health", () => Results.Ok(new
    {
        status = "Zoriqo API is reachable"
    })).AllowAnonymous();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseCors("ExpoDevelopment");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();