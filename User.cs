using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace di320fm
{
    class User
    {
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("listen_key")]
        public string ListenKey { get; set; }

        [JsonProperty("confirmation_token")]
        public string ConfirmationToken { get; set; }
    }
}
