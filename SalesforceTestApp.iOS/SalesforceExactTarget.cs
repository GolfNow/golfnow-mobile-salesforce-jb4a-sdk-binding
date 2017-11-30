using System;
using Foundation;
using JB4ASDK;
using UIKit;

namespace SalesforceTestApp.iOS
{
    public partial class AppDelegate
    {
        // Using GN 2.0 QA/Debug SFMC config
        const string kETAppID_Debug = @"9f8dfe7b-6643-4265-9b8c-a6ad1cc8866d";
        const string kETAccessToken_Debug = @"semsxbgwbfkfd96zy8kxz64h";

        ExactTargetOpenDirectDelegate openDirectDelegate;
        ETCloudPageWithAlertDelegate cloudPageDelegate;

        public void InitializeExactTarget(UIApplication application, NSDictionary launchOptions)
        {
            /*
             successful = [[ETPush pushManager] configureSDKWithAppID:kETAppID_Debug
                                              andAccessToken:kETAccessToken_Debug
                                               withAnalytics:YES
                                         andLocationServices:NO
                                        andProximityServices:NO
                                               andCloudPages:YES
                                             withPIAnalytics:YES
                                                       error:&error];
                                                       */

            var successful = ETPush.PushManager().ConfigureSDKWithAppID(
                kETAppID_Debug,
                kETAccessToken_Debug,
                true,
                false,
                false,
                true,
                true,
                out NSError error
            );

            if (!successful || error != null)
            {
                var alertViewController = UIAlertController.Create("Failed to configure ExactTarget", error?.Description, UIAlertControllerStyle.Alert);
                alertViewController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                Window.RootViewController.PresentViewController(alertViewController, true, null);
            }
            else
            {
                /*
                [[ETPush pushManager] registerUserNotificationSettings:settings];
                [[ETPush pushManager] registerForRemoteNotifications];
                
                [[ETPush pushManager] setOpenDirectDelegate:self];
                [[ETPush pushManager] setCloudPageWithAlertDelegate:self];

                [[ETPush pushManager] applicationLaunchedWithOptions:launchOptions];
                */

                var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                    null
                );

                ETPush.PushManager().RegisterUserNotificationSettings(notificationSettings);
                ETPush.PushManager().RegisterForRemoteNotifications();

                openDirectDelegate = new ETOpenDirectDelegate();
                cloudPageDelegate = new ETCloudPageWithAlertDelegate();

                ETPush.PushManager().OpenDirectDelegate = openDirectDelegate;
                ETPush.PushManager().CloudPageWithAlertDelegate = cloudPageDelegate;

                ETPush.PushManager().ApplicationLaunchedWithOptions(launchOptions);
            }
        }

        public class ETOpenDirectDelegate : ExactTargetOpenDirectDelegate
        {
            public override bool ShouldDeliverOpenDirectMessageIfAppIsRunning()
            {
                return true;
            }

            public override void DidReceiveOpenDirectMessageWithContents(string payload)
            {
                var landingPagePresenter = new ETWKLandingPagePresenter(payload);
                UIApplication.SharedApplication.
                             KeyWindow.
                             RootViewController.
                             PresentViewController(landingPagePresenter, true, null);
            }
        }

        public class ETCloudPageWithAlertDelegate : ExactTargetCloudPageWithAlertDelegate
        {
            public override bool ShouldDeliverCloudPageWithAlertMessageIfAppIsRunning()
            {
                return true;
            }

            public override void DidReceiveCloudPageWithAlertMessageWithContents(string payload)
            {
                var landingPagePresenter = new ETWKLandingPagePresenter(payload);
                UIApplication.SharedApplication.
                             KeyWindow.
                             RootViewController.
                             PresentViewController(landingPagePresenter, true, null);
            }
        }
    }
}
