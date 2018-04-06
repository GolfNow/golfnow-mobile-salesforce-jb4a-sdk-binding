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

        public async Task<bool> SetSubscriberKey(string subscriberKey)
        {
            return await SetSubKey(subscriberKey);
        }

        public async Task<bool> SetSubKey(string subscriberKey)
        {
            try
            {
                return ETPush.PushManager().SetSubscriberKey(subscriberKey);

            }
            catch (Exception ex)
            {
                //Add Logging
            }

            return false;
        }


    }
}
