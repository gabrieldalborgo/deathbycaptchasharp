using Newtonsoft.Json;
using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeathByCaptchaSharp
{
    public class Client: IDisposable
    {
        private readonly string username;
        private readonly string password;
        private readonly HttpClient client;
        private readonly Encoding encoding;

        public Client(string username, string password)
        {
            this.username = username;
            this.password = password;
            this.client = new HttpClient();
            this.client.BaseAddress = new Uri("http://api.dbcapi.me/api/");
            this.client.DefaultRequestHeaders.Add("Accept", "application/json");
            this.encoding = Encoding.ASCII;
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        /// <summary>
        /// Upload and wait for a CAPTCHA to be solved.
        /// </summary>
        /// <param name="img">Raw CAPTCHA image.</param>
        /// <param name="timeout">Solving timeout (in seconds).</param>
        /// <returns>CAPTCHA if solved, null otherwise.</returns>
        public async Task<Captcha> Decode(byte[] img, int timeout)
        {
            var captcha = await this.Upload(img);
            return await this.Poll(captcha, timeout);
        }

        /// <summary>
        /// Upload and wait for a CAPTCHA to be solved.
        /// </summary>
        /// <param name="img">Raw CAPTCHA image.</param>
        /// <param name="timeout">Solving timeout (in seconds).</param>
        /// <returns>CAPTCHA if solved, null otherwise.</returns>
        public async Task<Captcha> Decode(byte[] img, Hashtable extraData, int timeout)
        {
            var captcha = await this.Upload(img, extraData);
            return await this.Poll(captcha, timeout);
        }

        /// <summary>
        /// Upload and wait for a CAPTCHA to be solved.
        /// </summary>
        /// <param name="img">Raw CAPTCHA image.</param>
        /// <param name="timeout">Solving timeout (in seconds).</param>
        /// <returns>CAPTCHA if solved, null otherwise.</returns>
        public async Task<Captcha> Decode(Hashtable extraData, int timeout)
        {
            var captcha = await this.Upload(extraData);
            return await this.Poll(captcha, timeout);
        }

        /// <summary>
        /// Upload and wait for a CAPTCHA to be solved. [Tokens(reCAPTCHA v2)]
        /// </summary>
        /// <param name="googleKey">The google recaptcha site key of the website with the recaptcha</param>
        /// <param name="pageUrl">the url of the page with the recaptcha challenges</param>
        /// <param name="timeout">Solving timeout (in seconds).</param>
        /// <returns>CAPTCHA if solved, null otherwise.</returns>
        public async Task<Captcha> Decode(string googleKey, string pageUrl, int timeout)
        {
            return await this.Decode(googleKey, pageUrl, timeout, null, null);
        }

        /// <summary>
        /// Upload and wait for a CAPTCHA to be solved. [Tokens(reCAPTCHA v2)]
        /// </summary>
        /// <param name="googleKey">The google recaptcha site key of the website with the recaptcha</param>
        /// <param name="pageUrl">the url of the page with the recaptcha challenges</param>
        /// <param name="timeout">Solving timeout (in seconds).</param>
        /// <param name="proxy">Your proxy url and credentials (if any)</param>
        /// <param name="proxyType">Your proxy connection protocol</param>
        /// <returns>CAPTCHA if solved, null otherwise.</returns>
        public async Task<Captcha> Decode(string googleKey, string pageUrl, int timeout, string proxy, string proxyType)
        {
            var tokenParams = JsonConvert.SerializeObject(new
            {
                proxy = proxy,
                proxytype = proxyType,
                googlekey = googleKey,
                pageurl = pageUrl
            }, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var captcha = await this.Upload(new Hashtable
            {
                { "type", "4" },
                { "token_params", tokenParams },
            });
            return await this.Poll(captcha, timeout);
        }

        /// <summary>
        /// Upload a CAPTCHA.
        /// </summary>
        /// <param name="extraData">Extra data used by special captchas types.</param>
        /// <returns>Uploaded CAPTCHA, null if failed.</returns>
        public async Task<Captcha> Upload(Hashtable extraData)
        {
            var boundary = this.Boundary();

            var args = this.Credentials();

            if (extraData != null)
                foreach (DictionaryEntry ea in extraData)
                    args[ea.Key.ToString()] = ea.Value.ToString();

            var rawArgs = new string[args.Count + 2];
            int i = 0;
            foreach (DictionaryEntry e in args)
            {
                string v = (string)e.Value;
                rawArgs[i++] = String.Join("\r\n", new string[] {
                    "--" + boundary,
                    "Content-Disposition: form-data; name=\"" + (string)e.Key + "\"",
                    "Content-Length: " + v.Length,
                    "",
                    v
                });
            }

            byte[] hdr = this.encoding.GetBytes(String.Join("\r\n", rawArgs));
            byte[] ftr = this.encoding.GetBytes("\r\n--" + boundary + "--\r\n");
            byte[] body = new byte[hdr.Length + ftr.Length];
            hdr.CopyTo(body, 0);
            ftr.CopyTo(body, hdr.Length);

            var content = this.MultiPartFormDataContent(body, boundary);

            var captcha = await this.Call<Captcha>("captcha", content);
            return captcha.Uploaded ? captcha : null;
        }

        /// <summary>
        /// Upload a CAPTCHA.
        /// </summary>
        /// <param name="img">Raw CAPTCHA image.</param>
        /// <param name="extraData">Extra data used by special captchas types.</param>
        /// <returns>Uploaded CAPTCHA, null if failed.</returns>
        public async Task<Captcha> Upload(byte[] img, Hashtable extraData = null)
        {
            var boundary = this.Boundary();

            var args = this.Credentials();

            if (extraData != null)
                foreach (DictionaryEntry ea in extraData)
                    args[ea.Key.ToString()] = ea.Value.ToString();

            var rawArgs = new string[args.Count + 2];
            int i = 0;
            foreach (DictionaryEntry e in args)
            {
                string v = (string)e.Value;
                rawArgs[i++] = String.Join("\r\n", new string[] {
                    "--" + boundary,
                    "Content-Disposition: form-data; name=\"" + (string)e.Key + "\"",
                    "Content-Length: " + v.Length,
                    "",
                    v
                });
            }
            rawArgs[i++] = String.Join("\r\n", new string[] {
                "--" + boundary,
                "Content-Disposition: form-data; name=\"captchafile\"; filename=\"captcha.jpeg\"",
                "Content-Type: application/octet-stream",
                "Content-Length: " + img.Length,
                ""
            });

            byte[] hdr = this.encoding.GetBytes(String.Join("\r\n", rawArgs));
            byte[] ftr = this.encoding.GetBytes("\r\n--" + boundary + "--\r\n");
            byte[] body = new byte[hdr.Length + img.Length + ftr.Length];
            hdr.CopyTo(body, 0);
            img.CopyTo(body, hdr.Length);
            ftr.CopyTo(body, hdr.Length + img.Length);

            var content = this.MultiPartFormDataContent(body, boundary);

            var captcha = await this.Call<Captcha>("captcha", content);
            return captcha.Uploaded ? captcha : null;
        }

        /// <summary>
        /// Wait for a CAPTCHA to be solved
        /// </summary>
        /// <param name="captcha">Uploaded CAPTCHA</param>
        /// <param name="timeout">Solving timeout (in seconds)</param>
        /// <returns>CAPTCHA if solved, null otherwise.</returns>
        public async Task<Captcha> Poll(Captcha captcha, int timeout)
        {
            if (captcha == null || captcha.Id <= 0)
                return null;

            var deadline = DateTime.Now.AddSeconds(timeout > 0 ? timeout : 30);
            while (deadline > DateTime.Now && !captcha.Solved)
            {
                await Task.Delay(2000);
                captcha = await this.GetCaptcha(captcha.Id);
            }

            if (captcha.Solved && captcha.Correct)
                return captcha;
            return null;
        }

        /// <summary>
        /// Wait for a CAPTCHA to be solved
        /// </summary>
        /// <param name="captcha">Uploaded CAPTCHA ID</param>
        /// <param name="timeout">Solving timeout (in seconds)</param>
        /// <returns>CAPTCHA if solved, null otherwise.</returns>
        public async Task<Captcha> Poll(int captchaId, int timeout)
        {
            var captcha = await this.GetCaptcha(captchaId);
            return await this.Poll(captcha, timeout);
        }

        /// <summary>
        /// Get CAPTCHA status
        /// </summary>
        /// <param name="captchaId">CAPTCHA ID.</param>
        /// <returns>CAPTCHA</returns>
        public async Task<Captcha> GetCaptcha(int captchaId)
        {
            return await this.Call<Captcha>($"captcha/{captchaId}");
        }

        /// <summary>
        /// Report an incorrectly solved CAPTCHA.
        /// </summary>
        /// <param name="captchaId">CAPTCHA</param>
        /// <returns>True on success.</returns>
        public async Task<bool> Report(Captcha captcha)
        {
            return await this.Report(captcha.Id);
        }

        /// <summary>
        /// Report an incorrectly solved CAPTCHA.
        /// </summary>
        /// <param name="captchaId">CAPTCHA ID.</param>
        /// <returns>True on success.</returns>
        public async Task<bool> Report(int captchaId)
        {
            if (captchaId <= 0)
                return false;

            var content = this.UrlEncodedContent(this.Credentials());
            var captcha = await this.Call<Captcha>($"captcha/{captchaId}/report", content);
            return !captcha.Correct;
        }

        /// <summary>
        /// Get USER status
        /// </summary>
        /// <returns>USER</returns>
        public async Task<User> UserStatus()
        {
            var content = this.UrlEncodedContent(this.Credentials());
            return await this.Call<User>("user", content);
        }

        /// <summary>
        /// Get SERVER status
        /// </summary>
        /// <returns>SERVER</returns>
        public async Task<Server> ServerStatus()
        {
            return await this.Call<Server>("status");
        }

        private async Task<T> Call<T>(string cmd, HttpContent content = null)
        {
            HttpResponseMessage response = content == null
                ? await this.client.GetAsync(cmd)
                : await this.client.PostAsync(cmd, content);

            switch (response.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    throw new AccessDeniedException("Access denied, check your credentials and/or balance");
                case HttpStatusCode.BadRequest:
                    throw new InvalidCaptchaException("CAPTCHA was rejected, please check if it's a valid image");
                case HttpStatusCode.ServiceUnavailable:
                    throw new ServiceOverloadException("CAPTCHA was rejected due to service overload, try again later");
                case HttpStatusCode.SeeOther:
                case HttpStatusCode.OK:
                    try
                    {
                        var byteArray = await response.Content.ReadAsByteArrayAsync();
                        var stringContent = this.encoding.GetString(byteArray, 0, byteArray.Length);
                        return JsonConvert.DeserializeObject<T>(stringContent);
                    }
                    catch
                    {
                        throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private StringContent UrlEncodedContent(Hashtable args)
        {
            string[] fields = new string[args.Count];
            int i = 0;
            foreach (DictionaryEntry e in args)
            {
                fields[i] = WebUtility.UrlEncode((string)e.Key) + "=" + WebUtility.UrlEncode((string)e.Value);
                i++;
            }
            return new StringContent(string.Join("&", fields), this.encoding, "application/x-www-form-urlencoded");
        }

        private ByteArrayContent MultiPartFormDataContent(byte[] content, string boundary)
        {
            var byteContent = new ByteArrayContent(content);
            byteContent.Headers.Remove("Content-Type");
            byteContent.Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);
            return byteContent;
        }

        private Hashtable Credentials()
        {
            return new Hashtable
            {
                {  "username", this.username },
                {  "password", this.password },
            };
        }

        private string Boundary()
        {
            using (var cryptoServiceProvider = new SHA1CryptoServiceProvider())
                return BitConverter
                    .ToString(cryptoServiceProvider.ComputeHash(this.encoding.GetBytes(DateTime.Now.ToString("G"))))
                    .Replace("-", string.Empty);
        }
    }
}
