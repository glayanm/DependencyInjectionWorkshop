using Dapper;
using SlackAPI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string account, string password, string otp)
        {
            var passwordFromDb = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new { Id = account },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();

            var currentOtp = "";
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (response.IsSuccessStatusCode)
            {
                currentOtp = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {

                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;

                resetResponse.EnsureSuccessStatusCode();

                return true;
            }
            else
            {

                var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
    
                addFailedCountResponse.EnsureSuccessStatusCode();

                var message = $"{account} try to login failed.";
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");

                return false;
            }
        }
    }
}