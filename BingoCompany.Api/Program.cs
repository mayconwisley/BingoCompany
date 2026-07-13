using BingoCompany.Api.Security;
using BingoCompany.Infrastructure;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddScoped<CompanyEventOwnerFilter>();
builder.Services.AddSingleton<JwtKeyProvider>();
var jwtKey = builder.Configuration["BingoJwtKey"] ?? builder.Configuration["Authentication:JwtKey"] ?? throw new InvalidOperationException("Configure a variável de ambiente BingoJwtKey.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = false, ValidateAudience = false, ValidateLifetime = true, ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)) });
builder.Services.AddAuthorization();
builder.Services.AddBingoInfrastructure(builder.Configuration);
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<BingoDbContext>();
	await db.Database.MigrateAsync();
}
app.UseSwagger(); app.UseSwaggerUI();
app.UseCors();
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); app.MapHub<BingoCompany.Api.Hubs.BingoHub>("/hubs/bingo");
app.Run();
public partial class Program;
