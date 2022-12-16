using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;
using Tuni.MppOpcUaClientLib;

namespace Minipanosprosessi
{
    /// <summary>
    /// To keep track of the current stage of the process.
    /// </summary>
    enum Stage
    {
        impregnation,
        black_liquor_fill,
        white_liquor_fill,
        cooking,
        discharge
    }

    /// <summary>
    /// Indicator values from the system
    /// </summary>
    public struct Indicators
    {
        // starting values are: (20, 20, 0, 216, 90, 90, 0, false, true, true, true)
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
        public bool LA100;  // false -> alarm is active and level is under 100
    }

    /// <summary>
    /// Settings for the pulp control
    /// </summary>
    public struct Settings
    {
        public double cookingTime;
        public double cookingPressure;
        public double cookingTemperature;
        public double impregnationTime;
    }

    /// <summary>
    /// Control the pulp process sequence
    /// </summary>
    class ControlSystem : IProcessObserver
    {
        Communication communicationObject;
        MainWindow mainWindowObject;
        bool isStarted;
        Stage stage;
        Settings settings;
        Indicators indicators;
        private object lockObject = new object();
        private static System.Timers.Timer controllerTimer;
        PIController pressureController;
        System.Threading.Tasks.Task task;  // Another thread

        /// <summary>
        /// Control the pulp process sequence
        /// </summary>
        /// <param name="communication">The Communication object</param>
        /// <param name="mainWindow">The MainWindow object</param>
        public ControlSystem(Communication communication, MainWindow mainWindow)
        {
            communicationObject = communication;
            mainWindowObject = mainWindow;
            isStarted = false;
            stage = Stage.impregnation;
            indicators = new Indicators();
            settings = new Settings();

            // Configure pressure controller
            double kp = 0;
            double ki = 0.5;
            int controlCycleMs = 100;
            pressureController = new PIController(kp, ki, controlCycleMs);
            controllerTimer = new System.Timers.Timer(controlCycleMs);
            controllerTimer.Elapsed += PressureControllerEvent;
            controllerTimer.AutoReset = true;
        }

        /// <summary>
        /// Start the pulp process control.
        /// Will create another thread for the RunLoop method.
        /// </summary>
        public void Start()
        {
            lock (lockObject)
            {
                isStarted = true;
            }
            task = new System.Threading.Tasks.Task(() => RunLoop());
            task.Start();
        }

        /// <summary>
        /// Stop the pulp process control
        /// </summary>
        public void Stop()
        {
            System.Console.WriteLine("Stop called");
            lock (lockObject)
            {
                isStarted = false;
            }
            Thread.Sleep(100);
            communicationObject.setItem("V102", 0);
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V103", false);
            communicationObject.setItem("V201", false);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V301", false);
            communicationObject.setItem("V302", false);
            communicationObject.setItem("V303", false);
            communicationObject.setItem("V304", false);
            communicationObject.setItem("V401", false);
            communicationObject.setItem("V404", false);
            communicationObject.setItem("P100", 0);
            communicationObject.setItem("P200", 0);
            communicationObject.setItem("E100", false);

            mainWindowObject.setSequenceStage("Keskeytetty");
            stage = Stage.impregnation;
        }

        /// <summary>
        /// Update settings for the ControlSystem.
        /// </summary>
        /// <param name="settingsToSet">Settings struct that includes the setting values.</param>
        public void UpdateSettings(Settings settingsToSet)
        {
            System.Console.WriteLine("ControlSystem settings changed");
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
        /// <param name="args">Which process items have changed and their values</param>
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

        /// <summary>
        /// Event to control the pressure by adjusting the position of V104
        /// </summary>
        /// <param name="source">The event sender object</param>
        /// <param name="e">ElapsedEventArgs object</param>
        private void PressureControllerEvent(Object source, ElapsedEventArgs e)
        {
            double control = pressureController.updateOutput(indicators.PI300);
            int intControl = (int)control;

            if(intControl < 0)
            {
                intControl = 0;
            }
            else if(intControl > 100)
            {
                intControl = 100;
            }

            communicationObject.setItem("V104", intControl);
        }

        /// <summary>
        /// Run the pulp process sequence loop and keep track of the stage
        /// </summary>
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
                if(stage == Stage.discharge)
                {
                    Stop();
                    mainWindowObject.setSequenceStage("Valmis");
                    break;
                }
                lock (lockObject)
                {
                    stage++;
                }
            }
            System.Console.WriteLine("RunLoop done");
        }

