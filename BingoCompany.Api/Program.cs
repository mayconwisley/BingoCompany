using BingoCompany.Api.Security;
using BingoCompany.Api.Interfaces;
using BingoCompany.Api.Services;
using BingoCompany.Application;
using BingoCompany.Infrastructure;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Threading.RateLimiting;
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
var authenticationConfiguration = builder.Configuration.GetSection(AuthenticationConfiguration.SectionName).Get<AuthenticationConfiguration>() ?? new AuthenticationConfiguration();
var corsConfiguration = builder.Configuration.GetSection(CorsConfiguration.SectionName).Get<CorsConfiguration>() ?? new CorsConfiguration();
var allowedOrigins = corsConfiguration.AllowedOrigins.Where(origin => Uri.TryCreate(origin, UriKind.Absolute, out _)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

if (allowedOrigins.Length == 0)
{
	throw new InvalidOperationException("Configure ao menos uma origem permitida em Cors:AllowedOrigins.");
}

if (authenticationConfiguration.TokenLifetimeMinutes is < 5 or > 720)
{
	throw new InvalidOperationException("Authentication:TokenLifetimeMinutes deve estar entre 5 e 720.");
}

if (!builder.Environment.IsDevelopment() && allowedOrigins.All(origin => new Uri(origin).IsLoopback))
{
	throw new InvalidOperationException("Configure origens HTTPS de produção em Cors:AllowedOrigins.");
}

if (!builder.Environment.IsDevelopment() && !authenticationConfiguration.CookieName.StartsWith("__Secure-", StringComparison.Ordinal))
{
	throw new InvalidOperationException("Authentication:CookieName deve usar o prefixo __Secure- em produção.");
}

var jwtKeyProvider = new JwtKeyProvider(builder.Configuration, builder.Environment);
var jwtKey = jwtKeyProvider.GetKey();
builder.Services.AddSingleton(authenticationConfiguration);
builder.Services.AddSingleton(jwtKeyProvider);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidIssuer = authenticationConfiguration.Issuer,
		ValidateAudience = true,
		ValidAudience = authenticationConfiguration.Audience,
		ValidateLifetime = true,
		ClockSkew = TimeSpan.FromMinutes(1),
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
	};
	options.Events = new JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			if (string.IsNullOrWhiteSpace(context.Token))
			{
				context.Token = context.Request.Cookies[authenticationConfiguration.CookieName];
			}

			return Task.CompletedTask;
		}
	};
});
builder.Services.AddAuthorization();
builder.Services.AddBingoApplication();
builder.Services.AddBingoInfrastructure(builder.Configuration);
builder.Services.AddScoped<IEventParticipantRegistrationService, EventParticipantRegistrationService>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
		context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
		_ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
	options.AddPolicy("public-join", context => RateLimitPartition.GetFixedWindowLimiter(
		context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
		_ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
	options.KnownProxies.Add(IPAddress.Loopback);
	options.KnownProxies.Add(IPAddress.IPv6Loopback);
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<BingoDbContext>();
	await db.Database.MigrateAsync();
}
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseForwardedHeaders();
app.UseCors();
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseAuthentication();
app.Use(async (context, next) =>
{
	if (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) || HttpMethods.IsPatch(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method))
	{
		var hasAuthenticationCookie = context.Request.Cookies.ContainsKey(authenticationConfiguration.CookieName);
		if (hasAuthenticationCookie && context.Request.Path.StartsWithSegments("/api"))
		{
			var origin = context.Request.Headers.Origin.ToString();
			if (!allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				return;
			}
		}
	}

	await next();
});
app.UseAuthorization();
app.UseRateLimiter();
app.MapGet("/healthz", () => Results.Ok()).AllowAnonymous();
app.MapControllers();
app.MapHub<BingoCompany.Api.Hubs.BingoHub>("/hubs/bingo");
app.Run();
public partial class Program;
