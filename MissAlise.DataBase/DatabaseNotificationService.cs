//using System.Text.Json;
//using System.Text.Json.Nodes;
//using MissAlise.Background.Settings;

//namespace MissAlise.Background
//{
//#nullable disable
//	public delegate void DbNotificationDelegate<T>(T objectNotification);
//	public sealed class DatabaseNotificationService : BackgroundService
//	{
//		public static event DbNotificationDelegate<BackgroundJobNotification> BackgroundJobStateChanged;

//		private readonly ILogger _log;
//		private readonly NpgsqlConnection _connection;
//		private readonly JsonSerializerOptions serializeOptions = new JsonSerializerOptions
//		{
//			PropertyNameCaseInsensitive = true
//		};

//		public DatabaseNotificationService(ILogger logger)
//		{
//			_log = logger;
//			var builder = new NpgsqlConnectionStringBuilder(StartupSettings.Current.ConnectionString);
//			builder.KeepAlive = 4;
//			builder.IncludeErrorDetail = true;
//			builder.Enlist = false;
//			_connection = new NpgsqlConnection(builder.ConnectionString);
//		}

//		public override async Task StartAsync(CancellationToken cancellationToken)
//		{
//			await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
//			_connection.Notice += OnNotice;
//			_connection.Notification += OnNotification;

//			NpgsqlCommand cmd;
//			cmd = new NpgsqlCommand("LISTEN job_activity", _connection);
//			await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
//			cmd = new NpgsqlCommand("LISTEN trigger_config", _connection);
//			await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
//			cmd = new NpgsqlCommand("LISTEN job_config", _connection);
//			await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
//			cmd = new NpgsqlCommand("LISTEN server_config", _connection);
//			await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

//			await base.StartAsync(cancellationToken);
//		}


//		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
//		{
//			do
//				try
//				{
//					await _connection.WaitAsync(cancellationToken).ConfigureAwait(false);
//				}
//				catch (Exception error)
//				{
//					_log.Error(nameof(DatabaseNotificationService), nameof(ExecuteAsync), error);
//				}
//			while (!cancellationToken.IsCancellationRequested);
//		}

//		private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
//		{
//			try
//			{
//				switch (e.Channel)
//				{
//					case "job_config":
//					ApplyJobConfiguration(e);
//					break;
//					case "trigger_config":
//					ApplyTriggerConfiguration(e);
//					break;
//					case "server_config":
//					ApplyServerConfiguration(e);
//					break;
//					case "job_activity":
//					RaiseJobActivityChanged(e);
//					break;
//				}
//			}
//			catch (Exception error)
//			{
//				_log.Error(nameof(DatabaseNotificationService), nameof(OnNotification), error);
//			}
//		}

//		void RaiseJobActivityChanged(NpgsqlNotificationEventArgs e)
//		{
//			if (JsonSerializer.Deserialize<BackgroundJobNotification>(e.Payload, serializeOptions) is BackgroundJobNotification obj)
//				BackgroundJobStateChanged?.Invoke(obj);
//		}

//		void OnNotice(object sender, NpgsqlNoticeEventArgs e)
//			=> _log.Info(nameof(DatabaseNotificationService), nameof(ExecuteAsync), e.Notice.MessageText);

//		void ApplyServerConfiguration(NpgsqlNotificationEventArgs e)
//		{
//			var srvId = JsonNode.Parse(e.Payload)["serverid"].GetValue<Guid>();
//			if (srvId == BackgroundTasksService.Server.Id)
//				Newtonsoft.Json.JsonConvert.PopulateObject(e.Payload, BackgroundTasksService.Server, Json.Settings);
//		}

//		void ApplyJobConfiguration(NpgsqlNotificationEventArgs e)
//		{
//			var jobData = Newtonsoft.Json.Linq.JObject.Parse(e.Payload);
//			var job = EventTriggerCollection.Instance.FirstOrDefault(f => jobData.Value<string>("key").EndsWith(f.TaskKey) && jobData.Value<string>("entityid").Equals(f.Job.Organization.Id.ToString()))?.Job;
//			if (job != null)
//				Newtonsoft.Json.JsonConvert.PopulateObject(e.Payload, job);
//		}

//		void ApplyTriggerConfiguration(NpgsqlNotificationEventArgs e)
//		{
//			var triggerData = Newtonsoft.Json.Linq.JObject.Parse(e.Payload);
//			var trigger = EventTriggerCollection.Instance.FirstOrDefault(f => triggerData.Value<string>("taskkey").EndsWith(f.TaskKey) && triggerData.Value<string>("entityid").Equals(f.Job.Organization.Id.ToString()));
//			if (trigger != null)
//				Newtonsoft.Json.JsonConvert.PopulateObject(triggerData.Value<string>("value"), trigger);
//		}
//	}
//#nullable enable
//}