        /// <summary>
        /// Impregnation stage for the pulp process sequence.
        /// </summary>
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
            while (!indicators.LSp300){ if (!isStarted) { return; } }
            System.Console.WriteLine("LS+300 activated");

            // Phase 2:
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V401", false);

            System.Console.WriteLine("Waiting impregnation time");
            // Ei tarvi lukkoa koska asetuksia ei voi muuttaa sekvenssin aikana
            int Ti = (int)settings.impregnationTime * 1000;
            var timer = new Stopwatch();
            timer.Start();
            TimeSpan timeTaken = timer.Elapsed;
            while (timeTaken.TotalMilliseconds < Ti) { 
                if (!isStarted) { return; } 
                timeTaken = timer.Elapsed;
            }
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

        /// <summary>
        /// Black liquor fill stage for the pulp process sequence.
        /// </summary>
        private void BlackLiquorFill()
        {
            // Phase 1
            communicationObject.setItem("V204", true);
            communicationObject.setItem("V301", true);
            communicationObject.setItem("V303", true);
            communicationObject.setItem("P200", 100);
            communicationObject.setItem("V404", true);

            System.Console.WriteLine("Waiting for LI400 < 35");
            while (indicators.LI400 >= 35) { if (!isStarted) { return; } }
            System.Console.WriteLine("LI400 < 35 done");

            // Phase 2
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V301", false);
            communicationObject.setItem("V401", false);
            communicationObject.setItem("V303", false);
            communicationObject.setItem("P200", 0);
            communicationObject.setItem("V404", false);
        }

        /// <summary>
        /// White liquor fill stage for the pulp process sequence.
        /// </summary>
        private void WhiteLiquorFill()
        {
            // Phase 1
            communicationObject.setItem("V301", true);
            communicationObject.setItem("V401", true);

            communicationObject.setItem("V102", 100);
            communicationObject.setItem("V304", true);
            communicationObject.setItem("P100", 100);

            System.Console.WriteLine("Waiting for LI400 > 80");
            while (indicators.LI400 <= 80) { if (!isStarted) { return; } }
            System.Console.WriteLine("LI400 > 80 done");

            // Phase 2
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V301", false);
            communicationObject.setItem("V401", false);

            communicationObject.setItem("V102", 0);
            communicationObject.setItem("V304", false);
            communicationObject.setItem("P100", 0);
        }

        /// <summary>
        /// Cooking stage for the pulp process sequence.
        /// </summary>
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
            System.Console.WriteLine("Waiting for TI100 == cookingTemperature");
            while (indicators.TI100 < T + 0.4) { if (!isStarted) { return; } }
            System.Console.WriteLine("TI100 == cookingTemperature done");

            // Phase 2
            communicationObject.setItem("V104", 0);
            communicationObject.setItem("V204", false);
            communicationObject.setItem("V401", false);

            communicationObject.setItem("V102", 100);
            communicationObject.setItem("V304", true);
            communicationObject.setItem("P100", 100);

            System.Console.WriteLine("Control loop for TI300 = cookingTemperature and PI300 = cookingPressure for cookingTime");
            // Phase 3 lämpötila säätö TI300 == T ja paineen säätö PI300 == p ajan Tc
            DateTime startTime = DateTime.Now;
            double diffSeconds = 0;

            // Start pressure control
            pressureController.setSetpoint(settings.cookingPressure);
            controllerTimer.Start();
            
            while (diffSeconds < Tc)
            {
                if (!isStarted) { return; }

                if (indicators.TI100 > T)
                {
                    communicationObject.setItem("E100", false);
                }
                if (indicators.TI100 < T)
                {
                    communicationObject.setItem("E100", true);
                }
                diffSeconds = (DateTime.Now - startTime).TotalSeconds;

                // Requirements: T +-0.3, p +-10 hPa
                if (indicators.TI300 > T + 0.3 || indicators.TI300 < T - 0.3)
                {
                    if(diffSeconds > 5)
                    {
                        System.Console.WriteLine("Temperature differed over 0.3 degrees.");
                    }
                    startTime = DateTime.Now;
                }
                if (indicators.PI300 > p + 10 || indicators.PI300 < p - 10)
                {
                    if (diffSeconds > 5)
                    {
                        System.Console.WriteLine("Pressure differed over 10 hPa.");
                    }
                    startTime = DateTime.Now;
                }
            }
            // Stop pressure control
            controllerTimer.Stop();
            System.Console.WriteLine("Control loop done.");

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

        /// <summary>
        /// Discharge stage for the pulp process sequence.
        /// </summary>
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
            while (indicators.LSm300){ if (!isStarted) { return; } }
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
