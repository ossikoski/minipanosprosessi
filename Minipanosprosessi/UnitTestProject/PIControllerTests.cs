using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minipanosprosessi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minipanosprosessi.Tests
{
    [TestClass()]
    public class PIControllerTests
    {
        /// <summary>
        /// Verify that cotnroller works in inverted direction.
        /// </summary>
        [TestMethod()]
        public void WorksInvertedTest()
        {
            // arrange
            double kp = 1.0;
            double ki = 1.0;
            int tsMs = 1000;
            double setpoint = 10.0;
            double measuredValue = 5.0;
            double expected1 = -10.0;
            double expected2 = -15.0;
            var controller = new PIController(kp, ki, tsMs);
            controller.setSetpoint(setpoint);

            // act
            double output1 = controller.updateOutput(measuredValue);
            double output2 = controller.updateOutput(measuredValue);

            // assert
            Assert.AreEqual(expected1, output1, 0.01);
            Assert.AreEqual(expected2, output2, 0.01);
        }

        /// <summary>
        /// Verify that controller with cycle time 10 times smaller than other 
        /// controller produces same output when called 10 times. 
        /// </summary>
        [TestMethod()]
        public void DifferentCycleTimesTest()
        {
            // arrange
            double kp = 1.0;
            double ki = 1.0;
            int divider = 10;
            int tsMs1 = 1000;
            int tsMs2 = tsMs1 / divider;
            double setpoint = 10.0;
            double measuredValue = 5.0;
            var controller1 = new PIController(kp, ki, tsMs1);
            var controller2 = new PIController(kp, ki, tsMs2);
            controller1.setSetpoint(setpoint);
            controller2.setSetpoint(setpoint);

            // act
            double output1 = controller1.updateOutput(measuredValue);

            double output2 = 0;
            for(int i = 0; i < divider; i++)
            {
                output2 = controller2.updateOutput(measuredValue);
            }

            // assert
            Assert.AreEqual(output1, output2, 0.01);
        }

        /// <summary>
        /// Verify that controller with 10 times greater gains porduce 10 times
        /// greater output.
        /// </summary>
        [TestMethod()]
        public void IsLinearTest()
        {
            // arrange
            int multiplier = 10;
            double kp1 = 1.0;
            double ki1 = 1.0;
            double kp2 = multiplier * kp1;
            double ki2 = multiplier * ki1;
            int tsMs = 1000;
            double setpoint = 10.0;
            double measuredValue = 5.0;
            var controller1 = new PIController(kp1, ki1, tsMs);
            var controller2 = new PIController(kp2, ki2, tsMs);
            controller1.setSetpoint(setpoint);
            controller2.setSetpoint(setpoint);

            // act
            double output1 = controller1.updateOutput(measuredValue);
            double output2 = controller2.updateOutput(measuredValue);
            double output1Multiplied = output1 * multiplier;

            // assert
            Assert.AreEqual(output1Multiplied, output2, 0.01);
        }
    }
}