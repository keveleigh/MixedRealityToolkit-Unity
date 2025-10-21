using System;

namespace Microsoft.MixedReality.Toolkit.DataBinding
{
    public interface IBinder
    {
        void Subscribe();
        void Unsubscribe();
    }
}