using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppliedMotion.Stepper
{
    public class AlarmCode
    {
        internal AlarmCode(BitArray bitStatus)
        {
            PositionLimit = bitStatus[0];
            CCWLimit = bitStatus[1];
            CWLimit = bitStatus[2];
            OverTemperature = bitStatus[3];
            ExcessRegenInternalVoltage = bitStatus[4];
            OverVoltage = bitStatus[5];
            UnderVoltage = bitStatus[6];
            OverCurrent = bitStatus[7];
            BadHallSensorOpenMotorWinding = bitStatus[8];
            BadEncoder = bitStatus[9];
            CommError = bitStatus[10];
            BadFlash = bitStatus[11];
            WizardFailedNoMove = bitStatus[12];
            CurrentFoldbackMotorResistance = bitStatus[13];
            BlankQSegment = bitStatus[14];
            NoMove = bitStatus[15];
        }

        public bool NoMove { get; set; }

        public bool BlankQSegment { get; set; }

        public bool CurrentFoldbackMotorResistance { get; set; }

        public bool WizardFailedNoMove { get; set; }

        public bool BadFlash { get; set; }

        public bool CommError { get; set; }

        public bool BadEncoder { get; set; }

        public bool BadHallSensorOpenMotorWinding { get; set; }

        public bool OverCurrent { get; set; }

        public bool UnderVoltage { get; set; }

        public bool OverVoltage { get; set; }

        public bool ExcessRegenInternalVoltage { get; set; }

        public bool OverTemperature { get; set; }

        public bool CWLimit { get; set; }

        public bool CCWLimit { get; set; }

        public bool PositionLimit { get; set; }

        public override string ToString()
        {
            return string.Join(", ", Utility.Reflection.ReflectTrueBoolPropertiesToList<AlarmCode>(this));
        }
    }
}
