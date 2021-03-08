using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GoDaddyDNSSync
{
	public class Worker : BackgroundService
	{
		private readonly ILogger<Worker> _logger;
		private DateTime _lastRun = DateTime.Now.AddMinutes(-61);
		private readonly ApiInfo _apiInfo;
		private readonly JsonSerializerOptions _serializerOptions;

		public Worker(ILogger<Worker> logger, IOptions<ApiInfo> apiInfo)
		{
			_logger = logger;
			_apiInfo = apiInfo.Value;
			_serializerOptions = new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true
			};
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Worker started at {time}", DateTimeOffset.Now);
			while (!stoppingToken.IsCancellationRequested)
			{
				if (DateTime.Now.Hour > _lastRun.Hour)
				{
					_logger.LogInformation("Starting sync at {time}", DateTimeOffset.Now);
					await SendAPIRequest();
				}
				await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
			}

			_logger.LogInformation("Worker stopped at {time}", DateTimeOffset.Now);
		}

		private async Task<string> GetCurrentPublicIP()
		{
			using var client = new HttpClient();
			client.BaseAddress = new Uri("http://ipinfo.io/");
			var publicIPInfo = await client.GetFromJsonAsync<RemoteIP>("/json", _serializerOptions);
			_logger.LogInformation("Current public IP is {ip}", publicIPInfo.IP);
			return publicIPInfo.IP;
		}

		private async Task SendAPIRequest()
		{
			string[] exclusions = _apiInfo.Exclusions.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
			var currentIP = await GetCurrentPublicIP();
			using var client = new HttpClient();
			client.BaseAddress = new Uri("https://api.godaddy.com/");
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			string ssoKey = _apiInfo.Key + ":" + _apiInfo.Secret;
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("sso-key", ssoKey);

			var records = await client.GetFromJsonAsync<List<Record>>($"/v1/domains/{_apiInfo.BaseDomain}/records/A", _serializerOptions);
			List<string> recordsToUpdate = new();
			foreach(var record in records)
			{
				if (!exclusions.Contains(record.Name))
				{
					_logger.LogInformation("Record {name} has an IP of {data}", record.Name, record.Data);
					if (record.Data != currentIP)
					{
						recordsToUpdate.Add(record.Name);
					}
				}
			}

			foreach(var outOfSyncRec in recordsToUpdate)
			{
				var record = new Record
				{
					Data = currentIP,
					Name = outOfSyncRec,
					Ttl = 3600,
					Type = "A"
				};
				_logger.LogInformation("Updating record {outOfSyncRec} to IP {currentIP}", outOfSyncRec, currentIP);
				await client.PutAsJsonAsync($"/v1/domains/{_apiInfo.BaseDomain}/records/A/{outOfSyncRec}", record, _serializerOptions);
			}
			_lastRun = DateTime.Now;
		}
	}
}
