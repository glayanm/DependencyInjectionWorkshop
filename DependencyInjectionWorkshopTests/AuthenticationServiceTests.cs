using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Models.Repository;
using NSubstitute;
using NUnit.Framework;
using System;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "joey";
        private const int DefaultFailedCount = 99;
        private const string DefaultHashedPassword = "my hashed password";
        private const string DefaultInputPassword = "abc";
        private const string DefaultOtp = "123456";
        private AuthenticationService _authenticationService;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtpService _otpService;
        private IProfile _profile;

        [Test]
        public void account_is_locked()
        {
            GivenFailedCount(DefaultAccountId, true);
            ShouldBeThrow<FailedTooManyTimesException>();
        }

        [Test]
        public void is_invalid()
        {
            var isValid = WhenInvalid();

            ShouldBeInvalid(isValid);
        }

        [Test]
        public void is_valid()
        {
            ShouldBeValid(WhenValid());
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultAccountId, DefaultFailedCount);
            WhenInvalid();
            LogShouldContain(DefaultAccountId, DefaultFailedCount.ToString());
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldBeSend(DefaultAccountId);
        }

        [Test]
        public void Reset_failed_count_when_valid()
        {
            WhenValid();
            ShouldResetFailedCount();
        }

        [SetUp]
        public void SetUp()
        {
            _notification = Substitute.For<INotification>();
            _logger = Substitute.For<ILogger>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _otpService = Substitute.For<IOtpService>();
            _hash = Substitute.For<IHash>();
            _profile = Substitute.For<IProfile>();

            _authenticationService =
                new AuthenticationService(_failedCounter, _logger, _otpService, _profile, _hash, _notification);
        }
        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private void GivenFailedCount(string accountId, int failedCount)
        {
            _failedCounter.GetFailedCount(accountId).Returns(failedCount);
        }

        private void GivenFailedCount(string accountId, bool isLocked)
        {
            _failedCounter.GetIsLocked(accountId).Returns(isLocked);
        }

        private void GivenHash(string inputPassword, string hashedPassword)
        {
            _hash.Compute(inputPassword).Returns(hashedPassword);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetCurrentOtp(accountId).Returns(otp);
        }

        private void GivenPassword(string accountId, string password)
        {
            _profile.GetPassword(accountId).Returns(password);
        }

        private void LogShouldContain(string accountId, string failedAccount)
        {
            _logger.Info(Arg.Is<string>(p => p.Contains(accountId) && p.Contains(failedAccount)));
        }

        private void ShouldBeSend(string accountId)
        {
            _notification.Received(1).Send(accountId);
        }

        private void ShouldBeThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => _authenticationService.Verify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
            Assert.Throws<TException>(action);
        }

        private void ShouldResetFailedCount()
        {
            _failedCounter.Received(1).ResetFailedCount(DefaultAccountId);
        }

        private bool WhenInvalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, "Wrong Otp.");
            return isValid;
        }

        private bool WhenValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
            return isValid;
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            return _authenticationService.Verify(accountId, password, otp);
        }
    }
}