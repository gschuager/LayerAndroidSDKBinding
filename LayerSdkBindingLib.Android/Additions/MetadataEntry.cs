// Fix for generics erasure on inherited accessor methods, ie. adding back the inherited type-erased methods

namespace Com.Layer.Sdk.Messaging
{
    public partial class Metadata
    {
        public partial class Entry
        {
            public Java.Lang.Object Key { get { return GetKey(); } }

            public Java.Lang.Object Value { get { return GetValue(); } }
        }
    }
}