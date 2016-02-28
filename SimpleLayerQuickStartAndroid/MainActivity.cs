using Android.App;
using Android.Widget;
using Com.Layer.Sdk.Messaging;
using Com.Layer.Sdk;
using System.Text;
using Android.OS;

namespace SimpleLayerQuickStartAndroid
{
    [Activity(Label = "SimpleLayerQuickStartAndroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate 
            {
                LayerClient layerClient = MyApp.Instance.LayerClient;

                // Creates and returns a new conversation object with sample participant identifiers
                Conversation conversation;
                if (layerClient.Conversations.Count == 0)
                {
                    conversation = layerClient.NewConversation("948374839");
                }
                else
                {
                    conversation = layerClient.Conversations[0];
                }

                // Create a message part with a string of text
                MessagePart messagePart = layerClient.NewMessagePart("text/plain", Encoding.UTF8.GetBytes("Hi, how are you?"));

                // Creates and returns a new message object with the given conversation and array of message parts
                IMessage message = layerClient.NewMessage(messagePart);

                // Sends the specified message to the conversation
                conversation.Send(message);
            };
        }
    }
}

