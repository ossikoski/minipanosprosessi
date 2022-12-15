using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Minipanosprosessi;
using Moq;

namespace UnitTestProject
{
    [TestClass]
    public class TestControlSystem
    {
        ControlSystem controlSystem;
        MainWindow mockMainWindow;
        Communication mockCommunication;

        MainWindow mainWindow;
        Communication communication;
        public class MockMainWindow
        {
            private object lockObject;
            public MockMainWindow()
            {
                lockObject = new object();
            }
        }
        public class MockCommunication
        {
            private object lockObject;
            public MockCommunication()
            {
                lockObject = new object();

            }
        }
        public TestControlSystem()
        {
            //mockMainWindow = Mock.Of<MainWindow>();
            mainWindow = new MainWindow();
            communication = new Communication(mainWindow);
            mockCommunication = Mock.Of<Communication>();
            controlSystem = new ControlSystem(communication, mainWindow);
        }

        [TestMethod]
        public void TestStart()
        {
            
        }

    }
}
