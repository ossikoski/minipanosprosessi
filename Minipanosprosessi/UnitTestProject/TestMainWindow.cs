using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using Minipanosprosessi;
using Moq;
using Tuni.MppOpcUaClientLib;

namespace UnitTestProject
{
    [TestClass]
    public class TestMainWindow
    {
        ControlSystem controlSystem;
        //MainWindow mockMainWindow;
        //Communication mockCommunication;

        MainWindow mainWindow;
        Communication communication;
        public class correctTextBox
        {
            public string Text = "3.6";
        }
        public TestMainWindow()
        {
            //mockMainWindow = Mock.Of<MainWindow>();
            mainWindow = new MainWindow();
            communication = new Communication(mainWindow);
            //mockCommunication = Mock.Of<Communication>();
            controlSystem = new ControlSystem(communication, mainWindow);
        }

        [TestMethod]
        public void TestConnectionStatus()
        {
            ////ConnectionStatusEventArgs mockArgs = Mock.Of<ConnectionStatusEventArgs>();
            //ConnectionStatusEventArgs args = new ConnectionStatusEventArgs();

            //mainWindow.UpdateConnectionStatus();
        }

        [TestMethod]
        public void TestSettings()
        {
            Mock<MainWindow> name = new Mock<MainWindow>();
            object sender = new object();
            RoutedEventArgs e = new RoutedEventArgs();

            name.Setup(x => x.cookingTimeTextBox.Text).Returns("3.6");
            mainWindow.settingsButton_Click(sender, e);

            name.Setup(x => x.cookingTimeTextBox.Text).Returns("väärä muoto");
            Assert.ThrowsException<FormatException>(() => mainWindow.settingsButton_Click(sender, e));
        }
    }
}
