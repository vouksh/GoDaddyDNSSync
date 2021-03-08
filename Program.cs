using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace GoDaddyDNSSync
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console()
				.WriteTo.File(
				path: AppContext.BaseDirectory + "Sync-.log",
				shared: true,
				rollingInterval: RollingInterval.Day,
				retainedFileCountLimit: 7
				)
				.CreateLogger();
			try
			{
				CreateHostBuilder(args).Build().Run();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Application level error occurred");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(o =>
				{
					o.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
					o.SetBasePath(AppContext.BaseDirectory);
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.Configure<ApiInfo>(hostContext.Configuration.GetSection(nameof(ApiInfo)));
					services.AddHostedService<Worker>();
				})
				.UseSerilog()
				.UseWindowsService(c =>
				{
					c.ServiceName = "GoDaddy Sync Service";
				});
	}
}
