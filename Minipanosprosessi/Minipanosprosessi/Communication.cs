using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuni.MppOpcUaClientLib;

namespace Minipanosprosessi
{
    class Communication
    {
        private MppClient client = null;
        private HashSet<IProcessObserver> observers = null;
        private object lockObject;

        /// <summary>
        /// 
        /// </summary>
        public Communication()
        {
            lockObject = new object();
            observers = new HashSet<IProcessObserver>();
        }

        public void Connect()
        {
            if(client != null)
            {
                client.Dispose();
            }

            ConnectionParamsHolder par = new ConnectionParamsHolder("opc.tcp://localhost:8087");
            client = new MppClient(par);

            client.ConnectionStatus += new MppClient.ConnectionStatusEventHandler(NotifyObserversConnectionStatus);
            client.ProcessItemsChanged += new MppClient.ProcessItemsChangedEventHandler(NotifyObserversProcessItems);

            client.Init();

            // Temperature
            client.AddToSubscription("TI100");
            client.AddToSubscription("TI300");
            // Flow
            client.AddToSubscription("FI100");
            // Level
            client.AddToSubscription("LI100");
            client.AddToSubscription("LI200");
            client.AddToSubscription("LI400");
            // Pressure
            client.AddToSubscription("PI300");
            // Control valve
            client.AddToSubscription("V102");
            client.AddToSubscription("V104");
            // On/off valve
            client.AddToSubscription("V103");
            client.AddToSubscription("V201");
            client.AddToSubscription("V204");
            client.AddToSubscription("V301");
            client.AddToSubscription("V302");
            client.AddToSubscription("V303");
            client.AddToSubscription("V304");
            client.AddToSubscription("V401");
            client.AddToSubscription("V404");
            // Pump
            client.AddToSubscription("P100");
            client.AddToSubscription("P200");
            // Heater
            client.AddToSubscription("E100");
            // Limit switches
            client.AddToSubscription("LS+300");
            client.AddToSubscription("LS-300");
            client.AddToSubscription("LS-200");
            client.AddToSubscription("LA+100");
        }

        public void AddObserver(IProcessObserver observer)
        {
            lock (lockObject)
            {
                observers.Add(observer);
            }

        }

        public void RemoveObserver(IProcessObserver observer)
        {
            lock (lockObject)
            {
                observers.Remove(observer);
            }
        }

        private void NotifyObserversConnectionStatus(object source, ConnectionStatusEventArgs args)
        {
            ConnectionStatusEventArgs connectionStatusCopy = null;
            IProcessObserver[] observersCopy = null;

            lock (lockObject)
            {
                connectionStatusCopy = args;
                observersCopy = new IProcessObserver[observers.Count];
                observers.CopyTo(observersCopy);
            }

            foreach(IProcessObserver o in observersCopy)
            {
                o.UpdateConnectionStatus(connectionStatusCopy);
            }

        }

        private void NotifyObserversProcessItems(object source, ProcessItemChangedEventArgs args)
        {
            ProcessItemChangedEventArgs processItemsCopy = null;
            IProcessObserver[] observersCopy = null;

            lock (lockObject)
            {
                processItemsCopy = args;
                observersCopy = new IProcessObserver[observers.Count];
                observers.CopyTo(observersCopy);
            }

            foreach (IProcessObserver o in observersCopy)
            {
                o.UpdateProcessItems(processItemsCopy);
            }
        }
    }
}
