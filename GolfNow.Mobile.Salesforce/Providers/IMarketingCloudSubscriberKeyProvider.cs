using System;
using System.Threading.Tasks;

namespace GolfNow.Mobile.Salesforce.Providers
{
    public interface IMarketingCloudSubscriberKeyProvider
    {
        /// <summary>
        /// Gets the subscriber key from the Marketing Cloud SDK.
        /// </summary>
        Task<string> GetSubscriberKey();

        /// <summary>
        /// Sets the subscriber key to the Marketing Cloud SDK.
        /// </summary>
        Task SetSubscriberKey(string subscriberKey);
    }
}
