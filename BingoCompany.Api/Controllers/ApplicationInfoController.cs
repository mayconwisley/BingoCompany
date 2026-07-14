using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/application")]
public sealed class ApplicationInfoController : ControllerBase
{
	[HttpGet("info")]
	public ActionResult<ApplicationInfoResponse> Get()
	{
		var assembly = typeof(Program).Assembly;
		var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? assembly.GetName().Name ?? "Aplicação";
		var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty;
		var version = assembly.GetName().Version?.ToString(3)
			?? "0.0.0";

		return Ok(new ApplicationInfoResponse(title, description, version));
	}
}

public sealed record ApplicationInfoResponse(string Name, string Description, string Version);
