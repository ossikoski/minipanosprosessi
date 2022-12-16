using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minipanosprosessi
{
    /// <summary>
    /// A basic PI controller
    /// Used in this project to control pressure
    /// Works in inverted direction
    /// </summary>
    public class PIController
    {
        double Kp; // Proportional gain 
        double Ki; // Integral gain
        double integrator;
        double setpoint;
        double ts; // Cycle time (seconds)

        /// <summary>
        /// A basic PI controller
        /// Used in this project to control pressure
        /// Works in inverted direction
        /// </summary>
        /// <param name="kp">Proportional gain</param>
        /// <param name="ki">Integral gain</param>
        /// <param name="tsMs">Cycle time (milliseconds)</param>
        public PIController(double kp, double ki, int tsMs)
        {
            Kp = kp;
            Ki = ki;
            ts = tsMs / 1000.0; // milliseconds -> seconds
            integrator = 0;
            setpoint = 0;
        }

        /// <summary>
        /// Set a new set point for the controller
        /// </summary>
        /// <param name="newSetpoint">Set point to set</param>
        public void setSetpoint(double newSetpoint)
        {
            setpoint = newSetpoint;
        }

        /// <summary>
        /// Update output of the controller
        /// </summary>
        /// <param name="measuredValue">Measurement value for the quantity</param>
        /// <returns>The result/output of the controller</returns>
        public double updateOutput(double measuredValue)
        {
            double result;
            double error;

            error = -1*(setpoint - measuredValue); // Inverted direction control

            integrator += (Ki * error * ts);

            result = Kp * error + integrator;

            return result;
        }
    }
}
