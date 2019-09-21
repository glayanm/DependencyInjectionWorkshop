using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackAdapter
    {
        public SlackAdapter()
        {
        }

        public void Notify(string accountId)
        {
            var message = $"{accountId} try to login failed.";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }
}