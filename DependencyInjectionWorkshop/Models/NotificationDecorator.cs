namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : AuthenticationBaseDecorator
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification) : base(authentication)
        {
            _notification = notification;
        }

        private void Send(string accountId)
        {
            _notification.Send(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isVerify1 = base.Verify(accountId, password, otp);
            var isVerify = isVerify1;
            if (!isVerify)
            {
                Send(accountId);
            }

            return isVerify;
        }
    }
}