using BingoCompany.Infrastructure;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddBingoInfrastructure(builder.Configuration);
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BingoDbContext>();
    await db.Database.MigrateAsync();
}
app.UseSwagger(); app.UseSwaggerUI();
app.UseCors(); app.UseHttpsRedirection(); app.MapControllers(); app.MapHub<BingoCompany.Api.Hubs.BingoHub>("/hubs/bingo");
app.Run();
public partial class Program;
