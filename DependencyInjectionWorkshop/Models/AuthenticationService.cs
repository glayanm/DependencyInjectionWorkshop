using DependencyInjectionWorkshop.Models.Repository;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly FailedCounter _failedCounter;
        private readonly NLogAdapter _nLogAdapter;
        private readonly OtpService _otpService;
        private readonly IProfile _profile;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService(FailedCounter failedCounter, NLogAdapter nLogAdapter, OtpService otpService, IProfile profile, Sha256Adapter sha256Adapter, SlackAdapter slackAdapter)
        {
            _failedCounter = failedCounter;
            _nLogAdapter = nLogAdapter;
            _otpService = otpService;
            _profile = profile;
            _sha256Adapter = sha256Adapter;
            _slackAdapter = slackAdapter;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isLocked = _failedCounter.GetIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);

                var failedCount = _failedCounter.GetFailedCount(accountId);

                _nLogAdapter.LogMessage($"accountId:{accountId} failed times:{failedCount}");

                _slackAdapter.Notify(accountId);

                return false;
            }
        }
    }
}