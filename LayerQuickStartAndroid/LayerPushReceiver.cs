using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;

namespace Com.Layer.QuickStartAndroid
{
    //[BroadcastReceiver()]
    //[IntentFilter(new[] { "com.layer.sdk.PUSH" },
    //    Categories = new[] { "com.layer.quick_start_android.xamarin" })]
    //[IntentFilter(new[] { "android.intent.action.BOOT_COMPLETED" },
    //    Categories = new[] { "com.layer.quick_start_android.xamarin" })]
    public class LayerPushReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            //Don't show a notification on boot
            if (intent.Action == Intent.ActionBootCompleted)
            {
                return;
            }

            // Get notification content
            Bundle extras = intent.Extras;
            string message = "";
            Android.Net.Uri conversationId = null;
            if (extras.ContainsKey("layer-push-message"))
            {
                message = extras.GetString("layer-push-message");
            }
            if (extras.ContainsKey("layer-conversation-id"))
            {
                conversationId = extras.GetParcelable("layer-conversation-id") as Android.Net.Uri;
            }

            // Build the notification
            NotificationCompat.Builder builder = new NotificationCompat.Builder(context)
                    .SetSmallIcon(Resource.Drawable.ic_launcher)
                    .SetContentTitle(context.Resources.GetString(Resource.String.app_name))
                    .SetContentText(message)
                    .SetAutoCancel(true)
                    .SetPriority(NotificationCompat.PriorityDefault)
                    .SetDefaults(NotificationCompat.DefaultSound | NotificationCompat.DefaultVibrate);

            // Set the action to take when a user taps the notification
            Intent resultIntent = new Intent(context, typeof(MainActivity));
            resultIntent.SetFlags(ActivityFlags.ClearTop);
            resultIntent.PutExtra("layer-conversation-id", conversationId);
            PendingIntent resultPendingIntent = PendingIntent.GetActivity(context, 0, resultIntent, PendingIntentFlags.CancelCurrent);
            builder.SetContentIntent(resultPendingIntent);

            // Show the notification
            NotificationManager notifyMgr = (NotificationManager) context.GetSystemService(Context.NotificationService);
            notifyMgr.Notify(1, builder.Build());
        }
    }
}