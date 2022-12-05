﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuni.MppOpcUaClientLib;

namespace Minipanosprosessi
{
    class Communication
    {
        private MppClient client;
        bool isConnected = false;
        private HashSet<IProcessObserver> observers = null;
        private object lockObject;

        public Communication()
        {
            lockObject = new object();
            observers = new HashSet<IProcessObserver>();
        }

        public void Connect()
        {
            if(isConnected){return;}  // TODO ? Jos tarvii uudelleenyhdistää

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

            // TODO: Limit switches
            isConnected = false;
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

        }

        private void NotifyObserversProcessItems(object source, ProcessItemChangedEventArgs args)
        {

        }

        /// <summary>
        /// Set an on off item value.
        /// </summary>
        /// <param name="item">Item name</param>
        /// <param name="value">On or off value</param>
        public void setItem(string item, bool value)
        {
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
                
            }
            else if (item.StartsWith("P"))
            {
                /*
                if(value != 100 || value != 0){
                    throw new ArgumentException("Power parameter can only be 0 or 100");

                }
                */
            }

        }
    }
}