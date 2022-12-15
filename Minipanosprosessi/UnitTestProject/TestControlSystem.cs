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
            var mockMainWindow = Mock.Of<MainWindow>();
            var mockCommunication = Mock.Of<Communication>();
            controlSystem = new ControlSystem(mockCommunication, mockMainWindow);
        }

        [TestMethod]
        public void TestMethod1()
        {
            controlSystem.Start();
        }
    }
}
