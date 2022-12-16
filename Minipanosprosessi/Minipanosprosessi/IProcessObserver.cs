using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuni.MppOpcUaClientLib;

namespace Minipanosprosessi
{
    /// <summary>
    /// Interface to update connection status and process items from the simulator
    /// </summary>
    interface IProcessObserver
    {
        // Update connection status
        void UpdateConnectionStatus(ConnectionStatusEventArgs args);

        // Update process items
        void UpdateProcessItems(ProcessItemChangedEventArgs args);
    }
}
