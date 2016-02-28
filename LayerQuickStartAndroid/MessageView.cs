using Android.Graphics;
using Android.Views;
using Android.Widget;
using Com.Layer.Sdk.Messaging;
using System.Collections.Generic;
using System.Text;

namespace Com.Layer.QuickStartAndroid
{
    /**
     * Takes a Layer Message object, formats the text and attaches it to a LinearLayout
     */
    public class MessageView
    {
        //The parent object (in this case, a LinearLayout object with a ScrollView parent)
        private LinearLayout myParent;

        //The sender and message views
        private TextView senderTV;
        private TextView messageTV;

        private ImageView statusImage;

        private LinearLayout messageDetails;

        //Takes the Layout parent object and message
        public MessageView(LinearLayout parent, IMessage msg)
        {
            myParent = parent;

            //The first part of each message will include the sender and status
            messageDetails = new LinearLayout(parent.Context);
            messageDetails.Orientation = Orientation.Horizontal;
            myParent.AddView(messageDetails);

            //Creates the sender text view, sets the text to be italic, and attaches it to the message details view
            senderTV = new TextView(parent.Context);
            senderTV.SetTypeface(null, TypefaceStyle.Italic);
            messageDetails.AddView(senderTV);

            //Creates the message text view and attaches it to the parent
            messageTV = new TextView(parent.Context);
            myParent.AddView(messageTV);

            //The status is displayed with an icon, depending on whether the message has been read,
            // delivered, or sent
            //statusImage = new ImageView(parent.getContext());
            statusImage = _CreateStatusImage(msg);//statusImage.setImageResource(R.drawable.sent);
            messageDetails.AddView(statusImage);

            //Populates the text views
            UpdateMessage(msg);
        }

        //Takes a message and sets the text in the two text views
        public void UpdateMessage(IMessage msg)
        {
            string senderTxt = _CraftSenderText(msg);
            string msgTxt = _CraftMsgText(msg);

            senderTV.Text = senderTxt;
            messageTV.Text = msgTxt;
        }

        //The sender text is formatted like so:
        //  User @ Timestamp - Status
        private string _CraftSenderText(IMessage msg)
        {
            if (msg == null)
            {
                return "";
            }

            //The User ID
            string senderTxt = "";
            if (msg.Sender != null && msg.Sender.UserId != null)
            {
                senderTxt = msg.Sender.UserId;
            }

            //Add the timestamp
            if (msg.SentAt != null)
            {
                senderTxt += " @ " + new Java.Text.SimpleDateFormat("HH:mm:ss").Format(msg.SentAt);
            }

            //Add some formatting before the status icon
            senderTxt += "   ";

            //Return the formatted text
            return senderTxt;
        }

        //Checks the recipient status of the message (based on all participants, ie. READ meaning at least one has read)
        private MessageRecipientStatus _GetMessageStatus(IMessage msg)
        {
            if (msg == null || msg.Sender == null || msg.Sender.UserId == null)
            {
                return MessageRecipientStatus.Pending;
            }

            //If we didn't send the message, we already know the status - we have read it
            if (!msg.Sender.UserId.Equals(MainActivity.GetUserID(), System.StringComparison.InvariantCultureIgnoreCase))
            {
                return MessageRecipientStatus.Read;
            }

            //Assume the message has been sent
            MessageRecipientStatus status = MessageRecipientStatus.Sent;

            //Go through each user to check the status, in this case we check each user and
            // prioritize so
            // that we return the highest status: Sent -> Delivered -> Read
            var allParticipants = MainActivity.GetAllParticipants();
            for (int i = 0; i < allParticipants.Count; i++)
            {
                //Don't check the status of the current user
                string participant = allParticipants[i];
                if (participant.Equals(MainActivity.GetUserID(), System.StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                if (status == MessageRecipientStatus.Sent)
                {
                    if (msg.GetRecipientStatus(participant) == MessageRecipientStatus.Delivered)
                    {
                        status = MessageRecipientStatus.Delivered;
                    }

                    if (msg.GetRecipientStatus(participant) == MessageRecipientStatus.Read)
                    {
                        return MessageRecipientStatus.Read;
                    }
                }
                else if (status == MessageRecipientStatus.Delivered)
                {
                    if (msg.GetRecipientStatus(participant) == MessageRecipientStatus.Read)
                    {
                        return MessageRecipientStatus.Read;
                    }
                }
            }

            return status;
        }

        //Checks the message parts and parses the message contents
        private string _CraftMsgText(IMessage msg)
        {
            //The message text
            string msgText = "";

            //Go through each part, and if it is text (which it should be by default), append it to the
            // message text
            IList<MessagePart> parts = msg.MessageParts;
            for (int i = 0; i < parts.Count; i++)
            {
                //You can always set the mime type when creating a message part, by default the mime
                // type is initialized to plain text when the message part is created
                if (parts[i].MimeType.Equals("text/plain", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        msgText += Encoding.UTF8.GetString(parts[i].GetData()) + "\n";
                    }
                    catch (DecoderFallbackException e)
                    {
                    }
                }
            }

            //Return the assembled text
            return msgText;
        }

        //Sets the status image based on whether other users in the conversation have received or read
        //the message
        private ImageView _CreateStatusImage(IMessage msg)
        {
            ImageView status = new ImageView(myParent.Context);

            var messageRecipientStatus = _GetMessageStatus(msg);
            if (MessageRecipientStatus.Sent == messageRecipientStatus)
            {
                status.SetImageResource(Resource.Drawable.sent);
            }
            else if (MessageRecipientStatus.Delivered == messageRecipientStatus)
            {
                status.SetImageResource(Resource.Drawable.delivered);
            }
            else if (MessageRecipientStatus.Read == messageRecipientStatus)
            {
                status.SetImageResource(Resource.Drawable.read);
            }

            //Have the icon fill the space vertically
            status.LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.MatchParent);

            return status;
        }
    }
}