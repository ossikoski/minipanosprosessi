using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuni.MppOpcUaClientLib;

namespace Minipanosprosessi
{
    interface IProcessObserver
    {
        // Update connection status
        void UpdateConnectionStatus(ConnectionStatusEventArgs args);

        // Update process items
        void UpdateProcessItems(ProcessItemChangedEventArgs args);
    }
}
