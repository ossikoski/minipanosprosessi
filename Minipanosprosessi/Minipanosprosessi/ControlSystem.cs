using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        impregnation,
        black_liquor_fill,
        white_liquor_fill,
        cooking,
        discharge
    }
    public struct Indicators
    {
        /// <summary>
        /// 
        /// </summary>
        // alkuarvot (20, 20, 0, 216, 90, 90, 0, false, true, true, true)
        public double TI100;
        public double TI300;
        public double FI100;
        public int LI100;
        public int LI200;
        public int LI400;
        public int PI300;
        public bool LSp300;  // LSp300 = LS+300
        public bool LSm300;  // LSm300 = LS-300
        public bool LSm200;
        public bool LA100;  // false -> alarm is active and level is under 100)
    }
    public struct Settings
    {
        /// <summary>
        /// Asetukset 
        /// </summary>
        public double cookingTime;
        public double cookingPressure;
        public double cookingTemperature;
        public double impregnationTime;

    }

    class ControlSystem : IProcessObserver
    {
        Communication communicationObject;
        MainWindow mainWindowObject;
        bool isStarted;
        Stage stage;
        Settings settings;
        Indicators indicators;
        private object lockObject = new object();
        public ControlSystem(Communication communication, MainWindow mainWindow)
        {
            communicationObject = communication;
            mainWindowObject = mainWindow;
            isStarted = false;
            stage = Stage.impregnation;
            indicators = new Indicators();
            settings = new Settings();
        }
        public void Start()
        {
            lock (lockObject)
            {
                isStarted = true;
            }
            var task = new System.Threading.Tasks.Task(() => RunLoop());
            task.Start();
        }

        public void Stop()
        {
            lock (lockObject)
            {
                isStarted = false;
            }
            Discharge();
            // TODO
        }

        public void UpdateSettings(Settings settingsToSet)
        {
            lock (lockObject)
            {
                settings = settingsToSet;
            }
        }

        public void UpdateConnectionStatus(ConnectionStatusEventArgs args)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Update process items, in this case, those are indicator values
        /// </summary>
        /// <param name="args"></param>
        public void UpdateProcessItems(ProcessItemChangedEventArgs args)
        {
            lock (lockObject)
            {
                foreach (var item in args.ChangedItems)
                {
                    switch (item.Key)
                    {
                        case "TI100":
                            indicators.TI100 = (double)item.Value.GetValue();
                            break;
                        case "TI300":
                            indicators.TI300 = (double)item.Value.GetValue();
                            break;
                        case "FI100":
                            indicators.FI100 = (double)item.Value.GetValue();
                            break;
                        case "LI100":
                            indicators.LI100 = (int)item.Value.GetValue();
                            break;
                        case "LI200":
                            indicators.LI200 = (int)item.Value.GetValue();
                            break;
                        case "LI400":
                            indicators.LI400 = (int)item.Value.GetValue();
                            break;
                        case "PI300":
                            indicators.PI300 = (int)item.Value.GetValue();
                            break;
                        case "LS+300":
                            indicators.LSp300 = (bool)item.Value.GetValue();
                            break;
                        case "LS-300":
                            indicators.LSm300 = (bool)item.Value.GetValue();
                            break;
                        case "LS-200":
                            indicators.LSm200 = (bool)item.Value.GetValue();
                            break;
                        case "LA100":
                            indicators.LA100 = (bool)item.Value.GetValue();
                            break;
                        default:
                            // TODO throw exception ? 
                            break;
                    }
                }
            }
        }

        private void RunLoop()
        {
            while (isStarted){
                System.Console.WriteLine("Loop started");
                switch (stage)
                {
                    case Stage.impregnation:
                        mainWindowObject.setSequenceStage("Kyllästys");
                        Impregnation();
                        break;
                    case Stage.black_liquor_fill:
                        mainWindowObject.setSequenceStage("Mustalipeän täyttö");
                        BlackLiquorFill();
                        break;
                    case Stage.white_liquor_fill:
                        mainWindowObject.setSequenceStage("Valkolipeän täyttö");
                        WhiteLiquorFill();
                        break;
                    case Stage.cooking:
                        mainWindowObject.setSequenceStage("Keitto");
                        Cooking();
                        break;
                    case Stage.discharge:
                        mainWindowObject.setSequenceStage("Purku");
                        Discharge();
                        break;
                    default:
                        break;
                }
                lock (lockObject)
                {
                    stage++;
                }
            }
        }

        private void Impregnation()
        {
            // Phase 1:
            communicationObject.setItem("V201", true);
            communicationObject.setItem("V303", true);
            communicationObject.setItem("P100_P200_PRESET", true);
            communicationObject.setItem("P200", 100);
            communicationObject.setItem("V204", true);
            communicationObject.setItem("V301", true);

            System.Console.WriteLine("Waiting for LS+300");
            while (!indicators.LSp300){ }  // TODO: Failsafe?, miten toimii säikeiden kanssa?
            System.Console.WriteLine("LS+300 activated");

            // Phase 2:
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V401", false);

            System.Console.WriteLine("Waiting impregnation time");
            // Ei tarvi lukkoa koska asetuksia ei voi muuttaa sekvenssin aikana:
            int Ti = (int)settings.impregnationTime * 1000;
            Thread.Sleep(Ti);
            System.Console.WriteLine("Impregnation time passed");

            // Phase 3
            communicationObject.setItem("V201", false);
            communicationObject.setItem("V303", false);
            communicationObject.setItem("P200", 0);
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V301", false);
            communicationObject.setItem("V401", false);

            // Phase 4-5
            DepressurizeDigester();

        }
        private void BlackLiquorFill()
        {
            // Phase 1
            communicationObject.setItem("V204", true);
            communicationObject.setItem("V301", true);
            communicationObject.setItem("V303", true);
            communicationObject.setItem("P200", 100);
            communicationObject.setItem("V404", true);

            System.Console.WriteLine("Waiting for LI400 >= 35");
            while (indicators.LI400 >= 35) { }  // TODO Failsafe?
            System.Console.WriteLine("LI400 >= 35 done");

            // Phase 2
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V301", false);
            communicationObject.setItem("V401", false);
            communicationObject.setItem("V303", false);
            communicationObject.setItem("P200", 0);
            communicationObject.setItem("V404", false);
        }
        private void WhiteLiquorFill()
        {
            // Phase 1
            communicationObject.setItem("V301", true);
            communicationObject.setItem("V401", true);

            communicationObject.setItem("V102", 100);
            communicationObject.setItem("V304", true);
            communicationObject.setItem("P100", 100);

            System.Console.WriteLine("Waiting for LI400 >= 80");
            while (indicators.LI400 >= 80) { }  // TODO Failsafe?
            System.Console.WriteLine("LI400 >= 80 done");

            // Phase 2
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V301", false);
            communicationObject.setItem("V401", false);

            communicationObject.setItem("V102", 0);
            communicationObject.setItem("V304", false);
            communicationObject.setItem("P100", 0);
        }
        private void Cooking()
        {
            // Phase 1
            communicationObject.setItem("V104", 100);
            communicationObject.setItem("V301", true);

            communicationObject.setItem("V102", 100);
            communicationObject.setItem("V304", true);
            communicationObject.setItem("P100", 100);
            communicationObject.setItem("E100", true);

            double T = settings.cookingTemperature;
            double p = settings.cookingPressure;
            double Tc = settings.cookingTime;
            System.Console.WriteLine("Waiting for TI300 > cookingTemperature");
            while (indicators.TI300 > T) { } // TODO Failsafe?
            System.Console.WriteLine("TI300 > cookingTemperature done");

            // Phase 2
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V401", false);

            communicationObject.setItem("V102", 100);
            communicationObject.setItem("V304", true);
            communicationObject.setItem("P100", 100);

            System.Console.WriteLine("Control loop for TI300 = cookingTemperature and PI300 = cookingPressure for cookingTime");
            // Phase 3 lämpötila säätö TI300 == T ja paineen säätö PI300 == p ajan Tc
            // TODO
            System.Console.WriteLine("Control loop done");

            // Phase 4
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("E100", false);

            // Phase 5
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V301", false);
            communicationObject.setItem("V401", false);

            communicationObject.setItem("V102", 0);
            communicationObject.setItem("V304", false);
            communicationObject.setItem("P100", 0);

            // Phase 6-7
            DepressurizeDigester();

        }
        private void Discharge()
        {
            // Phase 1
            communicationObject.setItem("V103", true);
            communicationObject.setItem("V303", true);
            communicationObject.setItem("P200", 100);

            communicationObject.setItem("V204", true);
            communicationObject.setItem("V302", true);

            // "LS-300 is deactivated"
            System.Console.WriteLine("Waiting for LS-300 to deactivate");
            while (indicators.LSm300) { } // TODO Failsafe?
            System.Console.WriteLine("LS-300 deactivated");

            // Phase 2
            communicationObject.setItem("V103", false);
            communicationObject.setItem("V303", false);
            communicationObject.setItem("P200", 0);

            communicationObject.setItem("V204", false);
            communicationObject.setItem("V302", false);
        }

        /// <summary>
        /// Helper function for the functionality
        /// EM3_OP8 Depressurize digester/T300
        /// </summary>
        private void DepressurizeDigester()
        {
            System.Console.WriteLine("Depressurizing digester");
            communicationObject.setItem("V204", true);
            Thread.Sleep(1000);  // Td = 1s
            communicationObject.setItem("V204", false);
            System.Console.WriteLine("Digester depressurized");
        }
    }
}
