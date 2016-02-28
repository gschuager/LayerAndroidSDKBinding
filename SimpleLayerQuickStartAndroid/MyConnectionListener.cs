using Com.Layer.Sdk;
using Com.Layer.Sdk.Exceptions;
using Com.Layer.Sdk.Listeners;

namespace SimpleLayerQuickStartAndroid
{
    internal class MyConnectionListener : Java.Lang.Object, ILayerConnectionListener
    {
        public void OnConnectionConnected(LayerClient client)
        {
            // Ask the LayerClient to authenticate. If no auth credentials are present,
            // an authentication challenge is issued
            client.Authenticate();
        }

        public void OnConnectionDisconnected(LayerClient arg0)
        {
        }

        public void OnConnectionError(LayerClient arg0, LayerException e)
        {
        }
    }
}