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

            communication = new Communication();
            controlSystem = new ControlSystem(communication, this);

            // Add observers
            communication.AddObserver(controlSystem);
            communication.AddObserver(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            controlSystem.Start();
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            settingsButton.IsEnabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            controlSystem.Stop();
            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
            settingsButton.IsEnabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            communication.Connect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: parametreja ei voi muokata sekvenssin suorituksen aikana
            Settings settings = new Settings();

            // TODO Lisää validointia asetuksille?
            try
            {
                settings.cookingTime = double.Parse(cookingTimeTextBox.Text, CultureInfo.InvariantCulture);
                settings.cookingPressure = double.Parse(cookingPressureTextBox.Text, CultureInfo.InvariantCulture);
                settings.cookingTemperature = double.Parse(cookingTemperatureTextBox.Text, CultureInfo.InvariantCulture);
                settings.impregnationTime = double.Parse(impregnationTimeTextBox.Text, CultureInfo.InvariantCulture);

                controlSystem.UpdateSettings(settings);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Asetusten arvot ovat virheelliset.\n\nInfo:\n" + ex.Message, "Virhe");
            }
        }

        /// <summary>
        /// Set text (only) for the inidcator/valve text boxes
        /// </summary>
        /// <param name="textBlock">Text block component name.</param>
        /// <param name="value">Text value to set for the text block.</param>
        /// <param name="unit">Optional unit that will be added to the end of the value text.</param>
        private void setTextBlock(TextBlock textBlock, MppValue value, string unit = "")
        {
            string text;
            text = value.GetValue().ToString();
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
        /// 
        /// </summary>
        /// <param name="levelIndicator"></param>
        /// <param name="value"></param>
        /// <param name="maxValue"></param>
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
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="indicatorType"></param>
        /// <param name="value"></param>
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
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void UpdateConnectionStatus(ConnectionStatusEventArgs args)
        {
            if (args.StatusInfo.SimplifiedStatus.Equals(ConnectionStatusInfo.StatusType.Connected))
            {
                // TODO: vaihda Invoke -> BeginInvoke
                connectionLight.Dispatcher.Invoke(() =>
                {
                    connectionLight.Fill = Brushes.Lime;
                });
            }
            else if (args.StatusInfo.SimplifiedStatus.Equals(ConnectionStatusInfo.StatusType.Connecting))
            {
                // TODO: vaihda Invoke -> BeginInvoke
                connectionLight.Dispatcher.Invoke(() =>
                {
                    connectionLight.Fill = Brushes.Yellow;
                });
            }
            else
            {
                // TODO: vaihda Invoke -> BeginInvoke
                connectionLight.Dispatcher.Invoke(() =>
                {
                    connectionLight.Fill = Brushes.Red;
                });
            }
        }

        /// <summary>
        /// 
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
