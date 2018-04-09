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
            string subscriberKey = null;

            try
            {
                var cloudSdk = MarketingCloudSdk.Instance;

                if (cloudSdk != null || cloudSdk.InitializationStatus.IsUsable)
                {
                    subscriberKey = cloudSdk.RegistrationManager.ContactKey;
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }

            return Task.FromResult(subscriberKey);
        }

        public Task<bool> SetSubscriberKey(string subscriberKey)
        {
            bool result = false;

            try
            {
                var cloudSdk = MarketingCloudSdk.Instance;

                if (cloudSdk != null || cloudSdk.InitializationStatus.IsUsable)
                {
                    var registrationManager = cloudSdk.RegistrationManager;

                    result = registrationManager
                        .Edit()
                        .SetContactKey(subscriberKey)
                        .Commit();
                }
            }
            catch (Exception e)
            {
                OnException(e);
            }

            return Task.FromResult(result);
        }

        protected virtual void OnException(Exception e) { }
    }
}
