using System;
using System.Threading.Tasks;

namespace GolfNow.Mobile.Salesforce.Providers
{
    public interface ICustomerSubscriberKeyProvider
    {
        /// <summary>
        /// Gets the subscriber key for the given customer.
        /// </summary>
        /// <param name="emailAddress">The customer's email address.</param>
        Task<string> GetSubscriberKey(string emailAddress);
    }
}
