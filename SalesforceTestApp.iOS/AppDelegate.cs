using Foundation;
using JB4ASDK;
using UIKit;

namespace SalesforceTestApp.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            InitializeExactTarget(application, launchOptions);
                
            return true;
        }

        #region Notification Related Methods

        public override void DidRegisterUserNotificationSettings(UIApplication application, UIUserNotificationSettings notificationSettings)
        {
            ETPush.PushManager().DidRegisterUserNotificationSettings(notificationSettings);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            ETPush.PushManager().RegisterDeviceToken(deviceToken);
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            ETPush.PushManager().ApplicationDidFailToRegisterForRemoteNotificationsWithError(error);
        }

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            ETPush.PushManager().HandleLocalNotification(notification);
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            ETPush.PushManager().HandleNotification(userInfo, application.ApplicationState);
        }

        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, System.Action<UIBackgroundFetchResult> completionHandler)
        {
            ETPush.PushManager().HandleNotification(userInfo, application.ApplicationState);

            completionHandler?.Invoke(UIBackgroundFetchResult.NoData);
        }

        public override void HandleAction(UIApplication application, string actionIdentifier, NSDictionary remoteNotificationInfo, System.Action completionHandler)
        {
            ETPush.PushManager().HandleNotification(remoteNotificationInfo, application.ApplicationState);

            completionHandler?.Invoke();
        }

        public override void PerformFetch(UIApplication application, System.Action<UIBackgroundFetchResult> completionHandler)
        {
            ETPush.PushManager().RefreshWithFetchCompletionHandler(completionHandler);
        }

        #endregion

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}

