using Newtonsoft.Json;

namespace DeathByCaptchaSharp
{
    public class Captcha
    {
        [JsonProperty("captcha")]
        public int Id { get; internal set; }

        [JsonProperty("status")]
        public int Status { get; internal set; }

        [JsonProperty("is_correct")]
        public bool IsCorrect { get; internal set; }

        [JsonProperty("text")]
        public string Text { get; internal set; }

        [JsonIgnore]
        public bool Uploaded { get => this.Id > 0; }

        [JsonIgnore]
        public bool Solved { get => this.Uploaded && !string.IsNullOrEmpty(this.Text); }

        [JsonIgnore]
        public bool Correct { get => this.Solved && this.IsCorrect; }

        internal Captcha() { }
    }
}
