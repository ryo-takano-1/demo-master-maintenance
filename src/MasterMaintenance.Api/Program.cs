using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MasterMaintenance.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// CORS 設定
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // 本番環境では必要に応じてオリジンを制限
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT 認証設定
// 秘密鍵は appsettings.json の Jwt:Key をデフォルトとし、
// 環境変数 Jwt__Key（または Jwt:Key）で上書き可能
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is not configured. Set 'Jwt:Key' in appsettings.json or the environment variable 'Jwt__Key'.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)),
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// 開発時は自動マイグレーション適用
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// グローバル例外ハンドラ
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var isDev = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "サーバーエラーが発生しました。",
            Detail = isDev ? exceptionFeature?.Error?.ToString() : "予期しないエラーが発生しました。しばらく経ってから再度お試しください。",
        };

        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// テストプロジェクトから WebApplicationFactory で参照するための partial class
public partial class Program { }
