using System;

namespace AppliedMotion.Stepper.TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            StepperController sc = new StepperController("10.10.10.10");
            try
            {
                // stop the drive from moving
                sc.Stop();
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
                //System.Threading.Thread.Sleep(2000);
                //Console.WriteLine("Current Position: " + sc.GetEncoderPosition());
                //Console.WriteLine("Resetting position to 0");
                //sc.ResetEncoderPosition(0);
                //Console.WriteLine("Current Position: " + sc.GetEncoderPosition());
                //sc.EnableMotor();
                //Console.WriteLine("Moving...");
                //sc.MoveToAbsolutePosition(20000);
                //Console.WriteLine("Move complete.");
                //System.Threading.Thread.Sleep(2000);
                //sc.StartJog(10, 25, 25);
                //System.Threading.Thread.Sleep(2500);
                //sc.ChangeJogSpeed(25);
                //System.Threading.Thread.Sleep(2500);
                //sc.ChangeJogSpeed(3);
                //System.Threading.Thread.Sleep(2500);
                //sc.StopJog();

                //Console.WriteLine("Status: " + sc.GetStatus());
                //System.Threading.Thread.Sleep(1000);
                //Console.WriteLine("Current Position: " + sc.GetEncoderPosition());

                // stop the drive from moving
                sc.Stop();

                // set the number of steps per rev
                sc.SetNumberStepsPerRevolution(51200);

                // set revolutions per second
                sc.SetVelocity(25);

                Console.WriteLine($"Model: {sc.GetModel()}");
                Console.WriteLine($"Status: {sc.GetStatus()} Position: {sc.GetEncoderPosition()} ");

                Console.WriteLine("Resetting position to 0");
                sc.ResetEncoderPosition(0);
                sc.EnableMotor();

                string stopText = string.Empty;
                // read 15 positions
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("read 15 positions. 1 revolution = 51200 steps " +
                                  "\nmin number: -2147483648" +
                                  "\nmax number: 2147483647");
                int maxPositions = 15;
                for (int i = 0; i < maxPositions; i++)
                {
                    try
                    {
                        Console.WriteLine($"Enter a position #{i} of {maxPositions}");
                        sc.EnableMotor();
                        var position = (long)Convert.ToDouble(Console.ReadLine());
                        sc.MoveToAbsolutePosition(position);
                        Console.WriteLine($"Current Position: {sc.GetEncoderPosition()}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                sc.Stop();
                sc.DisableMotor();
            }
        }
    }
}