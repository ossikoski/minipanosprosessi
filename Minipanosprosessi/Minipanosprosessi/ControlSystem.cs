using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuni.MppOpcUaClientLib;

namespace Minipanosprosessi
{
    /// <summary>
    /// Minipanosprosessin ohjaussysteemin logiikka
    /// </summary>
    /// 
    enum Stage
    {
        /// <summary>
        /// Mikä prosessin vaihe on meneillään
        /// </summary>
        impregnation_1,
        impregnation_2,
        impregnation_3,
        impregnation_4,
        impregnation_5,
        black_liquor_fill_1,
        black_liquor_fill_2,
        white_liquor_fill_1,
        white_liquor_fill_2,
        cooking_1,
        cooking_2,
        cooking_3,
        cooking_4,
        cooking_5,
        cooking_6,
        cooking_7,
        discharge_1,
        discharge_2
    }
    class ControlSystem : IProcessObserver
    {
        Communication communicationObject;
        bool isStarted;
        Stage stage;
        public ControlSystem(Communication communication)
        {
            communicationObject = communication;
            isStarted = false;
            stage = Stage.impregnation_1;
        }

        public void Start()
        {
            isStarted = true;
            RunLoop();
        }

        public void Stop()
        {
            isStarted = false;
        }

        public void UpdateConnectionStatus(ConnectionStatusEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void UpdateProcessItems(ProcessItemChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void RunLoop()
        {
            Impregnation1();
            /*
            while (isStarted){
                switch (stage)
                {
                    case Stage.impregnation_1:
                        Impregnation1();
                    case Stage.impregnation_2:
                        Impregnation2();
                }
            }
            */
        }

        private void Impregnation1()
        {
            communicationObject.setItem("V201", true);
            communicationObject.setItem("V303", true);
            communicationObject.setItem("P200", 100);
            communicationObject.setItem("V204", true);
            communicationObject.setItem("V301", true);
        }
    }
}
