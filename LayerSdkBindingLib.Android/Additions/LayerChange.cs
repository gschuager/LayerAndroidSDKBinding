using Com.Layer.Sdk.Messaging;

namespace Com.Layer.Sdk.Changes
{
    public partial class LayerChange
    {
        // Can't just call the Object property and cast the returned instance to the desired type, because for some weird reason on 
        // the Xamarin generated binding (which does Java.Lang.Class.GetObject<ILayerObject>), the Object could be a non-T subclass 
        // but ILayerObject instead (ie. in the case for subinterfaces like IMessage, otherwise fine for subclasses like Conversation)!
        public T GetObject<T>()
            where T : class, ILayerObject
        {
            return Java.Lang.Class.GetObject<T>(this.Object.Handle, Android.Runtime.JniHandleOwnership.DoNotTransfer);
        }
    }
}