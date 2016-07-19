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
            var stations = JsonConvert.DeserializeObject<IEnumerable<Station>>("http://listen.di.fm/public3".GetStringAsync().Result);
            const string plsFile = "Digitally Imported.pls";

            string text = String.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("[playlist]");
            sb.AppendLine("NumberOfEntries=" + stations.Count());
            var ij = 0;
            foreach(var station in stations.OrderBy(x => x.Name))
            {
                ij++;
                sb.AppendLine(String.Format(@"File{0}=http://prem2.di.fm:80/{1}_hi?09f33f12640bf313a5737e1e", ij, station.Key));
                sb.AppendLine(String.Format("Title{0}=Digitally Imported - {1}", ij, station.Name));
                sb.AppendLine(String.Format("Length{0}=-1", ij));
            }
            sb.AppendLine("Version=2");

            text = sb.ToString();

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
