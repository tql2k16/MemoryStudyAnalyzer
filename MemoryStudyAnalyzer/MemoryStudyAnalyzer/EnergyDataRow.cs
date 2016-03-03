using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryStudyAnalyzer
{
    class EnergyDataRow
    {

        public static int EMG_SENSOR_COUNT = 8;
        public static int ACC_DATA_COUNT = 3;
        public static int GYRO_DATA_COUNT = 3;


        public int rowNumber { get; set; }

        public double timeStamp { get; set; }

        public double emgEnergy_0 { get; set; }
        public double emgEnergy_1 { get; set; }
        public double emgEnergy_2 { get; set; }
        public double emgEnergy_3 { get; set; }
        public double emgEnergy_4 { get; set; }
        public double emgEnergy_5 { get; set; }
        public double emgEnergy_6 { get; set; }
        public double emgEnergy_7 { get; set; }



        public double accEnergy_x { get; set; }
        public double accEnergy_y { get; set; }
        public double accEnergy_z { get; set; }

        public double gyroEnergy_x { get; set; }
        public double gyroEnergy_y { get; set; }
        public double gyroEnergy_z { get; set; }



        
        public double getSquaredAverageGyro()
        {
            double res = gyroEnergy_x * gyroEnergy_x + gyroEnergy_y * gyroEnergy_y + gyroEnergy_z * gyroEnergy_z;
            res /= 3;

            return res;
        }

       
        public double getSquaredAverageAccelerometer()
        {
            double res = accEnergy_x * accEnergy_x + accEnergy_y * accEnergy_y + accEnergy_z * accEnergy_z;
            res /= 3;

            return res;
        }

        public double getSquaredAverageEMG()
        {
            double res = emgEnergy_0 * emgEnergy_0 + emgEnergy_1 * emgEnergy_1 + emgEnergy_2 * emgEnergy_2 + emgEnergy_3 * emgEnergy_3 + emgEnergy_4 * emgEnergy_4 + +emgEnergy_5 * emgEnergy_5 + emgEnergy_6 * emgEnergy_6 + emgEnergy_7 * emgEnergy_7;

            res /= 8;

            return res;

        }

        public double getAverageEMG()
        {
            double res = emgEnergy_0 + emgEnergy_1 + emgEnergy_2 + emgEnergy_3 + emgEnergy_4 + emgEnergy_5 + emgEnergy_6 + emgEnergy_7;

            res /= 8;

            return res;
        }

        public double getAverageGyro()
        {
            double res = gyroEnergy_x + gyroEnergy_y + gyroEnergy_z;
            res /= 3;

            return res;
        }

        public double getAverageAccelerometer()
        {
            double res = accEnergy_x + accEnergy_y + accEnergy_z;
            res /= 3;

            return res;
        }





    }
}
