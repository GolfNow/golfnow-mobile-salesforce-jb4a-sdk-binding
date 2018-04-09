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

        public Task<bool> SetSubscriberKey(string subscriberKey)
        {
            bool result = false;

            try
            {
                result = ETPush.PushManager().SetSubscriberKey(subscriberKey);
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
