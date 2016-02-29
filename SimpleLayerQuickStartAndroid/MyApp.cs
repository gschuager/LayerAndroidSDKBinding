using Android.App;
using Android.Runtime;
using Com.Layer.Sdk;
using System;

namespace SimpleLayerQuickStartAndroid
{
    [Application]
    public class MyApp : Application
    {
        private const string LAYER_APP_ID = "";
        private const string GCM_PROJECT_NUMBER = "";

        public static MyApp Instance { get; private set; }

        public MyApp(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Instance = this;
        }

        private LayerClient _LayerClient;
        public LayerClient LayerClient
        {
            get
            {
                if (_LayerClient == null)
                {
                    // Used for debugging purposes ONLY. DO NOT include this option in Production Builds.
                    LayerClient.SetLoggingEnabled(this, true);

                    // Initializes a LayerClient object with the Google Project Number
                    LayerClient.Options options = new LayerClient.Options();

                    // Sets the GCM sender id allowing for push notifications
                    options.InvokeGoogleCloudMessagingSenderId(GCM_PROJECT_NUMBER);

                    // By default, only unread messages are synced after a user is authenticated, but you
                    // can change that behavior to all messages or just the last message in a conversation
                    options.InvokeHistoricSyncPolicy(LayerClient.Options.HistoricSyncPolicy.AllMessages);

                    // Create a LayerClient object
                    _LayerClient = LayerClient.NewInstance(this, LAYER_APP_ID, options);

                    MyConnectionListener connectionListener = new MyConnectionListener();
                    MyAuthenticationListener authenticationListener = new MyAuthenticationListener();

                    // Note: It is possible to register more than one listener for an activity. If you 
                    // execute this code more than once in your app, pass in the same listener to avoid 
                    // memory leaks and multiple callbacks.
                    _LayerClient.RegisterConnectionListener(connectionListener);
                    _LayerClient.RegisterAuthenticationListener(authenticationListener);
                }

                //Check the current state of the SDK. The client must be CONNECTED and the user must
                // be AUTHENTICATED in order to send and receive messages. Note: it is possible to be
                // authenticated, but not connected, and vice versa, so it is a best practice to check
                // both states when your app launches or comes to the foreground.
                if (!_LayerClient.IsConnected)
                {
                    // Asks the LayerSDK to establish a network connection with the Layer service
                    // If Layer is not connected, make sure we connect in order to send/receive messages.
                    // MyConnectionListener handles the callbacks associated with Connection, and
                    // will start the Authentication process once the connection is established.
                    _LayerClient.Connect();
                }
                else if (!_LayerClient.IsAuthenticated)
                {

                    // If the client is already connected, try to authenticate a user on this device.
                    // MyAuthenticationListener.java handles the callbacks associated with Authentication
                    // and will start the Conversation View once the user is authenticated
                    _LayerClient.Authenticate();

                }
                else
                {
                    // If the client is to Layer and the user is authenticated, allow for Conversation.
                    //onUserAuthenticated();
                }
                return _LayerClient;
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();

            LayerClient.ApplicationCreated(this);

            LayerClient layerClient = this.LayerClient;
        }
    }
}
