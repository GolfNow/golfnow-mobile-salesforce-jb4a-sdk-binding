using System;
using System.Threading.Tasks;
using GolfNow.Mobile.Salesforce.Providers;
using JB4ASDK;

namespace GolfNow.Mobile.Salesforce.iOS.Providers
{
    public class MarketingCloudSubscriberKeyProvider : IMarketingCloudSubscriberKeyProvider
    {
        public Task<string> GetSubscriberKey()
        {
            string subscriberKey = null;

            try
            {
                subscriberKey = ETPush.PushManager().SubscriberKey;
            }
            catch (Exception e)
            {
                OnException(e);
            }

            return Task.FromResult(subscriberKey);
        }

        public async Task<bool> SetSubscriberKey(string subscriberKey)
        {
            try
            {
                return ETPush.PushManager().SetSubscriberKey(subscriberKey);
            }
            catch (Exception e)
            {
                OnException(e);
            }

            return false;
        }

        protected virtual void OnException(Exception e) { }
    }
}
