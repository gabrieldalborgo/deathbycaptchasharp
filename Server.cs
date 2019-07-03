using Newtonsoft.Json;

namespace DeathByCaptchaSharp
{
    public class Server
    {
        [JsonProperty("status")]
        public int Status { get; internal set; }

        [JsonProperty("todays_accuracy")]
        public decimal TodaysAccuracy { get; internal set; }

        [JsonProperty("solved_in")]
        public int SolvedIn { get; internal set; }

        [JsonProperty("is_service_overloaded")]
        public bool IsServiceOverloaded { get; internal set; }

        internal Server() { }
    }
}