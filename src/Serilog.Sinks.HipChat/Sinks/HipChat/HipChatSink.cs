// Copyright 2014 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.HipChat
{
    class HipChatSink : PeriodicBatchingSink
    {
        readonly HipChatConnectionInfo _connectionInfo;

        readonly ITextFormatter _textFormatter;
        private readonly HttpClient _httpClient;

        private static readonly IDictionary<LogEventLevel, string> LevelHipChatColorMap = new Dictionary<LogEventLevel, string>
        {
            {LogEventLevel.Verbose, "gray"},
            {LogEventLevel.Debug, "gray"},
            {LogEventLevel.Information, "green"},
            {LogEventLevel.Warning, "yellow"},
            {LogEventLevel.Error, "red"},
            {LogEventLevel.Fatal, "red"},
        };

        /// <summary>
        /// A reasonable default for the number of events posted in
        /// each batch.
        /// </summary>
        public const int DefaultBatchPostingLimit = 5;

        /// <summary>
        /// A reasonable default time to wait between checking for event batches.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Construct a HipChat sink with the specified details.
        /// </summary>
        /// <param name="connectionInfo">Connection information used to construct the http client and HipChat messages.</param>
        /// <param name="batchSizeLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="textFormatter">Supplies culture-specific formatting information, or null.</param>
        public HipChatSink(HipChatConnectionInfo connectionInfo, int batchSizeLimit, TimeSpan period, ITextFormatter textFormatter)
            : base(batchSizeLimit, period)
        {
            if (connectionInfo == null) throw new ArgumentNullException("connectionInfo");

            _connectionInfo = connectionInfo;
            _textFormatter = textFormatter;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_connectionInfo.BaseAddress)
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Free resources held by the sink.
        /// </summary>
        /// <param name="disposing">If true, called because the object is being disposed; if false,
        /// the object is being disposed from the finalizer.</param>
        protected override void Dispose(bool disposing)
        {
            // First flush the buffer
            base.Dispose(disposing);

            if (disposing)
                _httpClient.Dispose();
        }

        /// <summary>
        /// Emit a batch of log events, running asynchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        /// <remarks>Override either <see cref="PeriodicBatchingSink.EmitBatch"/> or <see cref="PeriodicBatchingSink.EmitBatchAsync"/>,
        /// not both.</remarks>
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException("events");

            foreach (var logEvent in events)
            {
                using (var payload = new StringWriter())
                {
                    _textFormatter.Format(logEvent, payload);

                    var requestUri = string.Format("v2/room/{0}/notification?auth_token={1}", _connectionInfo.ToRoom, _connectionInfo.RoomApiToken);

                    var body = new
                    {
                        color = LevelHipChatColorMap[logEvent.Level],
                        message = payload.ToString(),
                        notify = logEvent.Level >= LogEventLevel.Warning
                    };

                    var result = await _httpClient.PostAsJsonAsync(requestUri, body);

                    if (result.IsSuccessStatusCode) continue;

                    SelfLog.WriteLine("Posting HipChat message failed {0}: {1}", "StatusCode", (int)result.StatusCode);
                    SelfLog.WriteLine("Posting HipChat message failed {0}: {1}", "Reason", result.ReasonPhrase);
                }
            }
        }
    }
}
