using Android.OS;
using Android.Util;
using Com.Layer.Sdk;
using Com.Layer.Sdk.Exceptions;
using Com.Layer.Sdk.Listeners;
using Org.Apache.Http;
using Org.Apache.Http.Client.Methods;
using Org.Apache.Http.Entity;
using Org.Apache.Http.Impl.Client;
using Org.Apache.Http.Util;
using Org.Json;
using System;

namespace SimpleLayerQuickStartAndroid
{
    internal class MyAuthenticationListener : Java.Lang.Object, ILayerAuthenticationListener
    {
        public void OnAuthenticated(LayerClient client, string arg1)
        {
            Log.Debug(nameof(MyAuthenticationListener), "Authentication successful");
        }

        /*
         * 1. Implement `onAuthenticationChallenge` in your Authentication Listener to acquire a nonce
         */
        public void OnAuthenticationChallenge(LayerClient client, string nonce)
        {
            //You can use any identifier you wish to track users, as long as the value is unique
            //This identifier will be used to add a user to a conversation in order to send them messages
            string userId = GetUserId();

            /*
            * 2. Acquire an identity token from the Layer Identity Service
            */
            new AcquireIdentityTokenAsyncTask(client, userId, nonce).Execute();
        }

        public void OnAuthenticationError(LayerClient client, LayerException e)
        {
            Log.Debug(nameof(MyAuthenticationListener), "There was an error authenticating");
        }

        public void OnDeauthenticated(LayerClient client)
        {
        }

        private string GetUserId()
        {
            if (Build.Fingerprint.StartsWith("generic"))
            {
                return "Simulator";
            }
            return "1";
        }

        private class AcquireIdentityTokenAsyncTask : AsyncTask<Java.Lang.Void, Java.Lang.Void, Java.Lang.Void>
        {
            private readonly LayerClient _layerClient;
            private readonly string _userId;
            private readonly string _nonce;

            public AcquireIdentityTokenAsyncTask(LayerClient layerClient, string userId, string nonce)
            {
                _layerClient = layerClient;
                _userId = userId;
                _nonce = nonce;
            }

            protected override Java.Lang.Void RunInBackground(params Java.Lang.Void[] @params)
            {
                try
                {
                    HttpPost post = new HttpPost("https://layer-identity-provider.herokuapp.com/identity_tokens");
                    post.SetHeader("Content-Type", "application/json");
                    post.SetHeader("Accept", "application/json");

                    JSONObject json = new JSONObject()
                            .Put("app_id", _layerClient.AppId)
                            .Put("user_id", _userId)
                            .Put("nonce", _nonce);
                    post.Entity = new StringEntity(json.ToString());

                    IHttpResponse response = (new DefaultHttpClient()).Execute(post);
                    string eit = (new JSONObject(EntityUtils.ToString(response.Entity)))
                            .OptString("identity_token");

                    /*
                     * 3. Submit identity token to Layer for validation
                     */
                    _layerClient.AnswerAuthenticationChallenge(eit);
                }
                catch (Exception ex)
                {
                    Log.Debug(nameof(MyAuthenticationListener), ex.StackTrace);
                }
                return null;
            }
        }
    }
}