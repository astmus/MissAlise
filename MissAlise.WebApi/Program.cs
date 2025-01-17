using System.Text.Json;
using System.Text.Json.Serialization;
using MissAlise.Entities.OneDrive;

namespace MissAlise.WebApi;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
		builder.AddServiceDefaults();

		builder.Services.AddControllers().AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
			options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
		});
	
		builder.Services.Configure<AzureAd>(builder.Configuration.GetSection("AzureAd"));
		
		builder.Services.AddProblemDetails();
		
		//builder.Services.AddEndpointsApiExplorer(); это только для minimal api
		builder.Services.AddSwaggerGen();

		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseSwagger();
			app.UseSwaggerUI();
		}		
		else
			app.UseExceptionHandler();
		
		app.UseHttpsRedirection();
		app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();		

		app.MapControllers();
		app.MapDefaultEndpoints();

		//// Configure the HTTP request pipeline.

		//app.UseHttpsRedirection();

		//app.UseAuthentication();
		//app.UseAuthorization();

		//app.MapControllers();
		
		app.Run();
	}
}
