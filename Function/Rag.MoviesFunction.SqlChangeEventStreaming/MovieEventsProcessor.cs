using Azure.Messaging.EventHubs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rag.MoviesFunction.AzureSql
{
	public class MovieEventsProcessor
	{
		private static readonly HashSet<long> _movieIds = [];
		private static readonly object _lock = new();

		private readonly ILogger<MovieEventsProcessor> _logger;
		private readonly string _connectionString;

		public MovieEventsProcessor(ILogger<MovieEventsProcessor> logger)
		{
			this._logger = logger;
			this._connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
		}

		[Function("ProcessEvents")]
		public void ProcessEvents(
			[EventHubTrigger(eventHubName: "%EventHubName%", Connection = "EventHubConnection")]
			EventData[] events)
		{
			foreach (var eventData in events)
			{
				try
				{
					this.ProcessEvent(eventData);
				}
				catch (Exception ex)
				{
					this._logger.LogError(ex, "Error processing event.");
				}
			}
		}

		private void ProcessEvent(EventData eventData)
		{
			var eventBodyJson = Encoding.UTF8.GetString(eventData.EventBody.ToArray());
			var eventBody = JObject.Parse(eventBodyJson);
			var operation = eventBody["operation"].Value<string>();
			var dataJson = eventBody["data"].Value<string>();
			var data = JObject.Parse(dataJson);
			var table = data["eventsource"]["tbl"].Value<string>();
			var primaryKeyColumns = data["eventsource"]["pkkey"].ToObject<JArray>();
			var primaryKey = string.Join(", ", primaryKeyColumns.OfType<JObject>().Select(pkc => $"{pkc["columnname"]} = {pkc["value"]}"));
			var movieId = primaryKeyColumns.OfType<JObject>().First(pkc => pkc["columnname"].Value<string>() == "MovieId")["value"].Value<long>();

			lock (_lock)
			{
				if (operation != "DEL" || table != "Movie")
				{
					// A movie row was added or updated, or a movie's child row was added, updated, or deleted; add the movie ID to the list to be vectorized
					this._logger.LogWarning($"Change event {operation} on {table}, {primaryKey}");
					_movieIds.Add(movieId);     
				}
				else
				{
					// A movie row was deleted, which means all of its child rows were also deleted (including the vector); remove the movie ID from the list to be vectorized
					this._logger.LogError($"Change event {operation} on {table}, {primaryKey}");
					_movieIds.Remove(movieId);
				}
			}
		}

		[Function("ProcessMovies")]
		public async Task ProcessMovies(
			[TimerTrigger(schedule: "*/5 * * * * *")]	// fire every 5 seconds
			TimerInfo timerInfo)
		{
			var movieIds = default(long[]);

			lock (_lock)
			{
				movieIds = _movieIds.ToArray();
				_movieIds.Clear();
			}

			if (movieIds.Length == 0)
			{
				return;
			}

			var movieIdsCsv = string.Join(",", movieIds);

			this._logger.LogWarning($"Vectorizing {movieIds.Length} new/changed movie(s): {movieIdsCsv}");

			using var conn = new SqlConnection(this._connectionString);
			await conn.OpenAsync();
			conn.InfoMessage += new SqlInfoMessageEventHandler((sender, e) =>
			{
				foreach (SqlError error in e.Errors)
				{
					this._logger.LogWarning(error.Message);
				}
			});

			using var cmd = new SqlCommand("VectorizeMovies", conn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.AddWithValue("@MovieIdsCsv", movieIdsCsv);
			await cmd.ExecuteNonQueryAsync();

			await conn.CloseAsync();
		}

	}
}
