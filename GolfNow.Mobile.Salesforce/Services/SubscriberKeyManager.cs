﻿using System;
using System.Threading.Tasks;
using Golfnow.Mobile.Services.AuthenticationService;
using Golfnow.Mobile.Services.Common.Providers;
using GolfNow.Mobile.Salesforce.Providers;

namespace GolfNow.Mobile.Salesforce.Services
{
    /// <summary>
    /// Defines a data object holding subscriber information.
    /// </summary>
    public class SubscriberData
    {
        /// <summary>
        /// Gets the current (active) subscriber key.
        /// </summary>
        public string SubscriberKey => (string.IsNullOrEmpty(CustomerSubscriberKey)) ? GuestSubscriberKey : CustomerSubscriberKey;

        /// <summary>
        /// Gets whether the current subscriber key corresponds to a guest.
        /// </summary>
        public bool IsGuest => (string.Equals(SubscriberKey, GuestSubscriberKey));


        /// <summary>
        /// The subscriber key for a known customer.
        /// </summary>
        public string CustomerSubscriberKey { get; set; }

        /// <summary>
        /// The email address for a known customer.
        /// </summary>
        public string CustomerEmailAddress { get; set; }

        /// <summary>
        /// The subscriber key for a guest customer.
        /// </summary>
        public string GuestSubscriberKey { get; set; }

        /// <summary>
        /// The date when the subscriber key was last fetched from the GolfNow API.
        /// </summary>
        public DateTime LastFetchDate { get; set; }


        /// <summary>
        /// Initializes an instance.
        /// </summary>
        public SubscriberData()
        {
            CustomerSubscriberKey = null;
            CustomerEmailAddress = null;
            GuestSubscriberKey = GenerateSubscriberKey();
            LastFetchDate = DateTime.MinValue;
        }

        /// <summary>
        /// Generates a subscriber key for use with guests.
        /// </summary>
        public static string GenerateSubscriberKey()
        {
            return Guid.NewGuid().ToString();
        }
    }


    public class SubscriberKeyManager : IDisposable
    {
        public class InvalidSubscriberKeyException : Exception
        {
            public InvalidSubscriberKeyException() { }
            public InvalidSubscriberKeyException(string message) : base(message) { }
        }

        // Cache keys
        public static string SUBSCRIBER_DATA = "SubscriberKeyManager.SubscriberData";

        /// <summary>
        /// The minimum customer subscriber key fetch interval in minutes. This is the minimum time period that
        /// has to elapse before another request to fetch the customer subscriber key for an authenticated user is attempted.
        /// </summary>
        public static double MinimumCustomerSubscriberKeyFetchInterval = 720.0f;


        /// <summary>
        /// Gets the current subscriber data in persistent storage. Returns null if one is not set.
        /// </summary>
        public SubscriberData SubscriberData { get; private set; }


        readonly ISecureCacheProvider cacheProvider;
        readonly IAuthenticationService authService;
        readonly ICustomerSubscriberKeyProvider customerProvider;
        readonly IMarketingCloudSubscriberKeyProvider marketingCloudProvider;

        public SubscriberKeyManager(ISecureCacheProvider cacheProvider,
                                    IAuthenticationService authService,
                                    ICustomerSubscriberKeyProvider customerProvider,
                                    IMarketingCloudSubscriberKeyProvider marketingCloudProvider
                                   )
        {
            this.cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
            this.authService = authService ?? throw new ArgumentNullException(nameof(authService));
            this.customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            this.marketingCloudProvider = marketingCloudProvider ?? throw new ArgumentNullException(nameof(marketingCloudProvider));

            // Register authentication service events
            this.authService.AuthenticationSuccess += OnAuthenticationSuccess;
            this.authService.AuthenticationError += OnAuthenticationError;
            this.authService.LogOutSuccess += OnLogOutSuccess;
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            authService.AuthenticationSuccess -= OnAuthenticationSuccess;
            authService.AuthenticationError -= OnAuthenticationError;
            authService.LogOutSuccess -= OnLogOutSuccess;
        }

        /// <summary>
        /// Forces a subscriber key update by invoking UpdateSubscriberKeyIfNeeded(true).
        /// </summary>
        public void UpdateSubscriberKey()
        {
            UpdateSubscriberKeyIfNeeded(true);
        }

