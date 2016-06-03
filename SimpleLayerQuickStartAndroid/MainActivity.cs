using System.Linq;
using Android.App;
using Android.Widget;
using Com.Layer.Sdk.Messaging;
using Com.Layer.Sdk;
using System.Text;
using Android.OS;
using Com.Layer.Sdk.Query;

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
            EditText text = FindViewById<EditText>(Resource.Id.text);

            button.Click += delegate 
            {
                LayerClient layerClient = MyApp.Instance.LayerClient;
//
//                var q = LayerQuery.InvokeBuilder(Java.Lang.Class.FromType(typeof (Conversation)))
//                    .Predicate(new Predicate(Conversation.Property.Participants, Predicate.Operator.In, "0"))
//                    .Build();
//                var conversations =
//                    layerClient.ExecuteQuery(q, LayerQuery.ResultType.Objects).Cast<Conversation>().ToArray();
//
//                if (!conversations.Any())
//                {
//                    conversations = new[] {layerClient.NewConversation("0")};
//                }


                Conversation conversation;
                if (layerClient.Conversations.Count == 0)
                {
                    conversation = layerClient.NewConversation("0");
                }
                else
                {
                    conversation = layerClient.Conversations[0];
                }
//
//                foreach (var conversation in conversations)
//                {

                    // Create a message part with a string of text
                    var messagePart = layerClient.NewMessagePart("text/plain",
                        Encoding.UTF8.GetBytes(text.Text));

                    // Creates and returns a new message object with the given conversation and array of message parts
                    var message = layerClient.NewMessage(messagePart);

                    // Sends the specified message to the conversation
                    conversation.Send(message);

//                }

                text.Text = "";
            };
        }
    }
}

