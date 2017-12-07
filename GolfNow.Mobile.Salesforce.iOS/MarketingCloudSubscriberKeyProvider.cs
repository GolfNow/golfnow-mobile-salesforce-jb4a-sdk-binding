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
            return Task.FromResult(ETPush.PushManager().SubscriberKey);
        }

        public Task SetSubscriberKey(string subscriberKey)
        {
            return Task.Factory.StartNew(() => ETPush.PushManager().SetSubscriberKey(subscriberKey));
        }
    }
}
