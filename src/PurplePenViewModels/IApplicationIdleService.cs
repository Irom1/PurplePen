using System;
using System.Collections.Generic;
using System.Text;

namespace PurplePen.ViewModels
{
    public interface IApplicationIdleService
    {
        public void QueueIdleEvent();
    }
}