        /// <summary>
        /// Updates the subscriber key only if needed. This method doest not make a subscriber key change if one is already set.
        /// </summary>
        public void UpdateSubscriberKeyIfNeeded(bool forceUpdate = false)
        {
            Task.Run(async () =>
            {
                try
                {
                    await LoadSubscriberDataIfNeeded();

                    // If forced to do an update, reset the subscriber metadata
                    if (forceUpdate)
                    {
                        await ResetSubscriberData();
                    }

                    // Check if updating the subscriber key for an authenticated user is necessary
                    if (await ShouldQueryForCustomerSubscriberKey())
                    {
                        // Update the metadata and mark it as dirty so that is is persisted
                        SubscriberData.CustomerEmailAddress = await GetCustomerEmailAddress();
                        SubscriberData.CustomerSubscriberKey = await GetCustomerSubscriberKey();
                        SubscriberData.LastFetchDate = DateTime.Now;
                    }

                    // Throw error for invalid subscriber key
                    if (string.IsNullOrEmpty(SubscriberData.SubscriberKey))
                    {
                        throw new InvalidSubscriberKeyException(
                            "SubscriberData: `SubscriberKey` cannot be null or blank. Ensure that `GuestSubscriberKey` is set in case `CustomerSubscriberKey` is null."
                        );
                    }

                    // Set the subscriber key in the Marketing Cloud SDK
                    await marketingCloudProvider.SetSubscriberKey(SubscriberData.SubscriberKey);

                    // Save the updated subscriber metadata to persistent store
                    await SaveSubscriberData();
                }
                catch (Exception e)
                {
                    HandleSubscriberKeyUpdateError(e);
                }
            });
        }

        /// <summary>
        /// Invoked when a subscriber key update operation resulted in an error.
        /// </summary>
        protected virtual void HandleSubscriberKeyUpdateError(Exception error) { }

        /// <summary>
        /// Loads and sets the current subscriber metadata from persistent storage.
        /// </summary>
        async Task LoadSubscriberDataIfNeeded()
        {
            if (SubscriberData == null)
            {
                var data = await cacheProvider.GetObjectAsync<SubscriberData>(SUBSCRIBER_DATA);

                SubscriberData = data ?? new SubscriberData();
            }
        }

        /// <summary>
        /// Returns whether a query to fetch the customer subscriber key should occur.
        /// </summary>
        async Task<bool> ShouldQueryForCustomerSubscriberKey()
        {
            if (await authService.IsAuthenticated())
            {
                // Check if there's a discrepancy with the stored customer email and the current customer email
                if(!string.Equals(SubscriberData.CustomerEmailAddress, await GetCustomerEmailAddress()))
                {
                    return true;
                }

                // Enforce a minimum time interval before a query to fetch the customer subscriber should occur
                bool isPastWaitingPeriod = (DateTime.Now >= SubscriberData?.LastFetchDate.AddMinutes(MinimumCustomerSubscriberKeyFetchInterval));
                    
                // Check if we should fetch for a customer subscriber key
                if (string.IsNullOrEmpty(SubscriberData.CustomerSubscriberKey) && isPastWaitingPeriod)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the customer's email address.
        /// </summary>
        async Task<string> GetCustomerEmailAddress()
        {
            var user = await authService.GetUserProfileAsync();

            return user?.EmailAddress ?? null;
        }

        /// <summary>
        /// Gets the customer subscriber key by querying the GolfNow provider.
        /// </summary>
        async Task<string> GetCustomerSubscriberKey()
        {
            string subscriberKey = null;

            var emailAddress = SubscriberData.CustomerEmailAddress;

            if (!string.IsNullOrEmpty(emailAddress))
            {
                subscriberKey = await customerProvider.GetSubscriberKey(emailAddress);
            }

            return subscriberKey;
        }

        /// <summary>
        /// Saves the subscriber metadata to persistent storage.
        /// </summary>
        async Task SaveSubscriberData()
        {
            if (SubscriberData != null)
            {
                await cacheProvider.InsertObjectAsync(SUBSCRIBER_DATA, SubscriberData);
            }
        }

        /// <summary>
        /// Resets the subscriber metadata by deleting the customer subscriber key.
        /// </summary>
        async Task ResetSubscriberData()
        {
            await cacheProvider.InvalidateObjectAsync<SubscriberData>(SUBSCRIBER_DATA);

            // Create a new instance of subscriber metadata, but retain the same guest subscriber key
            SubscriberData = new SubscriberData()
            {
                GuestSubscriberKey = SubscriberData.GuestSubscriberKey
            };
        }

        /// <summary>
        /// Invoked when the app authentication has changed.
        /// </summary>
        void OnAuthenticationSuccess(object sender, AuthenticationSuccessEventArgs e)
        {
            UpdateSubscriberKey();
        }

        /// <summary>
        /// Invoked when the app authentication has changed.
        /// </summary>
        void OnAuthenticationError(object sender, AuthenticationErrorEventArgs e)
        {
            UpdateSubscriberKey();
        }

        /// <summary>
        /// Invoked when the app authentication has changed.
        /// </summary>
        void OnLogOutSuccess(object sender, LogOutSuccessEventArgs e)
        {
            UpdateSubscriberKey();
        }
    }
}
