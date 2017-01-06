using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AppliedMotion.Stepper
{
    public class StepperController
    {
        public string IpAddress { get; private set; }
        private UdpClient _udpClient;
        bool _waitingForResponse = false;

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
            //Console.WriteLine("TX: " + command);
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
            //Console.WriteLine("Available: " + _udpClient.Available.ToString());

            //Console.WriteLine(_udpClient.Available.ToString() + " bytes available");
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 7777);

            Byte[] receiveBytes = _udpClient.Receive(ref remoteIpEndPoint);
            _waitingForResponse = false;
            Byte[] sclString = new byte[receiveBytes.Length - 2];

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
            Stopwatch swConflictTimeout = new Stopwatch();
            swConflictTimeout.Start();
            while (_waitingForResponse && swConflictTimeout.ElapsedMilliseconds < 5000)
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
                    // Console.WriteLine("RX: " + responseText);
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
            SendSclCommandAndGetResponse("JS" + System.Math.Round(speed, 2).ToString());
            SendSclCommandAndGetResponse("JA" + System.Math.Round(acceleration, 2).ToString());
            SendSclCommandAndGetResponse("JL" + System.Math.Round(deceleration, 2).ToString());
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
            SendSclCommandAndGetResponse("VE" + System.Math.Round(revsPerSec, 3).ToString());
        }

        public void ChangeJogSpeed(double speed)
        {
            SendSclCommandAndGetResponse("CS" + System.Math.Round(speed, 2).ToString());
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
            SendSclCommandAndGetResponse("EP" + newValue.ToString());
            SendSclCommandAndGetResponse("SP" + newValue.ToString());
        }

        public void MoveRelativeSteps(long steps)
        {
            SendSclCommandAndGetResponse("DI" + steps.ToString());
            SendSclCommandAndGetResponse("FL");
            WaitForStop();
        }

        public void MoveToAbsolutePosition(long position)
        {
            SendSclCommandAndGetResponse("DI" + position.ToString());
            SendSclCommandAndGetResponse("FP");
            WaitForStop();
        }

        internal void WaitForStop()
        {
            while (!GetStatus().InPosition)
            {
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
