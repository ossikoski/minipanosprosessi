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
        MainWindow mainWindowObject;
        private MppClient client;
        bool wasConnected = false;
        private HashSet<IProcessObserver> observers = null;
        private object lockObject;

        /// <summary>
        /// Communication class to communicate between the simulator and the system.
        /// </summary>
        /// <param name="mainWindow">MainWindow object</param>
        public Communication(MainWindow mainWindow)
        {
            mainWindowObject = mainWindow;
            lockObject = new object();
            observers = new HashSet<IProcessObserver>();
        }

        /// <summary>
        /// Connect to the simulator
        /// </summary>
        public void Connect()
        {
            if (client != null)
            {
                client.Dispose();
            }

            ConnectionParamsHolder par = new ConnectionParamsHolder("opc.tcp://localhost:8087");
            client = new MppClient(par);

            client.ConnectionStatus += new MppClient.ConnectionStatusEventHandler(NotifyObserversConnectionStatus);
            client.ProcessItemsChanged += new MppClient.ProcessItemsChangedEventHandler(NotifyObserversProcessItems);

            try
            {
                client.Init();
            }
            catch (InvalidOperationException)
            {
                mainWindowObject.showMessage("Yhteyttä ei voitu muodostaa.\n" + "Varmista, että simulaattori on päällä.", "Virhe");
                return;
            }

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

            wasConnected = true;
        }

        /// <summary>
        /// Add observer who will receive info from the simulator
        /// Save to observers list attribute 
        /// </summary>
        /// <param name="observer">The observer to save</param>
        public void AddObserver(IProcessObserver observer)
        {
            lock (lockObject)
            {
                observers.Add(observer);
            }
        }


        /// <summary>
        /// Add observer who will no longer receive info from the simulator
        /// </summary>
        /// <param name="observer">The observer to remove</param>
        public void RemoveObserver(IProcessObserver observer)
        {
            lock (lockObject)
            {
                observers.Remove(observer);
            }
        }

        /// <summary>
        /// Notify observers on the current connection status
        /// </summary>
        /// <param name="source">Source of the event</param>
        /// <param name="args">Information on the connection status</param>
        private void NotifyObserversConnectionStatus(object source, ConnectionStatusEventArgs args)
        {
            System.Console.WriteLine($"Connection status changed, status is now {args.StatusInfo.FullStatusString}");
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

            if (wasConnected && !args.StatusInfo.SimplifiedStatus.Equals(ConnectionStatusInfo.StatusType.Connected))
            {
                wasConnected = false;
                Connect();
            }
        }

        /// <summary>
        /// Notify observers on the changed process items.
        /// </summary>
        /// <param name="source">Source of the event</param>
        /// <param name="args">Information on the changed process items</param>
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

        /// <summary>
        /// Set an on off item value.
        /// </summary>
        /// <param name="item">Item name</param>
        /// <param name="value">On or off value</param>
        public void setItem(string item, bool value)
        {
            client.SetOnOffItem(item, value);
        }

        /// <summary>
        /// Set item control valve or pump value.
        /// </summary>
        /// <param name="item">Item name</param>
        /// <param name="value">Value to set. If item is a pump, can only be 0 or 100.</param>
        public void setItem(string item, int value)
        {
            if(item.StartsWith("V"))
            {
                if(0 <= value && value <= 100)
                {
                    client.SetValveOpening(item, value);
                }
            }
            else if (item.StartsWith("P"))
            {
                if(value == 0 || value == 100)
                {
                    client.SetPumpControl(item, value);
                }
                else 
                {
                    throw new ArgumentException("Power parameter can only be 0 or 100");
                }
            }

        }
    }
}
