using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AppliedMotion.Stepper
{
    public class StepperController
    {
        public string IpAddress { get; private set; }
        private UdpClient _udpClient;
        private bool _waitingForResponse = false;

        public StepperController(string ipAddress)
        {
            IpAddress = ipAddress;
            _udpClient = new UdpClient(7777);
            _udpClient.AllowNatTraversal(true);
            //_udpClient.ExclusiveAddressUse = false;
            _udpClient.Connect(IpAddress, 7775);
        }

        private void SendSclCommand(string command)
        {
            byte[] sclString = Encoding.ASCII.GetBytes(command);
            byte[] sendBytes = new byte[sclString.Length + 3];
            sendBytes[0] = 0;
            sendBytes[1] = 7;
            System.Array.Copy(sclString, 0, sendBytes, 2, sclString.Length);
            sendBytes[sendBytes.Length - 1] = 13;
            _udpClient.Send(sendBytes, sendBytes.Length);
            Debug.Print($"TX: {command}");
        }

        private string GetResponse()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 1000)
            {
                if (_udpClient.Available > 0) { break; }
            }
            if (_udpClient.Available == 0)
            {
                _waitingForResponse = false;
                return null;
            }

            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 7777);
            byte[] receiveBytes = _udpClient.Receive(ref remoteIpEndPoint);
            _waitingForResponse = false;
            byte[] sclString = new byte[receiveBytes.Length - 2];

            for (int i = 0; i < sclString.Length; i++)
            {
                sclString[i] = receiveBytes[i + 2];
            }
            return Encoding.ASCII.GetString(sclString);
        }

        public string SendSclCommandAndGetResponse(string command)
        {
            return SendSclCommandAndGetResponse(command, TimeSpan.FromSeconds(1));
        }

        public string SendSclCommandAndGetResponse(string command, TimeSpan timeout)
        {
            int repsonseTimeout = 5000;
            Stopwatch swConflictTimeout = new Stopwatch();
            swConflictTimeout.Start();
            while (_waitingForResponse && swConflictTimeout.ElapsedMilliseconds < repsonseTimeout)
            {
                System.Threading.Thread.Sleep(10);
            }

            _waitingForResponse = true;
            SendSclCommand(command);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.Elapsed < timeout)
            {
                var responseText = GetResponse();
                if (!string.IsNullOrWhiteSpace(responseText))
                {
                    Debug.Print($"RX: {responseText} @ {DateTime.Now.ToString("hh:mm:ss")}");
                    return responseText.Trim();
                }
                System.Threading.Thread.Sleep(10);
            }

            return null;
        }

        public void EnableMotor()
        {
            SendSclCommandAndGetResponse("ME");
        }

        public void SetNumberStepsPerRevolution(double numberSteps)
        {
            numberSteps = Math.Round(numberSteps, 2);
            if (numberSteps <= 51200 && numberSteps >= 200)
            {
                SendSclCommandAndGetResponse($"EG{numberSteps}");
            }
            else
            {
                Console.WriteLine($"setting default because {numberSteps} out of bounds");
                SendSclCommandAndGetResponse($"EG20000");
            }
        }

        public void DisableMotor()
        {
            SendSclCommandAndGetResponse("MD");
        }

        public string GetModel()
        {
            return SendSclCommandAndGetResponse("MV");
        }

        public void StartJog(double speed, double acceleration, double deceleration)
        {
            SendSclCommandAndGetResponse($"JS{Math.Round(speed, 2)}");
            SendSclCommandAndGetResponse($"JA{Math.Round(acceleration, 2)}");
            SendSclCommandAndGetResponse($"JL{Math.Round(deceleration, 2)}");
            SendSclCommandAndGetResponse("JM1");
            SendSclCommandAndGetResponse("CJ");
        }

        public void StopJog()
        {
            SendSclCommandAndGetResponse("SJ");
        }

        public void Stop()
        {
            SendSclCommandAndGetResponse("ST");
        }

        public void SetVelocity(double revsPerSec)
        {
            SendSclCommandAndGetResponse($"VE{System.Math.Round(revsPerSec, 3)}");
        }

        public void ChangeJogSpeed(double speed)
        {
            SendSclCommandAndGetResponse($"CS{System.Math.Round(speed, 2)}");
        }

        public MotorStatus GetStatus()
        {
            var response = SendSclCommandAndGetResponse("SC");
            if (response.StartsWith("SC="))
            {
                response = response.Substring(3).Trim();
                int responseCode = Convert.ToInt32(response);
                BitArray bA = new BitArray(System.BitConverter.GetBytes(responseCode));
                return new Stepper.MotorStatus(bA);
            }

            throw new Exception("Invalid status code response received: " + response);
        }

        public AlarmCode GetAlarmCode()
        {
            var response = SendSclCommandAndGetResponse("AL");
            if (response.StartsWith("AL="))
            {
                response = response.Substring(3).Trim();
                int responseCode = Convert.ToInt32(response);
                BitArray bA = new BitArray(System.BitConverter.GetBytes(responseCode));
                return new Stepper.AlarmCode(bA);
            }

            throw new Exception("Invalid status code response received: " + response);
        }

        public long GetEncoderPosition()
        {
            var response = SendSclCommandAndGetResponse("SP");
            if (response.StartsWith("SP="))
            {
                response = response.Substring(3).Trim();
                long encoderPosition = long.Parse(response);
                return encoderPosition;
            }
            else if (response == "*")
            {
                throw new Exception("Invalid status for encoder position query");
            }

            throw new Exception("Unexpected encoder position response: " + response);
        }

        public void ResetEncoderPosition(long newValue)
        {
            SendSclCommandAndGetResponse($"EP{newValue}");
            SendSclCommandAndGetResponse($"SP{newValue}");
        }

        public void MoveRelativeSteps(long steps)
        {
            SendSclCommandAndGetResponse($"DI{steps}");
            SendSclCommandAndGetResponse($"FL");
            WaitForStop();
        }

        public void MoveToAbsolutePosition(long position)
        {
            SendSclCommandAndGetResponse($"DI{position}");
            SendSclCommandAndGetResponse($"FP");
            WaitForStop();
        }

        public long GetEncoderCounts()
        {
            SendSclCommand("IFD");
            var response = SendSclCommandAndGetResponse($"IE");

            try
            {
                if (response.StartsWith("IE="))
                {
                    response = response.Substring(3).Trim();
                    long encoderPosition = long.Parse(response);
                    return encoderPosition;
                }
                else if (response == "*")
                {
                    throw new Exception("Invalid status for encoder position query");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            throw new Exception("Invalid status for encoder position query");
        }

        internal void WaitForStop()
        {
            try
            {
                while (GetStatus().Moving.ToString() != "False")
                {
                    Debug.Print(GetAlarmCode().ToString());
                    System.Threading.Thread.Sleep(00);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Debug.Print(e.Message);
            }
        }
    }
}