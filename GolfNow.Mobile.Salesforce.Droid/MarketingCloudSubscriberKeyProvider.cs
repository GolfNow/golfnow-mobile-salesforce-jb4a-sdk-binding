using System;
using System.Threading.Tasks;
using Com.Salesforce.Marketingcloud;
using GolfNow.Mobile.Salesforce.Providers;

namespace GolfNow.Mobile.Salesforce.Droid.Providers
{
    public class MarketingCloudSubscriberKeyProvider : IMarketingCloudSubscriberKeyProvider
    {
        public Task<string> GetSubscriberKey()
        {
            var cloudSdk = MarketingCloudSdk.Instance;

            if (cloudSdk == null || !cloudSdk.InitializationStatus.IsUsable)
            {
                return Task.FromResult<string>(null);
            }

            return Task.FromResult(cloudSdk.RegistrationManager.ContactKey);


        }

        public Task SetSubscriberKey(string subscriberKey)
        {
            var cloudSdk = MarketingCloudSdk.Instance;

            if (cloudSdk == null || !cloudSdk.InitializationStatus.IsUsable)
            {
                return Task.FromException(new TaskCanceledException("MarketingCloudSdk is not usable."));
            }

            var registrationManager = cloudSdk.RegistrationManager;

            return Task.Factory.StartNew(() =>
            {
                registrationManager
                .Edit()
                .SetContactKey(subscriberKey)
                .Commit();
            });


        }
    }
}
