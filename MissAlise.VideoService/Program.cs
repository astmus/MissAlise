using System.Text.Json;
using System.Text.Json.Serialization;
using MissAlise.Application;
using MissAlise.DataBase;

namespace MissAlise.Services.Videos
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.AddServiceDefaults();
			builder.AddRedisDistributedCache(connectionName: "cache");
			builder.AddRedisOutputCache(connectionName: "cache", configureOptions: options => { });

			builder.Services.AddApplicationServices().AddPersistanceServices(builder.Configuration);

			builder.Services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
				options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
			});

			//builder.Services.AddEndpointsApiExplorer();
			//builder.Services.AddSwaggerGen();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				//app.UseSwagger();
				//app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();
			app.UseAuthentication();
			app.UseAuthorization();
			app.MapControllers();

			app.Run();
		}
	}
}
