using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace R2D2PI
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Start();
        }

        public void Start()
        {
            R2D2Connection con1 = new R2D2Connection(R2D2Connection.ConnectionType.Controller, OnReceived);
            R2D2Connection con2 = new R2D2Connection(R2D2Connection.ConnectionType.R2D2, OnReceive);
            new Thread(() =>
            {
                con2.Connect();
                while (true)
                {
                    con2.SendCommand(new R2D2Connection.Command(R2D2Connection.Commands.SetLeftDriveMotor, BitConverter.GetBytes(5f)), false);
                    con2.SendCommand(new R2D2Connection.Command(R2D2Connection.Commands.SetRightDriveMotor, BitConverter.GetBytes(3f)), false);
                    Thread.Sleep(10);
                }

            }).Start();
            Thread.Sleep(1000);
            con1.Connect();
            int x = 0;
            while (true)
            {
                x++;
                con1.SendCommand(new R2D2Connection.Command(R2D2Connection.Commands.SetLeftDriveMotor, BitConverter.GetBytes((float)x)), false);
                con1.SendCommand(new R2D2Connection.Command(R2D2Connection.Commands.SetRightDriveMotor, BitConverter.GetBytes(3f)), false);
                Thread.Sleep(10);
                
            }
        }

        public void OnReceive(R2D2Connection.Command c)
        {
            Console.WriteLine("R2D2: Command recieved "+ (byte)c.commandID+" value " +BitConverter.ToSingle(c.param,0));
        }
        public void OnReceived(R2D2Connection.Command c)
        {
            Console.WriteLine("Cont: Command recieved " + (byte)c.commandID + " value " + BitConverter.ToSingle(c.param, 0));
        }
    }
}
