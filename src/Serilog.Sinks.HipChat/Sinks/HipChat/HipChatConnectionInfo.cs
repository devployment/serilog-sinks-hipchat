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

using System.ComponentModel;

namespace Serilog.Sinks.HipChat
{
    /// <summary>
    /// Connection information for use by the HipChat sink.
    /// </summary>
    public class HipChatConnectionInfo
    {
        /// <summary>
        /// The default API base address used for posting messages.
        /// </summary>
        private const string DefaultBaseAddress = "https://api.hipchat.com/";

        /// <summary>
        /// Constructs the <see cref="HipChatConnectionInfo"/> with the default base address set.
        /// </summary>
        public HipChatConnectionInfo()
        {
            BaseAddress = DefaultBaseAddress;
        }

        /// <summary>
        /// Gets or sets the HipChat API base address.
        /// Default value is http://api.hipchat.com/.
        /// </summary>
        [DefaultValue(DefaultBaseAddress)]
        public string BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the rooms messages will be send to
        /// </summary>
        public string ToRoom { get; set; }

        /// <summary>
        /// Gets or sets the API token for accessing rooms
        /// </summary>
        public string RoomApiToken { get; set; }
    }
}
