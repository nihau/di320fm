using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Flurl.Http;
using Flurl;

namespace di320fm
{
    static class Program
    {
        private const string UserUri = @"https://api.audioaddict.com/v1/di/members";

        private const string HttpAuthUsername = "ephemeron";
        private const string HttpAuthPwd = "dayeiph0ne@pp";

        private static void Main()
        {
            var email = Helper.RandomString(10) + "@google.com";
            var password = Helper.RandomString(10);
            const string firstName = "nihau";
            const string lastName = "1337";
            User me;

            var credentials = new NetworkCredential(HttpAuthUsername, HttpAuthPwd);

            using (var handler = new HttpClientHandler { Credentials = credentials })
            using (var client = new HttpClient(handler))
            {
                //register
                //done in such way coz Flurl can't work with non-standard api keys
                var registerParams = new Dictionary<string, string>
                {
                   { "member[email]", email },
                   { "member[first_name]", firstName },
                   { "member[last_name]", lastName },
                   { "member[password]", password },
                   { "member[password_confirmation]", password },
                };

                var response = client.PostAsync(UserUri, new FormUrlEncodedContent(registerParams));
                var jsonResponse = response.Result.Content.ReadAsStringAsync();
                me = JsonConvert.DeserializeObject<User>(jsonResponse.Result);

                //confirmation
                "http://www.di.fm/member/confirm/".AppendPathSegment(me.ConfirmationToken).GetAsync().Wait();

                //activate trial 
                var trialGet =
                    UserUri.AppendPathSegment("/1/subscriptions/trial/premium-pass")
                    .SetQueryParams(new { api_key = me.ApiKey })
                    .WithBasicAuth("ephemeron", "dayeiph0ne@pp")
                    .PostAsync()
                    .ReceiveJson();

                //must login after activating
                "https://api.audioaddict.com/v1/di/members/authenticate"
                    .SetQueryParams(new { api_key = me.ApiKey })
                    .WithBasicAuth("ephemeron", "dayeiph0ne@pp")
                    .PostAsync()
                    .ReceiveJson()
                    .Wait();
            }

            Console.WriteLine("New key " + me.ListenKey);
            ChangeListenKey(me.ListenKey);
        }

        /// <summary>
        /// OH SUCH DIRTY WOW
        /// </summary>
        /// <param name="newKey">gimme your key brah, i know you got it</param>
        private static void ChangeListenKey(string newKey)
        {
            const string plsFile = "Digitally Imported.pls";

            string text = String.Empty;

            if (!File.Exists(plsFile))
            {
                var assembly = Assembly.GetExecutingAssembly();
                const string resourceName = "di320fm." + plsFile;

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            else
            {
                text = File.ReadAllText(plsFile);   
            }

            int i = -1;

            for (; ; )
            {
                i = text.IndexOf("?", i + 1);

                if (i < 0)
                    break;

                var j = text.IndexOf("\n", i);

                text = text.Substring(0, i + 1) + newKey + text.Substring(j, text.Length - j);
            }

            File.WriteAllText(Path.GetFileNameWithoutExtension(plsFile) + " updated.pls", text);
        }
    }
}
