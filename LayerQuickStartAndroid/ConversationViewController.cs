using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Layer.Sdk;
using Com.Layer.Sdk.Changes;
using Com.Layer.Sdk.Exceptions;
using Com.Layer.Sdk.Listeners;
using Com.Layer.Sdk.Messaging;
using Com.Layer.Sdk.QueryNS;
using Java.Lang;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Com.Layer.QuickStartAndroid
{
    /**
     * Handles the conversation between the pre-defined participants (Device, Emulator) and displays
     * messages in the GUI.
     */
    public class ConversationViewController : Java.Lang.Object, View.IOnClickListener, ILayerChangeEventListener, 
        ITextWatcher, ILayerTypingIndicatorListener, ILayerSyncListener
    {
        private static readonly string TAG = typeof(ConversationViewController).Name;

        private LayerClient layerClient;

        //GUI elements
        private Button sendButton;
        private LinearLayout topBar;
        private EditText userInput;
        private ScrollView conversationScroll;
        private LinearLayout conversationView;
        private TextView typingIndicator;

        //List of all users currently typing
        private List<string> typingUsers;

        //Current conversation
        private Conversation activeConversation;

        //All messages
        private ConcurrentDictionary<string, MessageView> allMessages;

        public ConversationViewController(MainActivity ma, LayerClient client)
        {
            //Cache off LayerClient
            layerClient = client;

            //When conversations/messages change, capture them
            layerClient.RegisterEventListener(this);

            //List of users that are typing which is used with ILayerTypingIndicatorListener
            typingUsers = new List<string>();

            //Change the layout
            ma.SetContentView(Resource.Layout.activity_main);

            //Cache off gui objects
            sendButton = ma.FindViewById<Button>(Resource.Id.send);
            topBar = ma.FindViewById<LinearLayout>(Resource.Id.topbar);
            userInput = ma.FindViewById<EditText>(Resource.Id.input);
            conversationScroll = ma.FindViewById<ScrollView>(Resource.Id.scrollView);
            conversationView = ma.FindViewById<LinearLayout>(Resource.Id.conversation);
            typingIndicator = ma.FindViewById<TextView>(Resource.Id.typingIndicator);

            //Capture user input
            sendButton.SetOnClickListener(this);
            topBar.SetOnClickListener(this);
            userInput.Text = _GetInitialMessage();
            userInput.AddTextChangedListener(this);

            //If there is an active conversation between the Device, Simulator, and Dashboard (web
            // client), cache it
            activeConversation = _GetConversation();

            //If there is an active conversation, draw it
            _DrawConversation();

            if (activeConversation != null)
            {
                _GetTopBarMetaData();
            }
        }

        public static string _GetInitialMessage()
        {
            return "Hey, everyone! This is your friend, " + MainActivity.GetUserID();
        }

        //Create a new message and send it
        private void _SendButtonClicked()
        {        
            //Check to see if there is an active conversation between the pre-defined participants
            if (activeConversation == null)
            {
                activeConversation = _GetConversation();

                //If there isn't, create a new conversation with those participants
                if (activeConversation == null)
                {
                    activeConversation = layerClient.NewConversation(MainActivity.GetAllParticipants());
                }
            }

            _SendMessage(userInput.Text);

            //Clears the text input field
            userInput.Text = "";
        }

        private void _SendMessage(string text)
        {
            //Put the user's text into a message part, which has a MIME type of "text/plain" by default
            MessagePart messagePart = layerClient.NewMessagePart(text);

            //Formats the push notification that the other participants will receive
            MessageOptions options = new MessageOptions();
            options.InvokePushNotificationMessage(MainActivity.GetUserID() + ": " + text);

            //Creates and returns a new message object with the given conversation and array of
            // message parts
            IMessage message = layerClient.NewMessage(options, messagePart);

            //Sends the message
            if (activeConversation != null)
            {
                activeConversation.Send(message);
            }
        }

        //Create a random color and apply it to the Layer logo bar
        private void _TopBarClicked()
        {
            Random r = new Random();
            float red = (float)r.NextDouble();
            float green = (float)r.NextDouble();
            float blue = (float)r.NextDouble();

            _SetTopBarMetaData(red, green, blue);
            _SetTopBarColor(red, green, blue);
        }

        //Checks to see if there is already a conversation between the device and emulator
        private Conversation _GetConversation()
        {
            if (activeConversation == null)
            {
                Query query = Query.InvokeBuilder(Java.Lang.Class.FromType(typeof(Conversation)))
                        .Predicate(new Predicate(Conversation.Property.Participants, 
                            Predicate.Operator.EqualTo, new Java.Util.ArrayList(MainActivity.GetAllParticipants())))
                        .SortDescriptor(new SortDescriptor(Conversation.Property.CreatedAt,
                            SortDescriptor.Order.Descending))
                        .Build();

                IList results = layerClient.ExecuteQuery(query, Query.ResultType.Objects);
                if (results != null && results.Count > 0) 
                {
                    return results[0] as Conversation;
                }
            }

            //Returns the active conversation (which is null by default)
            return activeConversation;
        }

        //Redraws the conversation window in the GUI
        private void _DrawConversation()
        {
            //Only proceed if there is a valid conversation
            if (activeConversation != null)
            {
                //Clear the GUI first and empty the list of stored messages
                conversationView.RemoveAllViews();
                allMessages = new ConcurrentDictionary<string, MessageView>();

                //Grab all the messages from the conversation and add them to the GUI
                IList<IMessage> allMsgs = layerClient.GetMessages(activeConversation);
                for (int i = 0; i < allMsgs.Count; i++)
                {
                    AddMessageToView(allMsgs[i]);
                }

                //After redrawing, force the scroll view to the bottom (most recent message)
                conversationScroll.Post(() =>
                {
                    conversationScroll.FullScroll(FocusSearchDirection.Down);
                });
            }
        }

        //Creates a GUI element (header and body) for each Message
        private void AddMessageToView(IMessage msg)
        {
            //Make sure the message is valid
            if (msg == null || msg.Sender == null || msg.Sender.UserId == null)
            {
                return;
            }

            //Once the message has been displayed, we mark it as read
            //NOTE: the sender of a message CANNOT mark their own message as read
            if (!msg.Sender.UserId.Equals(layerClient.AuthenticatedUserId, StringComparison.InvariantCultureIgnoreCase))
            {
                msg.MarkAsRead();
            }

            //Grab the message id
            string msgId = msg.Id.ToString();

            //If we have already added this message to the GUI, skip it
            if (!allMessages.ContainsKey(msgId))
            {
                //Build the GUI element and save it
                MessageView msgView = new MessageView(conversationView, msg);
                allMessages[msgId] = msgView;
            }
        }

        //Stores RGB values in the conversation's metadata
        private void _SetTopBarMetaData(float red, float green, float blue)
        {
            if (activeConversation != null)
            {
                Metadata metadata = Metadata.NewInstance();

                Metadata colors = Metadata.NewInstance();
                colors.Put("red", red.ToString());
                colors.Put("green", green.ToString());
                colors.Put("blue", blue.ToString());

                metadata.Put("backgroundColor", colors);

                //Merge this new information with the existing metadata (passing in false will replace
                // the existing Map, passing in true ensures existing key/values are preserved)
                activeConversation.PutMetadata(metadata, true);
            }
        }

        //Check the conversation's metadata for RGB values
        private void _GetTopBarMetaData()
        {
            if (activeConversation != null)
            {
                Metadata current = activeConversation.Metadata;
                if (current.ContainsKey("backgroundColor"))
                {
                    Metadata colors = (Metadata) current.Get("backgroundColor");

                    if (colors != null)
                    {
                        float red = Single.Parse(colors.Get("red").ToString());
                        float green = Single.Parse(colors.Get("green").ToString());
                        float blue = Single.Parse(colors.Get("blue").ToString());

                        _SetTopBarColor(red, green, blue);
                    }
                }
            }
        }

        //Takes RGB values and sets the top bar color
        private void _SetTopBarColor(float red, float green, float blue)
        {
            if (topBar != null)
            {
                topBar.SetBackgroundColor(Color.Argb(255, 
                    (int) (255.0f * red), 
                    (int) (255.0f * green), 
                    (int) (255.0f * blue)));
            }
        }


        //================================================================================
        // View.IOnClickListener methods
        //================================================================================

        public void OnClick(View v)
        {
            //When the "send" button is clicked, grab the ongoing conversation (or create it) and
            // send the message
            if (v == sendButton)
            {
                _SendButtonClicked();
            }

            //When the Layer logo bar is clicked, randomly change the color and store it in the
            // conversation's metadata
            if (v == topBar)
            {
                _TopBarClicked();
            }
        }

        //================================================================================
        // ILayerChangeEventListener methods
        //================================================================================

        public void OnChangeEvent(LayerChangeEvent @event) 
        {
            //You can choose to handle changes to conversations or messages however you'd like:
            IList<LayerChange> changes = @event.Changes;
            for (int i = 0; i < changes.Count; i++)
            {
                LayerChange change = changes[i];
                if (change.ObjectType == LayerObjectType.Conversation)
                {
                    Conversation conversation = (Conversation) change.Object;
                    Log.Verbose(TAG, "Conversation " + conversation.Id + " attribute " +
                            change.AttributeName + " was changed from " + change.OldValue +
                            " to " + change.NewValue);

                    if (LayerChange.Type.Insert == change.ChangeType)
                    {
                    }
                    else if (LayerChange.Type.Update == change.ChangeType)
                    {
                    }
                    else if (LayerChange.Type.Delete == change.ChangeType)
                    {
                    }
                }
                else if (change.ObjectType == LayerObjectType.Message)
                {
                    IMessage message = change.Object as IMessage;
                    if (message != null) // ie. because for some weird reason on the Xamarin binding, the Object could be a non IMessage subclass!
                    {
                        Log.Verbose(TAG, "Message " + message.Id + " attribute " + change
                                .AttributeName + " was changed from " + change.OldValue +
                                " to " + change.NewValue);

                        if (LayerChange.Type.Insert == change.ChangeType)
                        {
                        }
                        else if (LayerChange.Type.Update == change.ChangeType)
                        {
                        }
                        else if (LayerChange.Type.Delete == change.ChangeType)
                        {
                        }
                    }
                }
            }

            //If we don't have an active conversation, grab the oldest one
            if (activeConversation == null)
            {
                activeConversation = _GetConversation();
            }

            //If anything in the conversation changes, re-draw it in the GUI
            _DrawConversation();

            //Check the meta-data for color changes
            _GetTopBarMetaData();
        }

        //================================================================================
        // ITextWatcher methods
        //================================================================================

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
        }

        public void AfterTextChanged(IEditable s)
        {
            //After the user has changed some text, we notify other participants that they are typing
            if (activeConversation != null)
            {
                activeConversation.Send(LayerTypingIndicatorListenerTypingIndicator.Started);
            }
        }

        //================================================================================
        // ILayerTypingIndicatorListener methods
        //================================================================================

        public void OnTypingIndicator(LayerClient layerClient, Conversation conversation, 
            string userID, LayerTypingIndicatorListenerTypingIndicator indicator)
        {
            //Only show the typing indicator for the active (displayed) converation
            if (conversation != activeConversation)
            {
                return;
            }

            if (LayerTypingIndicatorListenerTypingIndicator.Started == indicator)
            {
                // This user started typing, so add them to the typing list if they are not
                // already on it.
                if (!typingUsers.Contains(userID))
                {
                    typingUsers.Add(userID);
                }
            }
            else if (LayerTypingIndicatorListenerTypingIndicator.Finished == indicator)
            {
                // This user isn't typing anymore, so remove them from the list.
                typingUsers.Remove(userID);
            }

            //Format the text to display in the conversation view
            if (typingUsers.Count == 0)
            {
                //No one is typing, so clear the text
                typingIndicator.Text = "";

            }
            else if (typingUsers.Count == 1)
            {
                //Name the one user that is typing (and make sure the text is grammatically correct)
                typingIndicator.Text = typingUsers[0] + " is typing";

            }
            else if (typingUsers.Count > 1)
            {
                //Name all the users that are typing (and make sure the text is grammatically correct)
                string users = "";
                for (int i = 0; i < typingUsers.Count; i++)
                {
                    users += typingUsers[i];
                    if (i < typingUsers.Count - 1)
                    {
                        users += ", ";
                    }
                }

                typingIndicator.Text = users + " are typing";
            }
        }

        //================================================================================
        // ILayerSyncListener methods
        //================================================================================

        //Called before syncing with the Layer servers
        public void OnBeforeSync(LayerClient layerClient, LayerSyncListenerSyncType syncType)
        {
            Log.Verbose(TAG, syncType + " sync starting");
        }

        //Called during a sync, you can drive a spinner or progress bar using pctComplete, which is a
        // range between 0 and 100
        public void OnSyncProgress(LayerClient layerClient, LayerSyncListenerSyncType syncType, int pctComplete)
        {
            Log.Verbose(TAG, syncType + " sync is "  + pctComplete + "% Complete");
        }

        //Called after syncing with the Layer servers
        public void OnAfterSync(LayerClient layerClient, LayerSyncListenerSyncType syncType)
        {
            Log.Verbose(TAG, syncType + " sync complete");
        }

        //Captures any errors with syncing
        public void OnSyncError(LayerClient layerClient, IList<LayerException> layerExceptions)
        {
            foreach (LayerException e in layerExceptions)
            {
                Log.Verbose(TAG, "onSyncError: " + e.ToString());
            }
        }
    }
}