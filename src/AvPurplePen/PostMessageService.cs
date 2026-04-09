using Avalonia.Threading;
using PurplePen.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvPurplePen
{
    public class PostMessageService : IPostMessage
    {
        public void PostMessage(Action action)
        {
            Dispatcher.UIThread.Post(action);
        }
    }
}
