using System;
using System.Collections.Generic;
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
    public partial class MainWindow : Window
    {
        private MppClient client;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            // Demo
            client.SetOnOffItem("V201", true);
            client.SetOnOffItem("V303", true);
            client.SetOnOffItem("P100_P200_PRESET", true);
            client.SetPumpControl("P200", 100);
            client.SetOnOffItem("V204", true);
            client.SetOnOffItem("V301", true);
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            // Demotarkoituksiin
            V204image.Source = new BitmapImage(new Uri(@"Media\valveClosed.png", UriKind.Relative));
            connectionLight.Fill = Brushes.Red;
            T200level.Value = 0;
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionParamsHolder par = new ConnectionParamsHolder("opc.tcp://localhost:8087");
            client = new MppClient(par);

            client.ConnectionStatus += new MppClient.ConnectionStatusEventHandler(ConnectionEvent);
            client.ProcessItemsChanged += new MppClient.ProcessItemsChangedEventHandler(ProcessItemsEvent);

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
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ConnectionEvent(object source, ConnectionStatusEventArgs args)
        {
            if(args.StatusInfo.SimplifiedStatus.Equals(ConnectionStatusInfo.StatusType.Connected))
            {
                connectionLight.Dispatcher.Invoke(() =>
                {
                    connectionLight.Fill = Brushes.Lime;
                });
                MessageBox.Show("Yhteyden muodostus onnistui", "Info");
            }
            else if (args.StatusInfo.SimplifiedStatus.Equals(ConnectionStatusInfo.StatusType.Connecting))
            {
                connectionLight.Dispatcher.Invoke(() =>
                {
                    connectionLight.Fill = Brushes.Yellow;
                });
            }
            else
            {
                connectionLight.Dispatcher.Invoke(() =>
                {
                    connectionLight.Fill = Brushes.Red;
                });
            }
        }

        private void setTextBlock(TextBlock textBlock, MppValue value, string unit)
        {
            textBlock.Dispatcher.Invoke(() =>
            {
                string text;
                text = value.GetValue().ToString();
                text += unit;
                textBlock.Text = text;
            });
        }

        private void setLevelIndicator(ProgressBar levelIndicator, MppValue value, int maxValue)
        {
            levelIndicator.Dispatcher.Invoke(() =>
            {
                double percentage;
                int intValue;
                intValue = (int) value.GetValue();
                percentage = ((double) intValue / maxValue) * 100;
                levelIndicator.Value = percentage;
            });
        }

        private void changeItemImage(Image image, string indicatorType, MppValue value)
        {
            Console.WriteLine(image.ToString());
            Console.WriteLine(indicatorType);
            Console.WriteLine(value.ToString());


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

            image.Dispatcher.Invoke(() =>
            {
                image.Source = new BitmapImage(new Uri(path, UriKind.Relative));
            });
        }

        private void ProcessItemsEvent(object source, ProcessItemChangedEventArgs args)
        {
            
            foreach(var item in args.ChangedItems)
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
