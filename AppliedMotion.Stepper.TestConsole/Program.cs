using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppliedMotion.Stepper.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            StepperController sc = new StepperController("10.0.133.190");

            //for (int i = 0; i < 10; i++)
            //{
            //    SendScript(sc);
            //    System.Threading.Thread.Sleep(2000);
            //}

            //System.Threading.ThreadPool.QueueUserWorkItem((obj) =>
            //{
            //    while (true)
            //    {
            //        Console.WriteLine("Status: " + sc.GetStatus());
            //        Console.WriteLine("Alarm: " + sc.GetAlarmCode());
            //        System.Threading.Thread.Sleep(1000);
            //    }
            //});

            Console.WriteLine("Model: " + sc.GetModel());
            Console.WriteLine("Status: " + sc.GetStatus());
            sc.DisableMotor();
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine("Current Position: " + sc.GetEncoderPosition());
            Console.WriteLine("Resetting position to 0");
            sc.ResetEncoderPosition(0);
            Console.WriteLine("Current Position: " + sc.GetEncoderPosition());
            sc.EnableMotor();
            Console.WriteLine("Moving...");
            sc.MoveToAbsolutePosition(20000);
            Console.WriteLine("Move complete.");
            System.Threading.Thread.Sleep(2000);
            sc.StartJog(10, 25, 25);
            System.Threading.Thread.Sleep(2500);
            sc.ChangeJogSpeed(25);
            System.Threading.Thread.Sleep(2500);
            sc.ChangeJogSpeed(3);
            System.Threading.Thread.Sleep(2500);
            sc.StopJog();
            
            Console.WriteLine("Status: " + sc.GetStatus());
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine("Current Position: " + sc.GetEncoderPosition());

            Console.ReadLine();
        }
    }
}
