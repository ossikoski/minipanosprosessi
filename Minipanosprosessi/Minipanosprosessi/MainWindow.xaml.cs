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

namespace Minipanosprosessi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            V204image.Source = new BitmapImage(new Uri(@"Media\valveOpen.png", UriKind.Relative));
            T200level.Value = 80;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            V204image.Source = new BitmapImage(new Uri(@"Media\valveClosed.png", UriKind.Relative));
            T200level.Value = 20;
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            connectionLight.Fill = Brushes.Lime;
        }
    }
}
