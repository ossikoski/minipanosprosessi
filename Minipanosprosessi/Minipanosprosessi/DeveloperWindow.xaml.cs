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
using System.Windows.Shapes;
using Tuni.MppOpcUaClientLib;


namespace Minipanosprosessi
{
    /// <summary>
    /// Interaction logic for DeveloperWindow.xaml
    /// </summary>
    public partial class DeveloperWindow : Window
    {
        private MppClient client;
        public DeveloperWindow()
        {
            InitializeComponent();
            ConnectionParamsHolder par = new ConnectionParamsHolder("opc.tcp://localhost:8087");
            client = new MppClient(par);

            client.ConnectionStatus += new MppClient.ConnectionStatusEventHandler(ConnectionEvent);
            client.ProcessItemsChanged += new MppClient.ProcessItemsChangedEventHandler(ProcessItemsEvent);

            client.Init();
            
            client.AddToSubscription("TI300");

            Console.ReadLine();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            client.SetOnOffItem("V201", true);
            client.SetOnOffItem("V303", true);
            client.SetOnOffItem("P100_P200_PRESET", true);
            client.SetPumpControl("P200", 100);
            client.SetOnOffItem("V204", true);
            client.SetOnOffItem("V301", true);
        }

        private static void ConnectionEvent(object source, ConnectionStatusEventArgs args)
        {
            System.Console.WriteLine("asdasd");
        }

        private static void ProcessItemsEvent(object source, ProcessItemChangedEventArgs args)
        {
            System.Console.WriteLine("asdasd");
        }
    }
}
