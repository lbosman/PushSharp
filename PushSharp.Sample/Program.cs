using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Windows;
using PushSharp.WindowsPhone;

//using PushSharp.Android;
//using PushSharp.WindowsPhone;
//using PushSharp.Windows;

namespace PushSharp.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
			SendNotification("c9bf4367887a6579618eaca33562ac8b22f3e1a3fd182493316dc7d7ab358e6b", "Titel1", "Een", 1, "");
			SendNotification("c9bf4367887a6579618eaca33562ac8b22f3e1a3fd182493316dc7d7ab358e6b", "Titel2", "Twee", 2, "");
			SendNotification("c9bf4367887a6579618eaca33562ac8b22f3e1a3fd182493316dc7d7ab358e6b", "Titel3", "Drie", 3, "");
            SendNotification("c9bf4367887a6579618eaca33562ac8b22f3e1a3fd182493316dc7d7ab358e6b", "", "", 0, "");
			SendNotification("c9bf4367887a6579618eaca33562ac8b22f3e1a3fd182493316dc7d7ab358e6b", "Titel4", "Vier", 4, "");

        }

        private static void ReceiveFeedback()
        {
            FeedbackService fs = new FeedbackService();
            fs.OnFeedbackReceived += Fs_OnFeedbackReceived;

            //			var appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../Resources/PushSharp.Apns.Sandbox.p12"));
            //			ApplePushChannelSettings appleSettings = new ApplePushChannelSettings (false, appleCert, "Lize4Rune");

            var appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../Resources/OLBPhoneSandboxCertificate.p12"));
            ApplePushChannelSettings appleSettings = new ApplePushChannelSettings(false, appleCert, "0lbSandb0x");

            fs.Run(appleSettings);
        }

        static void Fs_OnFeedbackReceived(string deviceToken, DateTime timestamp)
        {
            Console.WriteLine(string.Format("Feedback received for token {0} on {1}", deviceToken, timestamp));
        }

		private static void SendNotification(string deviceToken, string title, string body, int badgeCount, string sound)
        {
            //Create our push services broker
            var push = new PushBroker();

            //Wire up the events for all the services that the broker registers
            push.OnNotificationSent += NotificationSent;
            push.OnChannelException += ChannelException;
            push.OnServiceException += ServiceException;
            push.OnNotificationFailed += NotificationFailed;
            push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpired;
            push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
            push.OnChannelCreated += ChannelCreated;
            push.OnChannelDestroyed += ChannelDestroyed;


            //------------------------------------------------
            //IMPORTANT NOTE about Push Service Registrations
            //------------------------------------------------
            //Some of the methods in this sample such as 'RegisterAppleServices' depend on you referencing the correct
            //assemblies, and having the correct 'using PushSharp;' in your file since they are extension methods!!!

            // If you don't want to use the extension method helpers you can register a service like this:
            //push.RegisterService<WindowsPhoneToastNotification>(new WindowsPhonePushService());

            //If you register your services like this, you must register the service for each type of notification
            //you want it to handle.  In the case of WindowsPhone, there are several notification types!

            //-------------------------
            // APPLE NOTIFICATIONS
            //-------------------------
            //Configure and start Apple APNS
            // IMPORTANT: Make sure you use the right Push certificate.  Apple allows you to generate one for connecting to Sandbox,
            //   and one for connecting to Production.  You must use the right one, to match the provisioning profile you build your
            //   app with!
            var appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Resources/OLBPhoneSandboxCertificate.p12"));
            //IMPORTANT: If you are using a Development provisioning Profile, you must use the Sandbox push notification server 
            //  (so you would leave the first arg in the ctor of ApplePushChannelSettings as 'false')
            //  If you are using an AdHoc or AppStore provisioning profile, you must use the Production push notification server
            //  (so you would change the first arg in the ctor of ApplePushChannelSettings to 'true')

            ApplePushChannelSettings appleSettings = new ApplePushChannelSettings(false, appleCert, "0lbSandb0x");
            push.RegisterAppleService(appleSettings); //Extension method

            AppleNotification notification = new AppleNotification();

            notification.ForDeviceToken(deviceToken);

            AppleNotificationAlert alert = new AppleNotificationAlert ();

			if (!string.IsNullOrWhiteSpace (title))
				alert.Title = title;

            			if (!string.IsNullOrWhiteSpace (body))
            				alert.Body = body;
            
            			notification.WithAlert (alert);
            
            			if (badgeCount >= 0)
            				notification.WithBadge (badgeCount);
            
            			if (!string.IsNullOrWhiteSpace (sound))
            			{
            				notification.WithSound (sound);
            			}

            push.QueueNotification(notification);

            //---------------------------
            // ANDROID GCM NOTIFICATIONS
            //---------------------------
            //Configure and start Android GCM
            //IMPORTANT: The API KEY comes from your Google APIs Console App, under the API Access section, 
            //  by choosing 'Create new Server key...'
            //  You must ensure the 'Google Cloud Messaging for Android' service is enabled in your APIs Console

            push.RegisterGcmService(new GcmPushChannelSettings("AIzaSyAQTRCFjVX5LQ0dOd4Gue4_mUuv3jlPMrg"));

            //Fluent construction of an Android GCM Notification
            //IMPORTANT: For Android you MUST use your own RegistrationId here that gets generated within your Android app itself!
			push.QueueNotification(
				new GcmNotification().ForDeviceRegistrationId("APA91bHr5W1cNl5mcZ_iWqGKVnvcXeZwYdVGCCFjt0M8egamRAIq5lCASbUQe-3E9M74CiD8Moildh4SC8Qj6qUUpCnNOQ5v17A9go1enqDipOGSaeiDU_I3fGroneA7tL3FAMlN60nW")
            				.WithJson("{\"alert\":\"Hello Leslie!\",\"badge\":7,\"sound\":\"sound.caf\"}"))
			;


            //-----------------------------
            //			// WINDOWS PHONE NOTIFICATIONS
            //			//-----------------------------
            //			//Configure and start Windows Phone Notifications
            //			push.RegisterWindowsPhoneService();
            //			//Fluent construction of a Windows Phone Toast notification
            //			//IMPORTANT: For Windows Phone you MUST use your own Endpoint Uri here that gets generated within your Windows Phone app itself!
            //			push.QueueNotification(new WindowsPhoneToastNotification()
            //				.ForEndpointUri(new Uri("DEVICE REGISTRATION CHANNEL URI HERE"))
            //				.ForOSVersion(WindowsPhoneDeviceOSVersion.MangoSevenPointFive)
            //				.WithBatchingInterval(BatchingInterval.Immediate)
            //				.WithNavigatePath("/MainPage.xaml")
            //				.WithText1("PushSharp")
            //				.WithText2("This is a Toast"));
            //
            //
            //			//-------------------------
            //			// WINDOWS NOTIFICATIONS
            //			//-------------------------
            //			//Configure and start Windows Notifications
            //			push.RegisterWindowsService(new WindowsPushChannelSettings("WINDOWS APP PACKAGE NAME HERE",
            //				"WINDOWS APP PACKAGE SECURITY IDENTIFIER HERE", "CLIENT SECRET HERE"));
            //			//Fluent construction of a Windows Toast Notification
            //			push.QueueNotification(new WindowsToastNotification()
            //				.AsToastText01("This is a test")
            //				.ForChannelUri("DEVICE CHANNEL URI HERE"));

            Console.WriteLine("Waiting for Queue to Finish...");

            //Stop and wait for the queues to drains
            push.StopAllServices();

            Console.WriteLine("Queue Finished, press return to exit...");
            Console.ReadLine();
        }

        static void DeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification)
        {
            //Currently this event will only ever happen for Android GCM
            Console.WriteLine("Device Registration Changed:  Old-> " + oldSubscriptionId + "  New-> " + newSubscriptionId + " -> " + notification);
        }

        static void NotificationSent(object sender, INotification notification)
        {
            Console.WriteLine("Sent: " + sender + " -> " + notification);
        }

        static void NotificationFailed(object sender, INotification notification, Exception notificationFailureException)
        {
            Console.WriteLine("Failure: " + sender + " -> " + notificationFailureException.Message + " -> " + notification);
        }

        static void ChannelException(object sender, IPushChannel channel, Exception exception)
        {
            Console.WriteLine("Channel Exception: " + sender + " -> " + exception);
        }

        static void ServiceException(object sender, Exception exception)
        {
            Console.WriteLine("Service Exception: " + sender + " -> " + exception);
        }

        static void DeviceSubscriptionExpired(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification)
        {
            Console.WriteLine("Device Subscription Expired: " + sender + " -> " + expiredDeviceSubscriptionId);
        }

        static void ChannelDestroyed(object sender)
        {
            Console.WriteLine("Channel Destroyed for: " + sender);
        }

        static void ChannelCreated(object sender, IPushChannel pushChannel)
        {
            Console.WriteLine("Channel Created for: " + sender);
        }
    }
}
