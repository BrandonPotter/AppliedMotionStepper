using System.Collections;

namespace AppliedMotion.Stepper
{
    public class MotorStatus
    {
        internal MotorStatus(BitArray bitStatus)
        {
            MotorEnabled = bitStatus[0];
            Sampling = bitStatus[1];
            DriveFault = bitStatus[2];
            InPosition = bitStatus[3];
            Moving = bitStatus[4];
            Jogging = bitStatus[5];
            Stopping = bitStatus[6];
            Waiting = bitStatus[7];
            Saving = bitStatus[8];
            Alarm = bitStatus[9];
            Homing = bitStatus[10];
            WaitOnTimer = bitStatus[11];
            WizardRunning = bitStatus[12];
            CheckingEncoder = bitStatus[13];
            QProgramRunning = bitStatus[14];
            Initializing = bitStatus[15];
        }

        public bool MotorEnabled { get; set; }
        public bool Sampling { get; set; }
        public bool DriveFault { get; set; }
        public bool InPosition { get; set; }
        public bool Moving { get; set; }
        public bool Jogging { get; set; }
        public bool Stopping { get; set; }
        public bool Waiting { get; set; }
        public bool Saving { get; set; }
        public bool Alarm { get; set; }
        public bool Homing { get; set; }
        public bool WaitOnTimer { get; set; }
        public bool WizardRunning { get; set; }
        public bool CheckingEncoder { get; set; }
        public bool QProgramRunning { get; set; }
        public bool Initializing { get; set; }

        public override string ToString()
        {
            return string.Join(", ", Utility.Reflection.ReflectTrueBoolPropertiesToList<MotorStatus>(this));
        }
    }
}
