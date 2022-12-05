using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuni.MppOpcUaClientLib;

namespace Minipanosprosessi
{
    class ControlSystem : IProcessObserver
    {
        public ControlSystem()
        {

        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public void UpdateConnectionStatus(ConnectionStatusEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void UpdateProcessItems(ProcessItemChangedEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
