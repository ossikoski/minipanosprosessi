using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minipanosprosessi
{
    class PIController
    {
        double Kp;
        double Ki;
        double integrator;
        double setpoint;
        double Ts;

        public PIController(double kp, double ki, double ts)
        {
            Kp = kp;
            Ki = ki;
            Ts = ts;
            integrator = 0;
            setpoint = 0;
        }

        public void setSetpoint(double newSetpoint)
        {
            setpoint = newSetpoint;
        }

        public double updateOutput(double measuredValue)
        {
            double result = 0;
            double error = 0;

            error = setpoint - measuredValue;

            integrator += (Ki * error * Ts);

            result = Kp * error + integrator;

            return result;
        }
    }
}
