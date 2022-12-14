using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tuni.MppOpcUaClientLib;


namespace Minipanosprosessi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IProcessObserver
    {
        private Communication communication;
        private ControlSystem controlSystem;
        private object lockObject = new object();

        public MainWindow()
        {
            InitializeComponent();

            communication = new Communication(this);
            controlSystem = new ControlSystem(communication, this);

            // Add observers
            communication.AddObserver(controlSystem);
            communication.AddObserver(this);
        }

        /// <summary>
        /// Event handler for when startButton is clicked
        /// </summary>
        /// <param name="sender">The event sender object</param>
        /// <param name="e">RoutedEventArgs object</param>
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            controlSystem.Start();
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            settingsButton.IsEnabled = false;
        }

        /// <summary>
        /// Event handler for when stopButton is clicked
        /// </summary>
        /// <param name="sender">The event sender object</param>
        /// <param name="e">RoutedEventArgs object</param>
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            controlSystem.Stop();
            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
            settingsButton.IsEnabled = true;
        }

        /// <summary>
        /// Event handler for when connectButton is clicked
        /// </summary>
        /// <param name="sender">The event sender object</param>
        /// <param name="e">RoutedEventArgs object</param>
        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            communication.Connect();
        }

        /// <summary>
        /// Event handler for when settingsButton is clicked
        /// Settings will be saved and sent to ControlSystem
        /// </summary>
        /// <param name="sender">The event sender object</param>
        /// <param name="e">RoutedEventArgs object</param>
        public void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();

            // TODO Lisää validointia asetuksille?
            try
            {
                settings.cookingTime = double.Parse(cookingTimeTextBox.Text, CultureInfo.InvariantCulture);
                settings.cookingPressure = double.Parse(cookingPressureTextBox.Text, CultureInfo.InvariantCulture);
                settings.cookingTemperature = double.Parse(cookingTemperatureTextBox.Text, CultureInfo.InvariantCulture);
                settings.impregnationTime = double.Parse(impregnationTimeTextBox.Text, CultureInfo.InvariantCulture);

                // Make sure that parameters are positive
                if(settings.cookingTime > 0 &&
                   settings.cookingPressure > 0 &&
                   settings.cookingTemperature > 0 &&
                   settings.impregnationTime > 0)
                {
                    controlSystem.UpdateSettings(settings);
                    startButton.IsEnabled = true;

                    showMessage("Asetukset tallennettu", "Info");
                }
                else
                {
                    showMessage("Kaikkien asetusten tulee olla positiivisia.", "Virhe");
                }

                
            }
            catch(Exception ex)
            {
                showMessage("Asetusten arvot ovat virheelliset.\n\nInfo:\n" + ex.Message, "Virhe");
            }
        }

        /// <summary>
        /// Set text (only) for the indicator/valve text boxes
        /// </summary>
        /// <param name="textBlock">Text block component name.</param>
        /// <param name="value">Text value to set for the text block.</param>
        /// <param name="unit">Optional unit that will be added to the end of the value text.</param>
        private void setTextBlock(TextBlock textBlock, MppValue value, string unit = "")
        {
            string text;
            text = value.GetValue().ToString();
            // Katkaistaan desimaaliluku kahteen desimaaliin kaikissa mis on ",".
            // Ei pyöristystä käyttöliittymään. Taustalla käytetään tarkkoja arvoja.
            if (text.Contains(","))
            {
                string[] strings = text.Split(',');
                try
                {
                    text = strings[0] + "," + strings[1].Substring(0, 2);
                }
                catch
                {

                }
                
            }
            text += unit;

            // TODO: vaihda Invoke -> BeginInvoke
            textBlock.Dispatcher.Invoke(() =>
            {
                textBlock.Text = text;  // private -> ei lukkoa
            });
        }
        
        /// <summary>
        /// Set text for the TextBlock sequenceTextBlock
        /// </summary>
        /// <param name="text">Text to set</param>
        public void setSequenceStage(string text)
        {
            TextBlock textBlock = sequenceTextBlock;
            textBlock.Dispatcher.Invoke(() =>
            {
                lock (lockObject)
                {
                    textBlock.Text = text;
                }
            });
        }

        /// <summary>
        /// Pops up a message dialog to show a message
        /// </summary>
        /// <param name="text"></param>
        public void showMessage(string text, string header)
        {
            lock (lockObject)
            {
                MessageBox.Show(text, header);
            }
        }

        /// <summary>
        /// Set a level indicator progressbar based on the percentage
        /// </summary>
        /// <param name="levelIndicator">ProgressBar object, the target to set the indicator for</param>
        /// <param name="value">Current value of tha quantity</param>
        /// <param name="maxValue">Maximum value of the quantity</param>
        private void setLevelIndicator(ProgressBar levelIndicator, MppValue value, int maxValue)
        {
            double percentage;
            int intValue;
            intValue = (int)value.GetValue();
            percentage = ((double)intValue / maxValue) * 100;

            // TODO: vaihda Invoke -> BeginInvoke
            levelIndicator.Dispatcher.Invoke(() =>
            {
                levelIndicator.Value = percentage;
            });
        }

        /// <summary>
        /// Change heater, pump or valve image based on on/off or open/closed position
        /// </summary>
        /// <param name="image">Image to set</param>
        /// <param name="indicatorType">The type of the indicator: "heater", "pump", "valve" or "controlValve".</param>
        /// <param name="value">Value of the stage of the item</param>
        private void changeItemImage(Image image, string indicatorType, MppValue value)
        {
            bool bValue = false;
            int intValue = 0;
            string path;

            if (value.ValueType.Equals(MppValue.ValueTypeType.Bool))
            {
                bValue = (bool)value.GetValue();
            }
            else if(value.ValueType.Equals(MppValue.ValueTypeType.Int))
            {
                intValue = (int)value.GetValue();
            }

            switch (indicatorType)
            {
                case "valve":
                    path = bValue ? @"Media\valveOpen.png" : @"Media\valveClosed.png";
                    break;
                case "heater":
                    path = bValue ? @"Media\heaterOn.png" : @"Media\heaterOff.png";
                    break;
                case "pump":
                    path = (intValue > 0) ? @"Media\pumpOn.png" : @"Media\pumpOff.png";
                    break;
                case "controlValve":
                    path = (intValue > 0) ? @"Media\controlValveOpen.png" : @"Media\controlValveClosed.png";
                    break;
                default:
                    path = "";
                    break;
            }

            // TODO: vaihda Invoke -> BeginInvoke
            image.Dispatcher.Invoke(() =>
            {
                image.Source = new BitmapImage(new Uri(path, UriKind.Relative));
            });
        }

        
        /// <summary>
        /// ConnectionStatus is updated in UI with connectionLight.
        /// Also, if connection is green, settings can be saved and then sequence started, otherwise not.
        /// If connection is green, connect button is disabled
        /// </summary>
        /// <param name="args">Information on the connection status</param>
        public void UpdateConnectionStatus(ConnectionStatusEventArgs args)
        {
            if (args.StatusInfo.SimplifiedStatus.Equals(ConnectionStatusInfo.StatusType.Connected))
            {
                // TODO: vaihda Invoke -> BeginInvoke
                settingsButton.Dispatcher.Invoke(() =>
                {
                    settingsButton.IsEnabled = true;
                });
                connectButton.Dispatcher.Invoke(() =>
                {
                    connectButton.IsEnabled = false;
                });

                connectionLight.Dispatcher.Invoke(() =>
                {
                    connectionLight.Fill = Brushes.Lime;
                });
            }
            else if (args.StatusInfo.SimplifiedStatus.Equals(ConnectionStatusInfo.StatusType.Connecting))
            {
                // TODO: vaihda Invoke -> BeginInvoke
                settingsButton.Dispatcher.Invoke(() =>
                {
                    settingsButton.IsEnabled = false;
                });
                startButton.Dispatcher.Invoke(() =>
                {
                    startButton.IsEnabled = false;
                });
                stopButton.Dispatcher.Invoke(() =>
                {
                    stopButton.IsEnabled = false;
                });
                connectButton.Dispatcher.Invoke(() =>
                {
                    connectButton.IsEnabled = true;
                });

                connectionLight.Dispatcher.Invoke(() =>
                {
                    connectionLight.Fill = Brushes.Yellow;
                });
            }
            else  // status Disconnected tai Unknown
            {
                // TODO: vaihda Invoke -> BeginInvoke
                settingsButton.Dispatcher.Invoke(() =>
                {
                    settingsButton.IsEnabled = false;
                });
                startButton.Dispatcher.Invoke(() =>
                {
                    startButton.IsEnabled = false;
                });
                stopButton.Dispatcher.Invoke(() =>
                {
                    stopButton.IsEnabled = false;
                });
                connectButton.Dispatcher.Invoke(() =>
                {
                    connectButton.IsEnabled = true;
                });

                connectionLight.Dispatcher.Invoke(() =>
                {
                    connectionLight.Fill = Brushes.Red;
                });
            }
        }

        /// <summary>
        /// Update process item values.
        /// </summary>
        /// <param name="args">Information on which process items have changed and their values</param>
        public void UpdateProcessItems(ProcessItemChangedEventArgs args)
        {
            lock (lockObject)
            {
                foreach (var item in args.ChangedItems)
                {
                    switch (item.Key)
                    {
                        case "TI100":
                            setTextBlock(TI100textBlock, item.Value, "°C");
                            break;
                        case "TI300":
                            setTextBlock(TI300textBlock, item.Value, "°C");
                            break;
                        case "FI100":
                            setTextBlock(FI100textBlock, item.Value, " l/min");
                            break;
                        case "LI100":
                            setTextBlock(LI100textBlock, item.Value, " mm");
                            setLevelIndicator(T100level, item.Value, 300);
                            break;
                        case "LI200":
                            setTextBlock(LI200textBlock, item.Value, " mm");
                            setLevelIndicator(T200level, item.Value, 300);
                            break;
                        case "LI400":
                            setTextBlock(LI400textBlock, item.Value, " mm");
                            setLevelIndicator(T400level, item.Value, 300);
                            break;
                        case "PI300":
                            setTextBlock(PI300textBlock, item.Value, " hPa");
                            break;
                        case "V102":
                            //setTextBlock(V102textBlock, item.Value, "%");
                            changeItemImage(V102image, "controlValve", item.Value);
                            break;
                        case "V104":
                            setTextBlock(V104textBlock, item.Value, "%");
                            changeItemImage(V104image, "controlValve", item.Value);
                            break;
                        case "V103":
                            changeItemImage(V103image, "valve", item.Value);
                            break;
                        case "V201":
                            changeItemImage(V201image, "valve", item.Value);
                            break;
                        case "V204":
                            changeItemImage(V204image, "valve", item.Value);
                            break;
                        case "V301":
                            changeItemImage(V301image, "valve", item.Value);
                            break;
                        case "V302":
                            changeItemImage(V302image, "valve", item.Value);
                            break;
                        case "V303":
                            changeItemImage(V303image, "valve", item.Value);
                            break;
                        case "V304":
                            changeItemImage(V304image, "valve", item.Value);
                            break;
                        case "V401":
                            changeItemImage(V401image, "valve", item.Value);
                            break;
                        case "V404":
                            changeItemImage(V404image, "valve", item.Value);
                            break;
                        case "P100":
                            changeItemImage(P100image, "pump", item.Value);
                            break;
                        case "P200":
                            changeItemImage(P200image, "pump", item.Value);
                            break;
                        case "E100":
                            changeItemImage(E100image, "heater", item.Value);
                            break;
                        default:
                            // code block
                            break;
                    }
                }
            }
        }
    }
}
