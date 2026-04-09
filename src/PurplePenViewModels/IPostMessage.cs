using System;
using System.Collections.Generic;
using System.Text;

namespace PurplePen.ViewModels
{
    // Interface to encapsulate posting an action to the Dispatcher.
    public interface IPostMessage
    {
        void PostMessage(Action action);
    }
}
