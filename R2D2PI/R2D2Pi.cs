using System;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;

namespace R2D2PI
{

    class R2D2Pi
    {
        SerialPort sabertooth;
        R2D2Connection connection;
        private float leftMotorTarget, rightMotorTarget, leftMotor, rightMotor;
        private Stopwatch timer;

        static void Main(string[] args)
        {
            R2D2Pi p = new R2D2Pi();
            p.Start();
        }

        public R2D2Pi()
        {
            timer = new Stopwatch();
            timer.Start();
            connection = new R2D2Connection(R2D2Connection.ConnectionType.R2D2, OnReceive);
            ConnectSabertooth();
        }

        private void ConnectSabertooth()
        {
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine(s);
            }
            sabertooth = new SerialPort("/dev/ttyS0", 9600, Parity.None, 8, StopBits.One);
            try
            {
                sabertooth.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message+" Retrying...");
                Thread.Sleep(1000);
                ConnectSabertooth();
                return;
            }

            byte[] buffer = new byte[1];
            buffer[0] = 0;
            WriteToSabertooth(buffer);
        }

        ~R2D2Pi()
        {
            if (sabertooth != null)
            {
                byte[] buffer = new byte[1];
                buffer[0] = 0;
                WriteToSabertooth(buffer);
            }
        }

        public void Start()
        {
            connection.Connect();
            while (true)
            {
                Loop();
            }
        }

        public void Loop()
        {
            if (timer.ElapsedMilliseconds>500)
            {
                leftMotorTarget = 0;
                rightMotorTarget = 0;
            }

            leftMotor = RampValue(leftMotor,leftMotorTarget,.05f);
            rightMotor = RampValue(rightMotor,rightMotorTarget,.05f);
            SetLeftMotor(leftMotor);
            SetRightMotor(rightMotor);
            Thread.Sleep(10);
        }

        public float RampValue(float startval, float endval, float interval)
        {
            if (endval-startval>0)
            {
                if (startval+interval>endval)
                {
                    return endval;
                }

                return startval + interval;
            }
            if (startval-interval<endval)
            {
                return endval;
            }

            return startval - interval;
        }


        public void OnReceive(R2D2Connection.Command c)
        {
            timer.Restart();
            switch (c.commandID)
            {
                case R2D2Connection.Commands.SetLeftDriveMotor:
                    leftMotorTarget = BitConverter.ToSingle(c.param, 0);
                    break;
                case R2D2Connection.Commands.SetRightDriveMotor:
                    rightMotorTarget = BitConverter.ToSingle(c.param, 0);
                    break;
                default:
                    break;
            }
        }

        private void SetLeftMotor(float value)
        {
            float motorPercent = value + 1;
            byte[] val = { (byte)(1 + motorPercent * 63) };
            WriteToSabertooth(val);
        }

        private void SetRightMotor(float value)
        {
            float motorPercent = value;
            byte[] val = new byte[1];
            // special case for weird resolution problem
            if (motorPercent>0)
            {
                val[0] = (byte)(192 + motorPercent * 63) ;
                
            }else if (motorPercent < 0)
            {
                val[0] = (byte)(192 + motorPercent * 64);
            }
            else
            {
                val[0] = 192;
            }

            WriteToSabertooth(val);
        }

        private void WriteToSabertooth(byte[] val)
        {
            try
            {
                sabertooth.Write(val, 0, 1);
            }catch(Exception e)
            {
                Console.WriteLine("Serial com error: "+e.Message);
                ConnectSabertooth();
            }

        }


    }
}
