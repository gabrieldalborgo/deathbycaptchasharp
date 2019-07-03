using Newtonsoft.Json;

namespace DeathByCaptchaSharp
{
    public class User
    {
        [JsonProperty("user")]
        public int Id { get; internal set; }

        [JsonProperty("status")]
        public int Status { get; internal set; }

        [JsonProperty("is_banned")]
        public bool IsBanned { get; internal set; }

        [JsonProperty("balance")]
        public decimal Balance { get; internal set; }

        [JsonProperty("rate")]
        public decimal Rate { get; internal set; }

        internal User() { }
    }
}