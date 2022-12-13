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

            while (!indicators.LSp300){}  // TODO: Failsafe?, miten toimii säikeiden kanssa?

            // Phase 2:
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V401", false);

            // Ei tarvi lukkoa koska asetuksia ei voi muuttaa sekvenssin aikana:
            int Ti = (int)settings.impregnationTime * 1000;
            Thread.Sleep(Ti);

            // Phase 3
            communicationObject.setItem("V201", false);
            communicationObject.setItem("V303", false);
            communicationObject.setItem("P200", 0);
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V301", false);
            communicationObject.setItem("V401", false);

            // Phase 4
            communicationObject.setItem("V204", true);
            Thread.Sleep(1000);
            
            // Phase 5
            communicationObject.setItem("V204", false);
        }
        private void BlackLiquorFill()
        {
            // Phase 1
            communicationObject.setItem("V204", true);
            communicationObject.setItem("V301", true);
            communicationObject.setItem("V303", true);
            communicationObject.setItem("P200", 100);
            communicationObject.setItem("V404", true);

            while (indicators.LI400 >= 35) { }  // TODO Failsafe?

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

            while (indicators.LI400 >= 80) { }  // TODO Failsafe?

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

            double T = settings.cookingTemperature;  // Ei tarvi lukkoa koska asetuksia ei voi muuttaa sekvenssin aikana
            double p = settings.cookingTemperature;
            double Tc = settings.cookingTime;
            while (indicators.TI300 > T) { } // TODO Failsafe?

            // Phase 2
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V401", false);

            communicationObject.setItem("V102", 100);
            communicationObject.setItem("V304", true);
            communicationObject.setItem("P100", 100);

            // Phase 3 lämpötila säätö == T ja paineen säätö == p ajan Tc
            // TODO
        }
        private void Discharge()
        {

        }
    }
}
