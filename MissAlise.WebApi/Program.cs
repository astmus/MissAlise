using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MissAlise.Application;
using MissAlise.DataBase;
using MissAlise.Entities.OneDrive;
using MissAlise.OneDrive;

namespace MissAlise.WebApi;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
		builder.AddServiceDefaults();

		builder.Services.AddControllers().AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
		});
	
		builder.Services.Configure<AzureAd>(builder.Configuration.GetSection("AzureAd"));
		
		builder.Services.AddProblemDetails();
		builder.Services.AddApplication().AddPersistance(builder.Configuration, false).AddOneDriveService(builder.Configuration.GetSection(nameof(AzureAd))).AddOneDriveHandling();
		//builder.Services.AddEndpointsApiExplorer(); это только для minimal api
		builder.Services.AddSwaggerGen(options=> {
			var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
		}
		);

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseSwagger();
			app.UseSwaggerUI();
		}		
		
		app.UseHttpsRedirection();
		app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();		

		app.MapControllers();
		app.MapDefaultEndpoints();

		app.Run();
	}
}
