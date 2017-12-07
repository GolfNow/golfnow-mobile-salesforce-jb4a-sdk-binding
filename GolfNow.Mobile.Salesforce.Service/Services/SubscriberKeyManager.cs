using System;
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
        /// The subscriber key for a guest customer.
        /// </summary>
        public string GuestSubscriberKey { get; set; }

        /// <summary>
        /// The date when the subscriber key was last fetched from the GolfNow API.
        /// </summary>
        public DateTime LastFetchDate { get; set; }

        /// <summary>
        /// Whether this data is "dirty" i.e. has non-persisted changes.
        /// </summary>
        public bool Dirty { get; set; }


        /// <summary>
        /// Initializes an instance.
        /// </summary>
        public SubscriberData()
        {
            CustomerSubscriberKey = null;
            GuestSubscriberKey = GenerateSubscriberKey();
            LastFetchDate = DateTime.MinValue;
            Dirty = true;
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

        }

        /// <summary>
        /// Initializes the SubscriberKeyManager. Invoke this method first before use.
        /// </summary>
        public Task Initialize()
        {
            // Register authentication service events
            authService.AuthenticationSuccess += OnAuthenticationSuccess;
            authService.LogOutSuccess += OnLogOutSuccess;

            return LoadSubscriberDataIfNeeded();
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            authService.AuthenticationSuccess -= OnAuthenticationSuccess;
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
            if (SubscriberData == null)
            {
                throw new InvalidOperationException("SubscriberMetadata is null. Did you forget to invoke Initialize()?");
            }

            Task.Run(async () =>
            {
                try
                {
                    // If forced to do an update, reset the subscriber metadata
                    if (forceUpdate)
                    {
                        ResetSubscriberData();
                    }

                    // Check if updating the subscriber key for an authenticated user is necessary
                    if (string.IsNullOrEmpty(SubscriberData.CustomerSubscriberKey) && await ShouldQueryForCustomerSubscriberKey())
                    {
                        // Update the metadata and mark it as dirty so that is is persisted
                        SubscriberData.CustomerSubscriberKey = await GetCustomerSubscriberKey();
                        SubscriberData.LastFetchDate = DateTime.Now;
                        SubscriberData.Dirty = true;
                    }

                    // Only update the subscriber metadata if needed
                    if (SubscriberData.Dirty)
                    {
                        // Set the subscriber key in the Marketing Cloud SDK
                        await marketingCloudProvider.SetSubscriberKey(SubscriberData.SubscriberKey);

                        // Save the updated subscriber metadata to persistent store
                        SaveSubscriberData();
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    SubscriberData.Dirty = true;
                }
            });
        }

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
                // Check if we should fetch for a GolfNow subscriber key
                if (DateTime.Now >= SubscriberData?.LastFetchDate.AddMinutes(MinimumCustomerSubscriberKeyFetchInterval))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the customer subscriber key by querying the GolfNow provider.
        /// </summary>
        async Task<string> GetCustomerSubscriberKey()
        {
            string subscriberKey = null;

            var user = await authService.GetAuthenticatedUserAsync();

            if (!string.IsNullOrEmpty(user?.EmailAddress))
            {
                subscriberKey = await customerProvider.GetSubscriberKey(user.EmailAddress);
            }

            return subscriberKey;
        }

        /// <summary>
        /// Saves the subscriber metadata to persistent storage.
        /// </summary>
        void SaveSubscriberData()
        {
            if (SubscriberData != null)
            {
                SubscriberData.Dirty = false;

                cacheProvider.InsertObjectAsync(SUBSCRIBER_DATA, SubscriberData);
            }
        }

        /// <summary>
        /// Resets the subscriber metadata by deleting the customer subscriber key.
        /// </summary>
        void ResetSubscriberData()
        {
            cacheProvider.InvalidateObjectAsync<SubscriberData>(SUBSCRIBER_DATA);

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
        void OnLogOutSuccess(object sender, LogOutSuccessEventArgs e)
        {
            UpdateSubscriberKey();
        }
    }
}
