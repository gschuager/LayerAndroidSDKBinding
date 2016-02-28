using Android.Util;
using Com.Layer.Sdk;
using Com.Layer.Sdk.Exceptions;
using Com.Layer.Sdk.Listeners;

namespace Com.Layer.QuickStartAndroid
{
    public class MyConnectionListener : Java.Lang.Object, ILayerConnectionListener
    {
        private static readonly string TAG = typeof(MyConnectionListener).Name;

        private MainActivity main_activity;

        public MyConnectionListener(MainActivity ma)
        {
            //Cache off the main activity in order to perform callbacks
            main_activity = ma;
        }

        //Called on connection success. The Quick Start App immediately tries to
        //authenticate a user (or, if a user is already authenticated, return to the conversation
        //screen).
        public void OnConnectionConnected(LayerClient client)
        {
            Log.Verbose(TAG, "Connected to Layer");

            //If the user is already authenticated (and this connection was being established after
            // the app was disconnected from the network), then start the conversation view.
            //Otherwise, start the authentication process, which effectively "logs in" a user
            if (client.IsAuthenticated)
            {
                main_activity.OnUserAuthenticated();
            }
            else
            {
                client.Authenticate();
            }
        }

        //Called when the connection is closed
        public void OnConnectionDisconnected(LayerClient client)
        {
            Log.Verbose(TAG, "Connection to Layer closed");
        }

        //Called when there is an error establishing a connection. There is no need to re-establish
        // the connection again by calling layerClient.connect() - the SDK will handle re-connection
        // automatically. However, this callback can be used with conjunction with onConnectionConnected
        // to provide feedback to the user that messages cannot be sent/received (assuming there is an
        // authenticated user).
        public void OnConnectionError(LayerClient client, LayerException e)
        {
            Log.Verbose(TAG, "Error connecting to layer: " + e.ToString());
        }
    }
}