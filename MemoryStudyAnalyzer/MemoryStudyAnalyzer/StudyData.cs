using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqStatistics;

namespace MemoryStudyAnalyzer
{
    class StudyData
    {

        // members

        public LinkedList<EnergyDataRow> energyData { get; set; }

        public Dictionary<string, double[]> energyDataAsColumns { get; set; }

        public LinkedList<double> squaredEMGList { get; set; }

        public LinkedList<double> squaredACCList { get; set; }

        public LinkedList<double> squaredGyroList { get; set; }

        public LinkedList<double> normalEMGList { get; set; }

        public LinkedList<double> normalACCList { get; set; }

        public LinkedList<double> normalGyroList { get; set; }


        public int participantID { get; set; }

        public string inputType { get; set; }

        public string itemSet { get; set; }

        public int runs { get; set; }

        public string gender { get; set; }

        public string screenCondition { get; set; }

        public float time { get; set; }



        public StudyData()
        {
            squaredEMGList = new LinkedList<double>();
            squaredACCList = new LinkedList<double>();
            squaredGyroList = new LinkedList<double>();
            normalEMGList = new LinkedList<double>();
            normalACCList = new LinkedList<double>();
            normalGyroList = new LinkedList<double>();

            energyData = new LinkedList<EnergyDataRow>();

            energyDataAsColumns = new Dictionary<string, double[]>();
        }

        // TODO column as enumerate or something
        public double[] getColumnData(string column) {
            double[] res = null;

            if (energyData.Count > 1) {
                res = new double[energyData.Count];

                if (energyDataAsColumns.ContainsKey(column))
                {
                    energyDataAsColumns.TryGetValue(column, out res);
                }
                else {
                    Console.WriteLine("could not get dat for: " + column);
                }
                    



            }


            return res;


        }

        public string getMean4MyoDataAsCSVString(string type) {

            StringBuilder sb = new StringBuilder();

            foreach (string key in energyDataAsColumns.Keys) {

                if(key.Contains(type)) {

                double[] current = getColumnData(key);
                sb.Append(current.Average());
                sb.Append(";");
                }
            }

            sb.Remove(sb.Length - 1, 1);


            return sb.ToString();


        }

        public string dataToCSVString() {
            StringBuilder sb = new StringBuilder();

            // row by row 
            // raw energy + squared + sum of emg / etc ?

            // statistics for clumns?

            return sb.ToString();
        }



        public void computeColumnData() {
            if (energyData.Count > 1)
            {
                double[] emg0 = new double[energyData.Count];
                double[] emg1 = new double[energyData.Count];
                double[] emg2 = new double[energyData.Count];
                double[] emg3 = new double[energyData.Count];
                double[] emg4 = new double[energyData.Count];
                double[] emg5 = new double[energyData.Count];
                double[] emg6 = new double[energyData.Count];
                double[] emg7 = new double[energyData.Count];

                double[] accX = new double[energyData.Count];
                double[] accY = new double[energyData.Count];
                double[] accZ = new double[energyData.Count];

                double[] gyroX = new double[energyData.Count];
                double[] gyroY = new double[energyData.Count];
                double[] gyroZ = new double[energyData.Count];

                //for(int i = )
                int rowCount = 0;
                foreach (EnergyDataRow row in energyData) {

                    emg0[rowCount] = row.emgEnergy_0;
                    emg1[rowCount] = row.emgEnergy_1;
                    emg2[rowCount] = row.emgEnergy_2;
                    emg3[rowCount] = row.emgEnergy_3;
                    emg4[rowCount] = row.emgEnergy_4;
                    emg5[rowCount] = row.emgEnergy_5;
                    emg6[rowCount] = row.emgEnergy_6;
                    emg7[rowCount] = row.emgEnergy_7;

                    accX[rowCount] = row.accEnergy_x;
                    accY[rowCount] = row.accEnergy_y;
                    accZ[rowCount] = row.accEnergy_z;

                    gyroX[rowCount] = row.gyroEnergy_x;
                    gyroY[rowCount] = row.gyroEnergy_y;
                    gyroZ[rowCount] = row.gyroEnergy_z;


                    rowCount++;
                }

                energyDataAsColumns.Add("emg0", emg0);
                energyDataAsColumns.Add("emg1", emg1);
                energyDataAsColumns.Add("emg2", emg2);
                energyDataAsColumns.Add("emg3", emg3);
                energyDataAsColumns.Add("emg4", emg4);
                energyDataAsColumns.Add("emg5", emg5);
                energyDataAsColumns.Add("emg6", emg6);
                energyDataAsColumns.Add("emg7", emg7);

                energyDataAsColumns.Add("accX", accX);
                energyDataAsColumns.Add("accY", accY);
                energyDataAsColumns.Add("accZ", accZ);

                energyDataAsColumns.Add("gyroX", gyroX);
                energyDataAsColumns.Add("gyroY", gyroY);
                energyDataAsColumns.Add("gyroZ", gyroZ);

            }

        }


    }
}
