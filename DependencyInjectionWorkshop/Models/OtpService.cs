using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpService
    {
        public OtpService()
        {
        }

        public string GetCurrentOtp(string accountId)
        {
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", accountId).Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }
        }
    }
}