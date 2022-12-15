using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minipanosprosessi
{
    class PIController
    {
        double Kp; // Proportional gain 
        double Ki; // Integral gain
        double integrator;
        double setpoint;
        double ts; // Cycle time (seconds)

        public PIController(double kp, double ki, int tsMs)
        {
            Kp = kp;
            Ki = ki;
            ts = tsMs / 1000.0; // milliseconds -> seconds
            integrator = 0;
            setpoint = 0;
        }

        public void setSetpoint(double newSetpoint)
        {
            setpoint = newSetpoint;
        }

        public double updateOutput(double measuredValue)
        {
            double result;
            double error;

            error = -1*(setpoint - measuredValue);

            integrator += (Ki * error * ts);

            result = Kp * error + integrator;

            return result;
        }
    }
}
