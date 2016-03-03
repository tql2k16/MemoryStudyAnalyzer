using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqStatistics;
using System.IO;

namespace MemoryStudyAnalyzer
{
    class MemoryStudyAnalyzerConsoleApplication
    {
        static void Main(string[] args)
        {

            MemoryStudyAnalyzerConsoleApplication app = new MemoryStudyAnalyzerConsoleApplication();

            int option;


            while (true)
            {
                Console.Clear();
                Console.WriteLine("===================================================================");
                Console.WriteLine("Memory");
                Console.WriteLine("===================================================================");
                Console.WriteLine("Press '1' to load all available Energy Data");
                Console.WriteLine("Press '2' to create Summary Statistics of all participants");
                Console.WriteLine("Press '3' to create Summary Statistics for each single participant");
                Console.WriteLine("Press '4' to clean files in the ./CLEANING/ folder");
                Console.WriteLine("Press '9' to exit");

                Console.Write(">>");

                string input = Console.ReadLine();
                while (input == "")
                {
                    input = Console.ReadLine();
                }

                try
                {
                    option = int.Parse(input);
                }
                catch
                {
                    Console.WriteLine("Input invalid");
                    option = 0;
                }

                if (app.handleInputMainMenu(option) < 0)
                    break;

            }

        }

        public MemoryStudyAnalyzerConsoleApplication()
        {
            studyDataAllParticipants = new Dictionary<string, StudyData>();
        }


        public int handleInputMainMenu(int option)
        {

            int res = 1;

            if (option == 1) // load all data
            {
                //displayFiles();

                loadParticipantData();

            }
            else if (option == 2) // overall statistics
            {
                writeParticipantStatisticsv2();
            }
            else if (option == 3) // single participant statistics
            {
                writeParticipantStatisticsSingleColumn();
            }
            else if (option == 4) // cleaning
            {
                cleanFilesInFolder();
            }
            else if (option == 9) // single participant statistics
            {
                res = -1;
            }


            return res;
        }

        // keys: input type + participant id (e.g. Move_10, Touch_10, Touchpad_10)
        public Dictionary<string, StudyData> studyDataAllParticipants;



        private void loadParticipantData()
        {

            string[] files = Directory.GetDirectories("./Logs/Energy/");


            foreach (string s in files)
            {
                string[] files2 = Directory.GetFiles(s + "/Berechnet/");

                foreach (string s2 in files2)
                {
                    if (s2.Contains("_Energy.csv"))
                        readFile(s2);
                }

            }

            while (!Console.KeyAvailable) { }




        }






        private string checkFile(string newFile)
        {

            string path = newFile;
            bool res = false;
            if (!newFile.EndsWith(".csv"))
                path += ".csv";

            try
            {
                var reader = new StreamReader(File.OpenRead(path));
                res = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not open file:" + newFile);
                Console.WriteLine(e);
                path = "INVALID";
            }

            return path;
        }

        public void displayFiles()
        {
            string[] files = Directory.GetFiles("./Logs/");

            Dictionary<int, string> options = new Dictionary<int, string>();
            foreach (string f in files)
            {
                if (f.Contains(".csv"))
                {
                    options.Add(options.Count + 1, f);

                    Console.WriteLine("[" + options.Count + "]: " + f);

                }
            }

            string input = Console.ReadLine();
            int option;

            while (input == "")
            {
                input = Console.ReadLine();
            }

            try
            {
                option = int.Parse(input);
            }
            catch
            {
                Console.WriteLine("Input invalid");
                option = 0;
            }


            if (option > 0)
            {
                Console.WriteLine("Selected: " + files[option - 1]);


                // TODO
                //readFile(files[option - 1]);
                cleanLogFile(files[option - 1], 1);

            }

            while (!Console.KeyAvailable)
            {
            }

        }

        public void readFile(string path)
        {

            if (path == null || path == "")
            {
                Console.WriteLine("no path is specified");
                return;
            }

            StudyData currentData = new StudyData();

            bool readInfoFromRawDataFile = false;

            try
            {
                // try to get study data
                string path2 = path.Replace("_Energy", "");
                var reader2 = new StreamReader(File.OpenRead(path2));

                string[] description = reader2.ReadLine().Split('|');


                // Myo | Teilnehmer Nr: 10 | Eingabetyp: Move | Itemset A | Computername: WHITEBOARD-PC | Durchlaufe: 6;;;;;;;;;;;;;;;;;;
                int participantID = -1;
                Int32.TryParse(description[1].Split(':')[1].Replace(" ", ""), out participantID);
                currentData.participantID = participantID;
                currentData.inputType = description[2].Split(':')[1].Replace(" ", "");
                currentData.itemSet = description[3].Split(' ')[2];
                int runs = -1;
                Int32.TryParse(description[5].Split(' ')[2].Replace(";", ""), out runs);
                currentData.runs = runs;


                currentData.gender = determineGenderBasedOnId(participantID);
                currentData.screenCondition = determineScreenConditionBasedOnId(participantID);


                // header
                reader2.ReadLine();

                // time




                //bool parsed = DateTime.TryParseExact(firstLine, "hh:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start);
                string[] firstLine = reader2.ReadLine().Split(';')[0].Remove(0, 11).Split(':');



                string lastLine = "";
                while (!reader2.EndOfStream)
                {
                    lastLine = reader2.ReadLine();
                }

                string[] lastLineArray = lastLine.Split(';')[0].Remove(0, 11).Split(':');

                int h1, h2;
                Int32.TryParse(firstLine[0], out h1);
                Int32.TryParse(lastLineArray[0], out h2);

                int m1, m2;
                Int32.TryParse(firstLine[1], out m1);
                Int32.TryParse(lastLineArray[1], out m2);

                float s1, s2;

                float.TryParse(firstLine[2], out s1);
                float.TryParse(lastLineArray[2], out s2);

                //firstLine = firstLine[2].Split(',');
                //lastLineArray = lastLineArray[2].Split(',');

                //int m1, m2;
                //Int32.TryParse(firstLine[2], out m1);
                //Int32.TryParse(lastLineArray[2], out m2);

                float t1 = (h1 * 3600) + (m1 * 60) + s1;
                float t2 = (h2 * 3600) + (m2 * 60) + s2;

                float difference = t2 - t1;

                currentData.time = difference;

                readInfoFromRawDataFile = true;

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Could not open file:" + e.FileName);
            }


            try
            {


                // energy data
                var reader = new StreamReader(File.OpenRead(path));

                Console.WriteLine("Reading " + path);

                if (!readInfoFromRawDataFile)
                {
                    // get input type and participant id from file name

                    string[] name = path.Split('/')[2].Split('_');

                    int id = -1;
                    Int32.TryParse(name[1], out id);
                    currentData.participantID = id;
                    currentData.inputType = name[0];

                    currentData.gender = determineGenderBasedOnId(id);
                    currentData.screenCondition = determineScreenConditionBasedOnId(id);

                }



                int count = 1;
                // read one line for the header
                string l1 = reader.ReadLine();
                //string l2 = reader.ReadLine();




                while (!reader.EndOfStream && !Console.KeyAvailable)
                {
                    var line = reader.ReadLine();
                    string[] values = line.Split(';');

                    /* Time; Energy Result EmgSensor_0; Energy Result EmgSensor_1; Energy Result EmgSensor_2; Energy Result EmgSensor_3; Energy Result EmgSensor_4; Energy Result EmgSensor_5; Energy Result EmgSensor_6; Energy Result EmgSensor_7; 
                    Energy Result Accelerometer_x; Energy Result Accelerometer_y; Energy Result Accelerometer_z;
                    Energy Result Gyroscope_x; Energy Result Gyroscope_y; Energy Result Gyroscope_z
                    */

                    EnergyDataRow currentRow = new EnergyDataRow();
                    currentRow.rowNumber = count;
                    count++;


                    currentRow.timeStamp = Convert.ToDouble(values[0]);
                    currentRow.emgEnergy_0 = Convert.ToDouble(values[1]);
                    currentRow.emgEnergy_1 = Convert.ToDouble(values[2]);
                    currentRow.emgEnergy_2 = Convert.ToDouble(values[3]);
                    currentRow.emgEnergy_3 = Convert.ToDouble(values[4]);
                    currentRow.emgEnergy_4 = Convert.ToDouble(values[5]);
                    currentRow.emgEnergy_5 = Convert.ToDouble(values[6]);
                    currentRow.emgEnergy_6 = Convert.ToDouble(values[7]);
                    currentRow.emgEnergy_7 = Convert.ToDouble(values[8]);
                    currentRow.accEnergy_x = Convert.ToDouble(values[9]);
                    currentRow.accEnergy_y = Convert.ToDouble(values[10]);
                    currentRow.accEnergy_z = Convert.ToDouble(values[11]);
                    currentRow.gyroEnergy_x = Convert.ToDouble(values[12]);
                    currentRow.gyroEnergy_y = Convert.ToDouble(values[13]);
                    currentRow.gyroEnergy_z = Convert.ToDouble(values[14]);

                    currentData.squaredEMGList.AddLast(currentRow.getSquaredAverageEMG());
                    currentData.squaredACCList.AddLast(currentRow.getSquaredAverageAccelerometer());
                    currentData.squaredGyroList.AddLast(currentRow.getSquaredAverageGyro());

                    currentData.normalEMGList.AddLast(currentRow.getAverageEMG());
                    currentData.normalACCList.AddLast(currentRow.getAverageAccelerometer());
                    currentData.normalGyroList.AddLast(currentRow.getAverageGyro());

                    //Console.WriteLine("Reading CSV file ... " + currentRow);

                    currentData.energyData.AddLast(currentRow);

                }

                currentData.computeColumnData();

                //Console.WriteLine("EMG^2:" + currentData.squaredEMGList.Average());
                //Console.WriteLine("ACC^2:" + currentData.squaredACCList.Average());
                //Console.WriteLine("GYRO^2:" + currentData.squaredGyroList.Average());

                //Console.WriteLine("EMG:" + currentData.normalEMGList.Average());
                //Console.WriteLine("ACC:" + currentData.normalACCList.Average());
                //Console.WriteLine("GYRO:" + currentData.normalGyroList.Average());

                Dictionary<string, double[]> energyDataAsColumns = currentData.energyDataAsColumns;

                studyDataAllParticipants.Add(currentData.inputType + "_" + currentData.participantID, currentData);
                Console.WriteLine("Finished Reading " + path);


            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Could not open file:" + e.FileName);
            }
        }

        private string determineScreenConditionBasedOnId(int participantID)
        {
            if (participantID > 0 && participantID < 10 || participantID > 18 && participantID < 28)
                return "small";
            else if (participantID >= 10 && participantID <= 18 || participantID >= 28 && participantID <= 36)
                return "big";
            else
                return "error";
        }

        private string determineGenderBasedOnId(int participantID)
        {
            if (participantID > 0 && participantID < 19 || participantID == 36)
                return "female";
            else if (participantID > 18 && participantID < 36)
                return "male";
            else
                return "error";
        }

        public void cleanFilesInFolder()
        {
            int option = 0;

            Console.WriteLine("Press '1' to replace , with .");
            Console.WriteLine("Press '2' to replace . with ,");

            string input = Console.ReadLine();



            while (input == "")
            {
                input = Console.ReadLine();
            }

            try
            {
                option = int.Parse(input);
            }
            catch
            {
                Console.WriteLine("Input invalid");
                option = 0;
                while (!Console.KeyAvailable)
                {
                }
                return;
            }



            string[] files = Directory.GetFiles("./Logs/CLEANING");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Cleaning Report");

            Dictionary<int, string> options = new Dictionary<int, string>();
            foreach (string f in files)
            {
                if (f.Contains(".csv") && !f.Contains("[CLEAN]"))
                {
                    Console.WriteLine("Cleaning file: " + f);

                    sb.Append(cleanLogFile(f, option));
                }
            }

            TextWriter report = new StreamWriter("./Logs/CLEANING/cleaned/report.txt", false);
            report.Write(sb.ToString());
            report.Close();

            while (!Console.KeyAvailable)
            {
            }

        }

        public string cleanLogFile(string path, int option)
        {
            if (path == null || path == "")
            {
                Console.WriteLine("no path is specified");
                return "error";
            }

            StringBuilder sb = new StringBuilder();

            FileInfo f = new FileInfo(path);

            sb.AppendLine("----------");

            sb.AppendLine("Old file:" + path);

            string path2 = path.Replace(".csv", "[CLEAN].csv").Replace("/Logs/CLEANING", "/Logs/CLEANING/cleaned");
            //path2 = path2.Replace("/Logs/CLEANING/", "/Logs/CLEANING/cleaned/");
            sb.AppendLine("New file:" + path);

            checkDirectory("./Logs/CLEANING/cleaned");

            TextWriter cleanFile = new StreamWriter(path2, false);



            try
            {


                // myo log data
                var reader = new StreamReader(File.OpenRead(path));

                int count = 1;
                // read one line for the header
                string l1 = reader.ReadLine();
                cleanFile.WriteLine(l1);
                string l2 = reader.ReadLine();
                cleanFile.WriteLine(l2);
                while (!reader.EndOfStream && !Console.KeyAvailable)
                {
                    var line = reader.ReadLine();
                    string[] values = line.Split(';');

                    string test = values[0];
                    if (Char.IsWhiteSpace(values[0].ElementAt(values[0].Length - 1)))
                    {
                        test = test.Remove(values[0].Length - 1);
                        //test.Substring(0, values[0].Length - 2);
                    }
                    test.Replace("/", "."); // fix date
                    cleanFile.Write(test);
                    cleanFile.Write(";");
                    for (int i = 1; i < values.Length; i++)
                    {
                        string removeComata = "";
                        if (option == 1)
                            removeComata = values[i].Replace(",", ".");
                        else if (option == 2)
                            removeComata = values[i].Replace(".", ",");
                        else
                            removeComata = values[i].Replace(",", ".");

                        //string 

                        if (i < 9)
                        {
                            //removeComata.Replace(" ", "");
                            removeComata.Remove(0);
                            removeComata.Remove(removeComata.Length - 1);
                        }

                        cleanFile.Write(removeComata);

                        if (!(i + 1 == values.Length))
                        {
                            cleanFile.Write(";");
                        }

                    }
                    cleanFile.WriteLine();

                    count++;
                }

                reader.Close();
                cleanFile.Close();

                Console.WriteLine("Cleaned file: " + path);

                FileInfo f2 = new FileInfo(path2);

                Console.WriteLine();
                sb.AppendLine("Size Comparison (old, new) =  (" + f.Length + "," + f2.Length + ")");
                sb.AppendLine("Difference:" + (f.Length - f2.Length));
                sb.AppendLine("----------");


            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Could not open file:" + e.FileName);
                return "error";
            }

            return sb.ToString();
        }


        public void writeSingleParticipantStatistics()
        {

            string path = "./Logs/Results/";

            foreach (string key in studyDataAllParticipants.Keys)
            {

                StudyData d;
                studyDataAllParticipants.TryGetValue(key, out d);

                if (d != null)
                {

                    path = "./Logs/Results/";

                    path += d.inputType + "/";

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    path += key + ".csv";

                    TextWriter currentParticipantWriter = new StreamWriter(path, false);

                    // header


                    double[] asd = new double[1000];







                    //string movePath = path + ""
                    //    string touchPath

                }
            }

        }

        private void checkDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void writeParticipantStatisticsSingleColumn()
        {

            // TODO path should depend on ID
            string path = "./Logs/Results/";

            checkDirectory(path);

            string emgSummaryPath = path + "summaryEMG.csv";
            string accSummaryPath = path + "summaryACC.csv";
            string gyroSummaryPath = path + "summaryGYRO.csv";

            string[] pathArray = new string[] { emgSummaryPath, accSummaryPath, gyroSummaryPath };

            foreach (string p in pathArray)
            {
                TextWriter writer = new StreamWriter(p, false);
                writer.WriteLine("sep=;");
                writer.Write("Teilnehmer ID");
                writer.Write(";");
                writer.Write("Zeit(Sekunden)");
                writer.Write(";");
                writer.Write("Geschlecht");
                writer.Write(";");
                writer.Write("Screen Condition");
                writer.Write(";");
                writer.Write("Eingabe Typ");
                writer.Write(";");
                writer.Write("Itemset");
                writer.Write(";");


                if (p.Contains("EMG"))
                {
                    writer.Write("EMG_0 Mean");
                    writer.Write(";");
                    writer.Write("EMG_1 Mean");
                    writer.Write(";");
                    writer.Write("EMG_2 Mean");
                    writer.Write(";");
                    writer.Write("EMG_3 Mean");
                    writer.Write(";");
                    writer.Write("EMG_4 Mean");
                    writer.Write(";");
                    writer.Write("EMG_5 Mean");
                    writer.Write(";");
                    writer.Write("EMG_6 Mean");
                    writer.Write(";");
                    writer.Write("EMG_7 Mean");
                }
                else if (p.Contains("ACC"))
                {

                    writer.Write("ACC_x Mean");
                    writer.Write(";");
                    writer.Write("ACC_y Mean");
                    writer.Write(";");
                    writer.Write("ACC_z Mean");
                }
                else if (p.Contains("GYRO"))
                {
                    writer.Write("Gyro_x Mean");
                    writer.Write(";");
                    writer.Write("Gyro_y Mean");
                    writer.Write(";");
                    writer.Write("Gyro_z Mean");
                }


                writer.Close();
            }

            // each condition: All
            LinkedList<double> emg0All = new LinkedList<double>();
            LinkedList<double> emg1All = new LinkedList<double>();
            LinkedList<double> emg2All = new LinkedList<double>();
            LinkedList<double> emg3All = new LinkedList<double>();
            LinkedList<double> emg4All = new LinkedList<double>();
            LinkedList<double> emg5All = new LinkedList<double>();
            LinkedList<double> emg6All = new LinkedList<double>();
            LinkedList<double> emg7All = new LinkedList<double>();
            LinkedList<double> accXAll = new LinkedList<double>();
            LinkedList<double> accYAll = new LinkedList<double>();
            LinkedList<double> accZAll = new LinkedList<double>();
            LinkedList<double> gyroXAll = new LinkedList<double>();
            LinkedList<double> gyroYAll = new LinkedList<double>();
            LinkedList<double> gyroZAll = new LinkedList<double>();



            // each condition: move
            LinkedList<double> emg0Move = new LinkedList<double>();
            LinkedList<double> emg1Move = new LinkedList<double>();
            LinkedList<double> emg2Move = new LinkedList<double>();
            LinkedList<double> emg3Move = new LinkedList<double>();
            LinkedList<double> emg4Move = new LinkedList<double>();
            LinkedList<double> emg5Move = new LinkedList<double>();
            LinkedList<double> emg6Move = new LinkedList<double>();
            LinkedList<double> emg7Move = new LinkedList<double>();
            LinkedList<double> accXMove = new LinkedList<double>();
            LinkedList<double> accYMove = new LinkedList<double>();
            LinkedList<double> accZMove = new LinkedList<double>();
            LinkedList<double> gyroXMove = new LinkedList<double>();
            LinkedList<double> gyroYMove = new LinkedList<double>();
            LinkedList<double> gyroZMove = new LinkedList<double>();

            // each condition: Trackpad
            LinkedList<double> emg0Trackpad = new LinkedList<double>();
            LinkedList<double> emg1Trackpad = new LinkedList<double>();
            LinkedList<double> emg2Trackpad = new LinkedList<double>();
            LinkedList<double> emg3Trackpad = new LinkedList<double>();
            LinkedList<double> emg4Trackpad = new LinkedList<double>();
            LinkedList<double> emg5Trackpad = new LinkedList<double>();
            LinkedList<double> emg6Trackpad = new LinkedList<double>();
            LinkedList<double> emg7Trackpad = new LinkedList<double>();
            LinkedList<double> accXTrackpad = new LinkedList<double>();
            LinkedList<double> accYTrackpad = new LinkedList<double>();
            LinkedList<double> accZTrackpad = new LinkedList<double>();
            LinkedList<double> gyroXTrackpad = new LinkedList<double>();
            LinkedList<double> gyroYTrackpad = new LinkedList<double>();
            LinkedList<double> gyroZTrackpad = new LinkedList<double>();

            // each condition: Touch
            LinkedList<double> emg0Touch = new LinkedList<double>();
            LinkedList<double> emg1Touch = new LinkedList<double>();
            LinkedList<double> emg2Touch = new LinkedList<double>();
            LinkedList<double> emg3Touch = new LinkedList<double>();
            LinkedList<double> emg4Touch = new LinkedList<double>();
            LinkedList<double> emg5Touch = new LinkedList<double>();
            LinkedList<double> emg6Touch = new LinkedList<double>();
            LinkedList<double> emg7Touch = new LinkedList<double>();
            LinkedList<double> accXTouch = new LinkedList<double>();
            LinkedList<double> accYTouch = new LinkedList<double>();
            LinkedList<double> accZTouch = new LinkedList<double>();
            LinkedList<double> gyroXTouch = new LinkedList<double>();
            LinkedList<double> gyroYTouch = new LinkedList<double>();
            LinkedList<double> gyroZTouch = new LinkedList<double>();




            foreach (string key in studyDataAllParticipants.Keys)
            {

                StudyData d;
                studyDataAllParticipants.TryGetValue(key, out d);

                if (d != null)
                {

                    // TextWriter file = new StreamWriter(path + "Participant"+ d.participantID + "/" + key + ".csv", false);

                    foreach (string p in pathArray)
                    {
                        TextWriter summary = new StreamWriter(p, true);
                        summary.WriteLine("");
                        summary.Write(d.participantID);
                        summary.Write(";");
                        summary.Write(d.time);
                        summary.Write(";");
                        summary.Write(d.gender);
                        summary.Write(";");
                        summary.Write(d.screenCondition);
                        summary.Write(";");
                        summary.Write(d.inputType);
                        summary.Write(";");
                        summary.Write(d.itemSet);
                        summary.Write(";");

                        if (p.Contains("EMG"))
                        {
                            summary.Write(d.getMean4MyoDataAsCSVString("emg"));
                        }
                        else if (p.Contains("ACC"))
                        {
                            summary.Write(d.getMean4MyoDataAsCSVString("acc"));
                        }
                        else if (p.Contains("GYRO"))
                        {
                            summary.Write(d.getMean4MyoDataAsCSVString("gyro"));
                        }



                        summary.Close();
                    }

                    emg0All.AddLast(d.getColumnData("emg0").Average());
                    emg1All.AddLast(d.getColumnData("emg1").Average());
                    emg2All.AddLast(d.getColumnData("emg2").Average());
                    emg3All.AddLast(d.getColumnData("emg3").Average());
                    emg4All.AddLast(d.getColumnData("emg4").Average());
                    emg5All.AddLast(d.getColumnData("emg5").Average());
                    emg6All.AddLast(d.getColumnData("emg6").Average());
                    emg7All.AddLast(d.getColumnData("emg7").Average());
                    accXAll.AddLast(d.getColumnData("accX").Average());
                    accYAll.AddLast(d.getColumnData("accY").Average());
                    accZAll.AddLast(d.getColumnData("accZ").Average());
                    gyroXAll.AddLast(d.getColumnData("gyroX").Average());
                    gyroYAll.AddLast(d.getColumnData("gyroY").Average());
                    gyroZAll.AddLast(d.getColumnData("gyroZ").Average());

                    if (d.inputType.Equals("Move"))
                    {
                        emg0Move.AddLast(d.getColumnData("emg0").Average());
                        emg1Move.AddLast(d.getColumnData("emg1").Average());
                        emg2Move.AddLast(d.getColumnData("emg2").Average());
                        emg3Move.AddLast(d.getColumnData("emg3").Average());
                        emg4Move.AddLast(d.getColumnData("emg4").Average());
                        emg5Move.AddLast(d.getColumnData("emg5").Average());
                        emg6Move.AddLast(d.getColumnData("emg6").Average());
                        emg7Move.AddLast(d.getColumnData("emg7").Average());
                        accXMove.AddLast(d.getColumnData("accX").Average());
                        accYMove.AddLast(d.getColumnData("accY").Average());
                        accZMove.AddLast(d.getColumnData("accZ").Average());
                        gyroXMove.AddLast(d.getColumnData("gyroX").Average());
                        gyroYMove.AddLast(d.getColumnData("gyroY").Average());
                        gyroZMove.AddLast(d.getColumnData("gyroZ").Average());
                    }
                    else if (d.inputType.Equals("Touch"))
                    {
                        emg0Touch.AddLast(d.getColumnData("emg0").Average());
                        emg1Touch.AddLast(d.getColumnData("emg1").Average());
                        emg2Touch.AddLast(d.getColumnData("emg2").Average());
                        emg3Touch.AddLast(d.getColumnData("emg3").Average());
                        emg4Touch.AddLast(d.getColumnData("emg4").Average());
                        emg5Touch.AddLast(d.getColumnData("emg5").Average());
                        emg6Touch.AddLast(d.getColumnData("emg6").Average());
                        emg7Touch.AddLast(d.getColumnData("emg7").Average());
                        accXTouch.AddLast(d.getColumnData("accX").Average());
                        accYTouch.AddLast(d.getColumnData("accY").Average());
                        accZTouch.AddLast(d.getColumnData("accZ").Average());
                        gyroXTouch.AddLast(d.getColumnData("gyroX").Average());
                        gyroYTouch.AddLast(d.getColumnData("gyroY").Average());
                        gyroZTouch.AddLast(d.getColumnData("gyroZ").Average());
                    }
                    else if (d.inputType.Equals("Touchpad"))
                    {
                        emg0Trackpad.AddLast(d.getColumnData("emg0").Average());
                        emg1Trackpad.AddLast(d.getColumnData("emg1").Average());
                        emg2Trackpad.AddLast(d.getColumnData("emg2").Average());
                        emg3Trackpad.AddLast(d.getColumnData("emg3").Average());
                        emg4Trackpad.AddLast(d.getColumnData("emg4").Average());
                        emg5Trackpad.AddLast(d.getColumnData("emg5").Average());
                        emg6Trackpad.AddLast(d.getColumnData("emg6").Average());
                        emg7Trackpad.AddLast(d.getColumnData("emg7").Average());
                        accXTrackpad.AddLast(d.getColumnData("accX").Average());
                        accYTrackpad.AddLast(d.getColumnData("accY").Average());
                        accZTrackpad.AddLast(d.getColumnData("accZ").Average());
                        gyroXTrackpad.AddLast(d.getColumnData("gyroX").Average());
                        gyroYTrackpad.AddLast(d.getColumnData("gyroY").Average());
                        gyroZTrackpad.AddLast(d.getColumnData("gyroZ").Average());
                    }

                }

            }



            // order by input type

            foreach (string p in pathArray)
            {
                TextWriter summary = new StreamWriter(p, true);
                summary.WriteLine();
                summary.WriteLine();

                summary.WriteLine("Move");

                if (p.Contains("EMG"))
                {
                    summary.Write(getDescriptiveStatisticsString(emg0Move, "EMG_0", emg0All.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(emg1Move, "EMG_1", emg1All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg2Move, "EMG_2", emg2All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg3Move, "EMG_3", emg3All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg4Move, "EMG_4", emg4All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg5Move, "EMG_5", emg5All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg6Move, "EMG_6", emg6All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg7Move, "EMG_7", emg7All.Average(), false));
                }
                else if (p.Contains("ACC"))
                {
                    summary.Write(getDescriptiveStatisticsString(accXMove, "ACC_X", accXAll.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(accYMove, "ACC_Y", accYAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(accZMove, "ACC_Z", accZAll.Average(), false));
                }
                else if (p.Contains("GYRO"))
                {
                    summary.Write(getDescriptiveStatisticsString(gyroXMove, "GYRO_X", gyroXAll.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(gyroYMove, "GYRO_Y", gyroYAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(gyroZMove, "GYRO_Z", gyroZAll.Average(), false));
                }
                summary.WriteLine();


                summary.WriteLine("Touch");

                if (p.Contains("EMG"))
                {
                    summary.Write(getDescriptiveStatisticsString(emg0Touch, "EMG_0", emg0All.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(emg1Touch, "EMG_1", emg1All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg2Touch, "EMG_2", emg2All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg3Touch, "EMG_3", emg3All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg4Touch, "EMG_4", emg4All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg5Touch, "EMG_5", emg5All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg6Touch, "EMG_6", emg6All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg7Touch, "EMG_7", emg7All.Average(), false));
                }
                else if (p.Contains("ACC"))
                {
                    summary.Write(getDescriptiveStatisticsString(accXTouch, "ACC_X", accXAll.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(accYTouch, "ACC_Y", accYAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(accZTouch, "ACC_Z", accZAll.Average(), false));
                }
                else if (p.Contains("GYRO"))
                {
                    summary.Write(getDescriptiveStatisticsString(gyroXTouch, "GYRO_X", gyroXAll.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(gyroYTouch, "GYRO_Y", gyroYAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(gyroZTouch, "GYRO_Z", gyroZAll.Average(), false));
                }
                summary.WriteLine();


                summary.WriteLine("Touchpad");

                if (p.Contains("EMG"))
                {
                    summary.Write(getDescriptiveStatisticsString(emg0Trackpad, "EMG_0", emg0All.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(emg1Trackpad, "EMG_1", emg1All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg2Trackpad, "EMG_2", emg2All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg3Trackpad, "EMG_3", emg3All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg4Trackpad, "EMG_4", emg4All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg5Trackpad, "EMG_5", emg5All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg6Trackpad, "EMG_6", emg6All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg7Trackpad, "EMG_7", emg7All.Average(), false));
                }
                else if (p.Contains("ACC"))
                {
                    summary.Write(getDescriptiveStatisticsString(accXTrackpad, "ACC_X", accXAll.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(accYTrackpad, "ACC_Y", accYAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(accZTrackpad, "ACC_Z", accZAll.Average(), false));
                }
                else if (p.Contains("GYRO"))
                {
                    summary.Write(getDescriptiveStatisticsString(gyroXTrackpad, "GYRO_X", gyroXAll.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(gyroYTrackpad, "GYRO_Y", gyroYAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(gyroZTrackpad, "GYRO_Z", gyroZAll.Average(), false));
                }
                summary.WriteLine();
                summary.Close();

            }

            // order by emg sensor or acc/gyro axis

            foreach (string p in pathArray)
            {
                TextWriter summary = new StreamWriter(p, true);
                summary.WriteLine();
                summary.WriteLine();



                if (p.Contains("EMG"))
                {
                    summary.WriteLine("EMG");

                    summary.Write(getDescriptiveStatisticsString(emg0Move, "EMG_0_Move", emg0All.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(emg0Touch, "EMG_0_Touch", emg0All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg0Trackpad, "EMG_0_Trackpad", emg0All.Average(), false));

                    summary.Write(getDescriptiveStatisticsString(emg1Move, "EMG_1_Move", emg1All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg1Touch, "EMG_1_Touch", emg1All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg1Trackpad, "EMG_1_Trackpad", emg1All.Average(), false));

                    summary.Write(getDescriptiveStatisticsString(emg2Move, "EMG_2_Move", emg2All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg2Touch, "EMG_2_Touch", emg2All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg2Trackpad, "EMG_2_Trackpad", emg2All.Average(), false));

                    summary.Write(getDescriptiveStatisticsString(emg3Move, "EMG_3_Move", emg3All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg3Touch, "EMG_3_Touch", emg3All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg3Trackpad, "EMG_3_Trackpad", emg3All.Average(), false));

                    summary.Write(getDescriptiveStatisticsString(emg4Move, "EMG_4_Move", emg4All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg4Touch, "EMG_4_Touch", emg4All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg4Trackpad, "EMG_4_Trackpad", emg4All.Average(), false));

                    summary.Write(getDescriptiveStatisticsString(emg5Move, "EMG_5_Move", emg5All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg5Touch, "EMG_5_Touch", emg5All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg5Trackpad, "EMG_5_Trackpad", emg5All.Average(), false));

                    summary.Write(getDescriptiveStatisticsString(emg6Move, "EMG_6_Move", emg6All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg6Touch, "EMG_6_Touch", emg6All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg6Trackpad, "EMG_6_Trackpad", emg6All.Average(), false));

                    summary.Write(getDescriptiveStatisticsString(emg7Move, "EMG_7_Move", emg7All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg7Touch, "EMG_7_Touch", emg7All.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(emg7Trackpad, "EMG_7_Trackpad", emg7All.Average(), false));



                }
                else if (p.Contains("ACC"))
                {
                    summary.WriteLine("Accelerometer");

                    summary.Write(getDescriptiveStatisticsString(accXMove, "ACC_X_Move", accXAll.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(accXTouch, "ACC_X_Touch", accXAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(accXTrackpad, "ACC_X_Trackpad", accXAll.Average(), false));

                    summary.Write(getDescriptiveStatisticsString(accYMove, "ACC_Y_Move", accYAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(accYTouch, "ACC_Y_Touch", accYAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(accYTrackpad, "ACC_Y_Trackpad", accYAll.Average(), false));

                    summary.Write(getDescriptiveStatisticsString(accZMove, "ACC_Z_Move", accZAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(accZTouch, "ACC_Z_Touch", accZAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(accZTrackpad, "ACC_Z_Trackpad", accZAll.Average(), false));

                }
                else if (p.Contains("GYRO"))
                {
                    summary.WriteLine("Gyroscope");

                    summary.Write(getDescriptiveStatisticsString(gyroXMove, "GYRO_X_Move", gyroXAll.Average(), true));
                    summary.Write(getDescriptiveStatisticsString(gyroXTouch, "GYRO_X_Touch", gyroXAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(gyroXTrackpad, "GYRO_X_Trackpad", gyroXAll.Average(), false));

                    summary.Write(getDescriptiveStatisticsString(gyroYMove, "GYRO_Y_Move", gyroYAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(gyroYTouch, "GYRO_Y_Touch", gyroYAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(gyroYTrackpad, "GYRO_Y_Trackpad", gyroYAll.Average(), false));


                    summary.Write(getDescriptiveStatisticsString(gyroZMove, "GYRO_Z_Move", gyroZAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(gyroZTouch, "GYRO_Z_Touch", gyroZAll.Average(), false));
                    summary.Write(getDescriptiveStatisticsString(gyroZTrackpad, "GYRO_Z_Trackpad", gyroZAll.Average(), false));

                }

                summary.WriteLine();
                summary.Close();

            }


        }

        public void writeParticipantStatistics()
        {

            // TODO path should depend on ID
            string path = "./Logs/Results/";

            checkDirectory(path);



            string pathToAllData = path + "participantSummary.csv";

            Console.WriteLine(File.Exists(pathToAllData) ? "File exists." : "File does not exist.");

            if (!File.Exists(pathToAllData))
            {
                // write summary header
                TextWriter summaryHeader = new StreamWriter(pathToAllData, false);

                summaryHeader.WriteLine("sep=;");
                summaryHeader.Write("Teilnehmer ID");
                summaryHeader.Write(";");
                summaryHeader.Write("Zeit(Sekunden)");
                summaryHeader.Write(";");
                summaryHeader.Write("Geschlecht");
                summaryHeader.Write(";");
                summaryHeader.Write("Screen Condition");
                summaryHeader.Write(";");
                summaryHeader.Write("Eingabe Typ");
                summaryHeader.Write(";");
                summaryHeader.Write("Itemset");
                summaryHeader.Write(";");
                summaryHeader.Write("EMG Energy");
                summaryHeader.Write(";");
                summaryHeader.Write("Accelerometer Energy");
                summaryHeader.Write(";");
                summaryHeader.Write("Gyroscope Energy");
                summaryHeader.Write(";");
                summaryHeader.Write("EMG Energy Squared");
                summaryHeader.Write(";");
                summaryHeader.Write("Accelerometer Energy Squared");
                summaryHeader.Write(";");
                summaryHeader.Write("Gyroscope Energy Squared");


                summaryHeader.WriteLine("");


                summaryHeader.Close();



            }


            TextWriter summary = new StreamWriter(pathToAllData, true);

            // all conditions

            LinkedList<double> emgMeans = new LinkedList<double>();
            LinkedList<double> accMeans = new LinkedList<double>();
            LinkedList<double> gyroMeans = new LinkedList<double>();

            LinkedList<double> emgMeansSquared = new LinkedList<double>();
            LinkedList<double> accMeansSquared = new LinkedList<double>();
            LinkedList<double> gyroMeansSquared = new LinkedList<double>();

            LinkedList<double> timeList = new LinkedList<double>();


            // each condition: emg
            LinkedList<double> emgMeansMove = new LinkedList<double>();
            LinkedList<double> emgMeansTouch = new LinkedList<double>();
            LinkedList<double> emgMeansTrackpad = new LinkedList<double>();


            LinkedList<double> emgMeansMoveSquared = new LinkedList<double>();
            LinkedList<double> emgMeansTouchSquared = new LinkedList<double>();
            LinkedList<double> emgMeansTrackpadSquared = new LinkedList<double>();

            // each condition: acc
            LinkedList<double> accMeansMove = new LinkedList<double>();
            LinkedList<double> accMeansTouch = new LinkedList<double>();
            LinkedList<double> accMeansTrackpad = new LinkedList<double>();


            LinkedList<double> accMeansMoveSquared = new LinkedList<double>();
            LinkedList<double> accMeansTouchSquared = new LinkedList<double>();
            LinkedList<double> accMeansTrackpadSquared = new LinkedList<double>();

            // each condition: gyro
            LinkedList<double> gyroMeansMove = new LinkedList<double>();
            LinkedList<double> gyroMeansTouch = new LinkedList<double>();
            LinkedList<double> gyroMeansTrackpad = new LinkedList<double>();


            LinkedList<double> gyroMeansMoveSquared = new LinkedList<double>();
            LinkedList<double> gyroMeansTouchSquared = new LinkedList<double>();
            LinkedList<double> gyroMeansTrackpadSquared = new LinkedList<double>();

            // time
            LinkedList<double> timeListMove = new LinkedList<double>();
            LinkedList<double> timeListTouch = new LinkedList<double>();
            LinkedList<double> timeListTrackpad = new LinkedList<double>();



            foreach (string key in studyDataAllParticipants.Keys)
            {

                StudyData d;
                studyDataAllParticipants.TryGetValue(key, out d);

                if (d != null)
                {

                    // TextWriter file = new StreamWriter(path + "Participant"+ d.participantID + "/" + key + ".csv", false);
                    summary.Write(d.participantID);
                    summary.Write(";");

                    summary.Write(d.time);
                    timeList.AddLast(d.time);
                    summary.Write(";");
                    summary.Write(d.gender);
                    summary.Write(";");
                    summary.Write(d.screenCondition);
                    summary.Write(";");
                    summary.Write(d.inputType);
                    summary.Write(";");
                    summary.Write(d.itemSet);
                    summary.Write(";");
                    summary.Write(d.normalEMGList.Average());
                    emgMeans.AddLast(d.normalEMGList.Average());
                    summary.Write(";");
                    summary.Write(d.normalACCList.Average());
                    accMeans.AddLast(d.normalACCList.Average());
                    summary.Write(";");
                    summary.Write(d.normalGyroList.Average());
                    gyroMeans.AddLast(d.normalGyroList.Average());
                    summary.Write(";");
                    summary.Write(d.squaredEMGList.Average());
                    emgMeansSquared.AddLast(d.squaredEMGList.Average());
                    summary.Write(";");
                    summary.Write(d.squaredACCList.Average());
                    accMeansSquared.AddLast(d.squaredACCList.Average());
                    summary.Write(";");
                    summary.Write(d.squaredGyroList.Average());
                    gyroMeansSquared.AddLast(d.squaredGyroList.Average());

                    summary.WriteLine("");


                    if (d.inputType.Equals("Move"))
                    {
                        emgMeansMove.AddLast(d.normalEMGList.Average());
                        accMeansMove.AddLast(d.normalACCList.Average());
                        gyroMeansMove.AddLast(d.normalGyroList.Average());
                        emgMeansMoveSquared.AddLast(d.squaredEMGList.Average());
                        accMeansMoveSquared.AddLast(d.squaredACCList.Average());
                        gyroMeansMoveSquared.AddLast(d.squaredGyroList.Average());
                        timeListMove.AddLast(d.time);

                    }
                    else if (d.inputType.Equals("Touch"))
                    {
                        emgMeansTouch.AddLast(d.normalEMGList.Average());
                        accMeansTouch.AddLast(d.normalACCList.Average());
                        gyroMeansTouch.AddLast(d.normalGyroList.Average());
                        emgMeansTouchSquared.AddLast(d.squaredEMGList.Average());
                        accMeansTouchSquared.AddLast(d.squaredACCList.Average());
                        gyroMeansTouchSquared.AddLast(d.squaredGyroList.Average());
                        timeListTouch.AddLast(d.time);
                    }
                    else if (d.inputType.Equals("Touchpad"))
                    {
                        emgMeansTrackpad.AddLast(d.normalEMGList.Average());
                        accMeansTrackpad.AddLast(d.normalACCList.Average());
                        gyroMeansTrackpad.AddLast(d.normalGyroList.Average());
                        emgMeansTrackpadSquared.AddLast(d.squaredEMGList.Average());
                        accMeansTrackpadSquared.AddLast(d.squaredACCList.Average());
                        gyroMeansTrackpadSquared.AddLast(d.squaredGyroList.Average());
                        timeListTrackpad.AddLast(d.time);
                    }



                    // file.Close();
                }
            }

            summary.WriteLine();
            summary.WriteLine();

            // Footer - Header
            summary.Write(getDescriptiveStatisticsString(emgMeans, "Normal EMG Energy: All Participants, all input conditions", 0, true));
            summary.Write(getDescriptiveStatisticsString(accMeans, "Normal Accelerometer Energy: All Participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(gyroMeans, "Normal Gyroscope Energy: All Participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(emgMeansSquared, "Squared EMG Energy: All Participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(accMeansSquared, "Squared Accelerometer Energy: All Participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(gyroMeansSquared, "Squared Gyroscope Energy: All Participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(timeList, "Time: All Participants, all input conditions"));

            summary.WriteLine();
            summary.WriteLine();


            summary.WriteLine("Energy Data: EMG");
            summary.Write(getDescriptiveStatisticsString(emgMeansMove, "Normal Emg Energy: All Participants, Move Controller", emgMeans.Average(), true));
            summary.Write(getDescriptiveStatisticsString(emgMeansTrackpad, "Normal Emg Energy: All Participants, Trackpad", emgMeans.Average()));
            summary.Write(getDescriptiveStatisticsString(emgMeansTouch, "Normal Emg Energy: All Participants, Touch", emgMeans.Average()));

            summary.WriteLine();
            summary.WriteLine("Energy Data: Accelerometer");
            summary.Write(getDescriptiveStatisticsString(accMeansMove, "Normal Accelerometer Energy: All Participants, Move Controller", accMeans.Average(), true));
            summary.Write(getDescriptiveStatisticsString(accMeansTrackpad, "Normal Accelerometer Energy: All Participants, Trackpad", accMeans.Average()));
            summary.Write(getDescriptiveStatisticsString(accMeansTouch, "Normal Accelerometer Energy: All Participants, Touch", accMeans.Average()));

            summary.WriteLine();
            summary.WriteLine("Energy Data: Gyroscope");
            summary.Write(getDescriptiveStatisticsString(gyroMeansMove, "Normal Gyroscope Energy: All Participants, Move Controller", gyroMeans.Average(), true));
            summary.Write(getDescriptiveStatisticsString(gyroMeansTrackpad, "Normal Gyroscope Energy: All Participants, Trackpad", gyroMeans.Average()));
            summary.Write(getDescriptiveStatisticsString(gyroMeansTouch, "Normal Gyroscope Energy: All Participants, Touch", gyroMeans.Average()));

            summary.WriteLine();
            summary.WriteLine("Energy Data: EMG (Squared)");
            summary.Write(getDescriptiveStatisticsString(emgMeansMoveSquared, "Squared Emg Energy: All Participants, Move Controller", emgMeansSquared.Average(), true));
            summary.Write(getDescriptiveStatisticsString(emgMeansTrackpadSquared, "Squared Emg Energy: All Participants, Trackpad", emgMeansSquared.Average()));
            summary.Write(getDescriptiveStatisticsString(emgMeansTouchSquared, "Squared Emg Energy: All Participants, Touch", emgMeansSquared.Average()));

            summary.WriteLine();
            summary.WriteLine("Energy Data: Accelerometer (Squared)");
            summary.Write(getDescriptiveStatisticsString(accMeansMoveSquared, "Squared Accelerometer Energy: All Participants, Move Controller", accMeansSquared.Average(), true));
            summary.Write(getDescriptiveStatisticsString(accMeansTrackpadSquared, "Squared Accelerometer Energy: All Participants, Trackpad", accMeansSquared.Average()));
            summary.Write(getDescriptiveStatisticsString(accMeansTouchSquared, "Squared Accelerometer Energy: All Participants, Touch", accMeansSquared.Average()));

            summary.WriteLine();
            summary.WriteLine("Energy Data: Gyroscope (Squared)");
            summary.Write(getDescriptiveStatisticsString(gyroMeansMoveSquared, "Squared Gyroscope Energy: All Participants, Move Controller", gyroMeansSquared.Average(), true));
            summary.Write(getDescriptiveStatisticsString(gyroMeansTrackpadSquared, "Squared Gyroscope Energy: All Participants, Trackpad", gyroMeansSquared.Average()));
            summary.Write(getDescriptiveStatisticsString(gyroMeansTouchSquared, "Squared Gyroscope Energy: All Participants, Touch", gyroMeansSquared.Average()));

            summary.WriteLine();
            summary.WriteLine("Time in seconds");
            summary.Write(getDescriptiveStatisticsString(timeListMove, "Move Time", timeList.Average(), true));
            summary.Write(getDescriptiveStatisticsString(timeListTrackpad, "Trackpad Time", timeList.Average(), false));
            summary.Write(getDescriptiveStatisticsString(timeListTouch, "Touch Time", timeList.Average(), false));



            summary.Close();

        }


        public void writeParticipantStatisticsv2()
        {
            // TODO path should depend on ID
            string path = "./Logs/Results/";

            checkDirectory(path);



            string pathToAllData = path + "participantSummary.csv";

            Console.WriteLine(File.Exists(pathToAllData) ? "File exists." : "File does not exist.");

            if (!File.Exists(pathToAllData))
            {
                // write summary header
                TextWriter summaryHeader = new StreamWriter(pathToAllData, false);

                summaryHeader.WriteLine("sep=;");

               
                summaryHeader.Write("Teilnehmer ID");
                summaryHeader.Write(";");
                summaryHeader.Write("Zeit(Sekunden)");
                summaryHeader.Write(";");
                summaryHeader.Write("Geschlecht");
                summaryHeader.Write(";");
                summaryHeader.Write("Screen Condition");
                summaryHeader.Write(";");
                summaryHeader.Write("Eingabe Typ");
                summaryHeader.Write(";");
                summaryHeader.Write("Itemset");
                summaryHeader.Write(";");


                summaryHeader.Write("ACC_x Mean");
                summaryHeader.Write(";");
                summaryHeader.Write("ACC_y Mean");
                summaryHeader.Write(";");
                summaryHeader.Write("ACC_z Mean");
                summaryHeader.Write(";");

                summaryHeader.Write("Gyro_x Mean");
                summaryHeader.Write(";");
                summaryHeader.Write("Gyro_y Mean");
                summaryHeader.Write(";");
                summaryHeader.Write("Gyro_z Mean");

                summaryHeader.Write(";");

                summaryHeader.Write("EMG Energy");
                summaryHeader.Write(";");
                summaryHeader.Write("Accelerometer Energy");
                summaryHeader.Write(";");
                summaryHeader.Write("Gyroscope Energy");
                summaryHeader.Write(";");
                summaryHeader.Write("EMG Energy Squared");
                summaryHeader.Write(";");
                summaryHeader.Write("Accelerometer Energy Squared");
                summaryHeader.Write(";");
                summaryHeader.Write("Gyroscope Energy Squared");


                summaryHeader.WriteLine("");


                summaryHeader.Close();



            }


            TextWriter summary = new StreamWriter(pathToAllData, true);

            // all conditions

            LinkedList<double> emgMeans = new LinkedList<double>();
            LinkedList<double> accMeans = new LinkedList<double>();
            LinkedList<double> gyroMeans = new LinkedList<double>();

            LinkedList<double> emgMeansSquared = new LinkedList<double>();
            LinkedList<double> accMeansSquared = new LinkedList<double>();
            LinkedList<double> gyroMeansSquared = new LinkedList<double>();

            LinkedList<double> timeList = new LinkedList<double>();


            // each condition: emg
            LinkedList<double> emgMeansMove = new LinkedList<double>();
            LinkedList<double> emgMeansTouch = new LinkedList<double>();
            LinkedList<double> emgMeansTrackpad = new LinkedList<double>();


            LinkedList<double> emgMeansMoveSquared = new LinkedList<double>();
            LinkedList<double> emgMeansTouchSquared = new LinkedList<double>();
            LinkedList<double> emgMeansTrackpadSquared = new LinkedList<double>();

            // each condition: acc
            LinkedList<double> accMeansMove = new LinkedList<double>();
            LinkedList<double> accMeansTouch = new LinkedList<double>();
            LinkedList<double> accMeansTrackpad = new LinkedList<double>();


            LinkedList<double> accMeansMoveSquared = new LinkedList<double>();
            LinkedList<double> accMeansTouchSquared = new LinkedList<double>();
            LinkedList<double> accMeansTrackpadSquared = new LinkedList<double>();

            // each condition: gyro
            LinkedList<double> gyroMeansMove = new LinkedList<double>();
            LinkedList<double> gyroMeansTouch = new LinkedList<double>();
            LinkedList<double> gyroMeansTrackpad = new LinkedList<double>();


            LinkedList<double> gyroMeansMoveSquared = new LinkedList<double>();
            LinkedList<double> gyroMeansTouchSquared = new LinkedList<double>();
            LinkedList<double> gyroMeansTrackpadSquared = new LinkedList<double>();

            // time
            LinkedList<double> timeListMove = new LinkedList<double>();
            LinkedList<double> timeListTouch = new LinkedList<double>();
            LinkedList<double> timeListTrackpad = new LinkedList<double>();

            // each condition: All
            //LinkedList<double> emg0All = new LinkedList<double>();
            //LinkedList<double> emg1All = new LinkedList<double>();
            //LinkedList<double> emg2All = new LinkedList<double>();
            //LinkedList<double> emg3All = new LinkedList<double>();
            //LinkedList<double> emg4All = new LinkedList<double>();
            //LinkedList<double> emg5All = new LinkedList<double>();
            //LinkedList<double> emg6All = new LinkedList<double>();
            //LinkedList<double> emg7All = new LinkedList<double>();
            LinkedList<double> accXAll = new LinkedList<double>();
            LinkedList<double> accYAll = new LinkedList<double>();
            LinkedList<double> accZAll = new LinkedList<double>();
            LinkedList<double> gyroXAll = new LinkedList<double>();
            LinkedList<double> gyroYAll = new LinkedList<double>();
            LinkedList<double> gyroZAll = new LinkedList<double>();



            // each condition: move
            //LinkedList<double> emg0Move = new LinkedList<double>();
            //LinkedList<double> emg1Move = new LinkedList<double>();
            //LinkedList<double> emg2Move = new LinkedList<double>();
            //LinkedList<double> emg3Move = new LinkedList<double>();
            //LinkedList<double> emg4Move = new LinkedList<double>();
            //LinkedList<double> emg5Move = new LinkedList<double>();
            //LinkedList<double> emg6Move = new LinkedList<double>();
            //LinkedList<double> emg7Move = new LinkedList<double>();
            LinkedList<double> accXMove = new LinkedList<double>();
            LinkedList<double> accYMove = new LinkedList<double>();
            LinkedList<double> accZMove = new LinkedList<double>();
            LinkedList<double> gyroXMove = new LinkedList<double>();
            LinkedList<double> gyroYMove = new LinkedList<double>();
            LinkedList<double> gyroZMove = new LinkedList<double>();

            // each condition: Trackpad
            //LinkedList<double> emg0Trackpad = new LinkedList<double>();
            //LinkedList<double> emg1Trackpad = new LinkedList<double>();
            //LinkedList<double> emg2Trackpad = new LinkedList<double>();
            //LinkedList<double> emg3Trackpad = new LinkedList<double>();
            //LinkedList<double> emg4Trackpad = new LinkedList<double>();
            //LinkedList<double> emg5Trackpad = new LinkedList<double>();
            //LinkedList<double> emg6Trackpad = new LinkedList<double>();
            //LinkedList<double> emg7Trackpad = new LinkedList<double>();
            LinkedList<double> accXTrackpad = new LinkedList<double>();
            LinkedList<double> accYTrackpad = new LinkedList<double>();
            LinkedList<double> accZTrackpad = new LinkedList<double>();
            LinkedList<double> gyroXTrackpad = new LinkedList<double>();
            LinkedList<double> gyroYTrackpad = new LinkedList<double>();
            LinkedList<double> gyroZTrackpad = new LinkedList<double>();

            // each condition: Touch
            //LinkedList<double> emg0Touch = new LinkedList<double>();
            //LinkedList<double> emg1Touch = new LinkedList<double>();
            //LinkedList<double> emg2Touch = new LinkedList<double>();
            //LinkedList<double> emg3Touch = new LinkedList<double>();
            //LinkedList<double> emg4Touch = new LinkedList<double>();
            //LinkedList<double> emg5Touch = new LinkedList<double>();
            //LinkedList<double> emg6Touch = new LinkedList<double>();
            //LinkedList<double> emg7Touch = new LinkedList<double>();
            LinkedList<double> accXTouch = new LinkedList<double>();
            LinkedList<double> accYTouch = new LinkedList<double>();
            LinkedList<double> accZTouch = new LinkedList<double>();
            LinkedList<double> gyroXTouch = new LinkedList<double>();
            LinkedList<double> gyroYTouch = new LinkedList<double>();
            LinkedList<double> gyroZTouch = new LinkedList<double>();



            foreach (string key in studyDataAllParticipants.Keys)
            {

                StudyData d;
                studyDataAllParticipants.TryGetValue(key, out d);

                if (d != null)
                {

                    // TextWriter file = new StreamWriter(path + "Participant"+ d.participantID + "/" + key + ".csv", false);
                    summary.Write(d.participantID);
                    summary.Write(";");

                    summary.Write(d.time);
                    timeList.AddLast(d.time);
                    summary.Write(";");
                    summary.Write(d.gender);
                    summary.Write(";");
                    summary.Write(d.screenCondition);
                    summary.Write(";");
                    summary.Write(d.inputType);
                    summary.Write(";");
                    summary.Write(d.itemSet);
                    summary.Write(";");


                    summary.Write(d.getMean4MyoDataAsCSVString("acc"));
                    summary.Write(";");

                    summary.Write(d.getMean4MyoDataAsCSVString("gyro"));
                    summary.Write(";");

                    summary.Write(d.normalEMGList.Average());
                    emgMeans.AddLast(d.normalEMGList.Average());
                    summary.Write(";");
                    summary.Write(d.normalACCList.Average());
                    accMeans.AddLast(d.normalACCList.Average());
                    summary.Write(";");
                    summary.Write(d.normalGyroList.Average());
                    gyroMeans.AddLast(d.normalGyroList.Average());
                    summary.Write(";");
                    summary.Write(d.squaredEMGList.Average());
                    emgMeansSquared.AddLast(d.squaredEMGList.Average());
                    summary.Write(";");
                    summary.Write(d.squaredACCList.Average());
                    accMeansSquared.AddLast(d.squaredACCList.Average());
                    summary.Write(";");
                    summary.Write(d.squaredGyroList.Average());
                    gyroMeansSquared.AddLast(d.squaredGyroList.Average());

                    summary.WriteLine("");


                    //emg0All.AddLast(d.getColumnData("emg0").Average());
                    //emg1All.AddLast(d.getColumnData("emg1").Average());
                    //emg2All.AddLast(d.getColumnData("emg2").Average());
                    //emg3All.AddLast(d.getColumnData("emg3").Average());
                    //emg4All.AddLast(d.getColumnData("emg4").Average());
                    //emg5All.AddLast(d.getColumnData("emg5").Average());
                    //emg6All.AddLast(d.getColumnData("emg6").Average());
                    //emg7All.AddLast(d.getColumnData("emg7").Average());
                    accXAll.AddLast(d.getColumnData("accX").Average());
                    accYAll.AddLast(d.getColumnData("accY").Average());
                    accZAll.AddLast(d.getColumnData("accZ").Average());
                    gyroXAll.AddLast(d.getColumnData("gyroX").Average());
                    gyroYAll.AddLast(d.getColumnData("gyroY").Average());
                    gyroZAll.AddLast(d.getColumnData("gyroZ").Average());

                    if (d.inputType.Equals("Move"))
                    {
                        emgMeansMove.AddLast(d.normalEMGList.Average());
                        accMeansMove.AddLast(d.normalACCList.Average());
                        gyroMeansMove.AddLast(d.normalGyroList.Average());
                        emgMeansMoveSquared.AddLast(d.squaredEMGList.Average());
                        accMeansMoveSquared.AddLast(d.squaredACCList.Average());
                        gyroMeansMoveSquared.AddLast(d.squaredGyroList.Average());
                        timeListMove.AddLast(d.time);

                        //emg0Move.AddLast(d.getColumnData("emg0").Average());
                        //emg1Move.AddLast(d.getColumnData("emg1").Average());
                        //emg2Move.AddLast(d.getColumnData("emg2").Average());
                        //emg3Move.AddLast(d.getColumnData("emg3").Average());
                        //emg4Move.AddLast(d.getColumnData("emg4").Average());
                        //emg5Move.AddLast(d.getColumnData("emg5").Average());
                        //emg6Move.AddLast(d.getColumnData("emg6").Average());
                        //emg7Move.AddLast(d.getColumnData("emg7").Average());
                        accXMove.AddLast(d.getColumnData("accX").Average());
                        accYMove.AddLast(d.getColumnData("accY").Average());
                        accZMove.AddLast(d.getColumnData("accZ").Average());
                        gyroXMove.AddLast(d.getColumnData("gyroX").Average());
                        gyroYMove.AddLast(d.getColumnData("gyroY").Average());
                        gyroZMove.AddLast(d.getColumnData("gyroZ").Average());

                    }
                    else if (d.inputType.Equals("Touch"))
                    {
                        emgMeansTouch.AddLast(d.normalEMGList.Average());
                        accMeansTouch.AddLast(d.normalACCList.Average());
                        gyroMeansTouch.AddLast(d.normalGyroList.Average());
                        emgMeansTouchSquared.AddLast(d.squaredEMGList.Average());
                        accMeansTouchSquared.AddLast(d.squaredACCList.Average());
                        gyroMeansTouchSquared.AddLast(d.squaredGyroList.Average());
                        timeListTouch.AddLast(d.time);


                        //emg0Touch.AddLast(d.getColumnData("emg0").Average());
                        //emg1Touch.AddLast(d.getColumnData("emg1").Average());
                        //emg2Touch.AddLast(d.getColumnData("emg2").Average());
                        //emg3Touch.AddLast(d.getColumnData("emg3").Average());
                        //emg4Touch.AddLast(d.getColumnData("emg4").Average());
                        //emg5Touch.AddLast(d.getColumnData("emg5").Average());
                        //emg6Touch.AddLast(d.getColumnData("emg6").Average());
                        //emg7Touch.AddLast(d.getColumnData("emg7").Average());
                        accXTouch.AddLast(d.getColumnData("accX").Average());
                        accYTouch.AddLast(d.getColumnData("accY").Average());
                        accZTouch.AddLast(d.getColumnData("accZ").Average());
                        gyroXTouch.AddLast(d.getColumnData("gyroX").Average());
                        gyroYTouch.AddLast(d.getColumnData("gyroY").Average());
                        gyroZTouch.AddLast(d.getColumnData("gyroZ").Average());

                    }
                    else if (d.inputType.Equals("Touchpad"))
                    {
                        emgMeansTrackpad.AddLast(d.normalEMGList.Average());
                        accMeansTrackpad.AddLast(d.normalACCList.Average());
                        gyroMeansTrackpad.AddLast(d.normalGyroList.Average());
                        emgMeansTrackpadSquared.AddLast(d.squaredEMGList.Average());
                        accMeansTrackpadSquared.AddLast(d.squaredACCList.Average());
                        gyroMeansTrackpadSquared.AddLast(d.squaredGyroList.Average());
                        timeListTrackpad.AddLast(d.time);

                        //emg0Trackpad.AddLast(d.getColumnData("emg0").Average());
                        //emg1Trackpad.AddLast(d.getColumnData("emg1").Average());
                        //emg2Trackpad.AddLast(d.getColumnData("emg2").Average());
                        //emg3Trackpad.AddLast(d.getColumnData("emg3").Average());
                        //emg4Trackpad.AddLast(d.getColumnData("emg4").Average());
                        //emg5Trackpad.AddLast(d.getColumnData("emg5").Average());
                        //emg6Trackpad.AddLast(d.getColumnData("emg6").Average());
                        //emg7Trackpad.AddLast(d.getColumnData("emg7").Average());
                        accXTrackpad.AddLast(d.getColumnData("accX").Average());
                        accYTrackpad.AddLast(d.getColumnData("accY").Average());
                        accZTrackpad.AddLast(d.getColumnData("accZ").Average());
                        gyroXTrackpad.AddLast(d.getColumnData("gyroX").Average());
                        gyroYTrackpad.AddLast(d.getColumnData("gyroY").Average());
                        gyroZTrackpad.AddLast(d.getColumnData("gyroZ").Average());
                    }




                    // file.Close();
                }
            }

            // Explanation of the Columns
            summary.WriteLine();
            summary.WriteLine();


            string accGyroXYZ = "Die einzlenen Werte für Accelerometer bzw. Gyroscope Energie (x,y,z) sind die gemittelten Werte der einzelnen Teilnehmer.";
            string combined = "Die Werte von EMG/Accelerometer/Gyroscope Energy sind die gemittelten Werte der 8 EMG Sensoren bzw. der drei x,y,z des Accelerometers bzw. Gyroscopes wobei die einzelnen Werte addiert wurden und durch die Anzahl der Sensoren bzw. Koordinaten geteilt wurden. In den Spalten mit squared wurde beim addieren immer der quadrierte Wert des einzelnen Sensors bzw. der x,y,z Werte genommen.";

            string energyBerechungHeader = "Wie wird die Energie bestimmt?";
            string energyBerechnung = "Die Rohdaten vom Myo werden durch Oke's Programm zuerst normalisiert (jeder EMG Sensors, jede x,y,z Komponente), dann werden Daten gesammelt (Windowing, Windowlength = 30 sampes(?)), damit darauf ein ein Frequenzfilter angewendet (1Hz-20Hz) werden kann. Anschließen wird auf die gefilterten Fenster-Werte eine Fast Fourier Transformation (FFT) angewandt, um die Koeffizienten zu bestimmen, die für die Energie Berechnung beötigt werden, zu bestimmen. Der Energyaufwand (energy expenditure) wird dann anhand der Formel (siehe Paper TODO) bestimmt.";


            string bedeutung = "Bedeutung EMG / Accelerometer / Gyroscope nach meinem Verständnis";
            string emgBedeutung = "Die rohen EMG Werte des Myo's sind ganzzahlige Werte, die positiv bzw. negativ sein können. Je angespannter (belasteter(?)) der Muskel ist desto höher sind die Werte der einzelnen EMG Sensoren.";

            string accBedeutung = "Das Accelerometer beschreibt die lineare Beschleunigung entlang der drei Achsen x,y,z. So wie ich es verstehe, gibt es einen Accelerometer für jede Achse. Im Ruhezustand sollte auf das Accelerometer die Kraft 1g = 9.8m/s^2 (Erdanziehung) wirken.";

            string gyroBedeutung = "Die Werte des Gyroscope's beschreiben die Winkelgeschwindikeit (angular velocity) um die drei Achsen. D.h. der x-Wert beschreibt die Winkelgeschwindigkeit mit der um die -Achse rotiert wird.";

            summary.WriteLine("Wie wurden die einzelnen Spalten berechnet?");
            summary.WriteLine(accGyroXYZ);
            summary.WriteLine(combined);
            summary.WriteLine();
            summary.WriteLine(energyBerechungHeader);
            summary.WriteLine(energyBerechnung);
            summary.WriteLine();
            summary.WriteLine(bedeutung);
            summary.WriteLine(emgBedeutung);
            summary.WriteLine(accBedeutung);
            summary.WriteLine(gyroBedeutung);


            summary.WriteLine();
            summary.WriteLine();

            summary.WriteLine("Descriptive Statistics");
            summary.WriteLine();

            // Footer - Header
            summary.WriteLine("Zusammenfassung, alle Teilnehmer und alle Input conditions");
            summary.Write(getDescriptiveStatisticsString(emgMeans, "Normal EMG Energy: All Participants, all input conditions", 0, true));

            summary.Write(getDescriptiveStatisticsString(accXAll, "Normal Accelerometer X Energy: All participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(accYAll, "Normal Accelerometer Y Energy: All participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(accZAll, "Normal Accelerometer Z Energy: All participants, all input conditions"));

            summary.Write(getDescriptiveStatisticsString(gyroXAll, "Normal Gyroscope X Energy: All participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(gyroYAll, "Normal Gyroscope Y Energy: All participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(gyroZAll, "Normal Gyroscope Z Energy: All participants, all input conditions"));

            summary.Write(getDescriptiveStatisticsString(accMeans, "Normal Accelerometer Energy: All Participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(gyroMeans, "Normal Gyroscope Energy: All Participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(emgMeansSquared, "Squared EMG Energy: All Participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(accMeansSquared, "Squared Accelerometer Energy: All Participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(gyroMeansSquared, "Squared Gyroscope Energy: All Participants, all input conditions"));
            summary.Write(getDescriptiveStatisticsString(timeList, "Time: All Participants, all input conditions"));

            summary.WriteLine();
            summary.WriteLine();

 
            summary.WriteLine("Energy Data: EMG (basierend auf der Summe der einzelnen Sensorwerte");
            summary.Write(getDescriptiveStatisticsString(emgMeansMove, "Normal Emg Energy: All Participants, Move Controller", emgMeans.Average(), true));
            summary.Write(getDescriptiveStatisticsString(emgMeansTrackpad, "Normal Emg Energy: All Participants, Trackpad", emgMeans.Average()));
            summary.Write(getDescriptiveStatisticsString(emgMeansTouch, "Normal Emg Energy: All Participants, Touch", emgMeans.Average()));

            summary.WriteLine();

            summary.WriteLine("Acceleromert (x,y,z) und Gyroscope (x,y,z) sortiert nach Inputcondition");

            summary.WriteLine("Move");
    

            //summary.Write(getDescriptiveStatisticsString(emg0Move, "EMG_0", emg0All.Average(), true));
            //summary.Write(getDescriptiveStatisticsString(emg1Move, "EMG_1", emg1All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg2Move, "EMG_2", emg2All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg3Move, "EMG_3", emg3All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg4Move, "EMG_4", emg4All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg5Move, "EMG_5", emg5All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg6Move, "EMG_6", emg6All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg7Move, "EMG_7", emg7All.Average(), false));

            summary.Write(getDescriptiveStatisticsString(accXMove, "ACC_X", accXAll.Average(), true));
            summary.Write(getDescriptiveStatisticsString(accYMove, "ACC_Y", accYAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(accZMove, "ACC_Z", accZAll.Average(), false));
            summary.WriteLine();
            summary.Write(getDescriptiveStatisticsString(gyroXMove, "GYRO_X", gyroXAll.Average(), true));
            summary.Write(getDescriptiveStatisticsString(gyroYMove, "GYRO_Y", gyroYAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(gyroZMove, "GYRO_Z", gyroZAll.Average(), false));

            summary.WriteLine();


            summary.WriteLine("Touch");


            //summary.Write(getDescriptiveStatisticsString(emg0Touch, "EMG_0", emg0All.Average(), true));
            //summary.Write(getDescriptiveStatisticsString(emg1Touch, "EMG_1", emg1All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg2Touch, "EMG_2", emg2All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg3Touch, "EMG_3", emg3All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg4Touch, "EMG_4", emg4All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg5Touch, "EMG_5", emg5All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg6Touch, "EMG_6", emg6All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg7Touch, "EMG_7", emg7All.Average(), false));

   
            summary.Write(getDescriptiveStatisticsString(accXTouch, "ACC_X", accXAll.Average(), true));
            summary.Write(getDescriptiveStatisticsString(accYTouch, "ACC_Y", accYAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(accZTouch, "ACC_Z", accZAll.Average(), false));
            summary.WriteLine();
            summary.Write(getDescriptiveStatisticsString(gyroXTouch, "GYRO_X", gyroXAll.Average(), true));
            summary.Write(getDescriptiveStatisticsString(gyroYTouch, "GYRO_Y", gyroYAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(gyroZTouch, "GYRO_Z", gyroZAll.Average(), false));

            summary.WriteLine();


            summary.WriteLine("Touchpad");
    

            //summary.Write(getDescriptiveStatisticsString(emg0Trackpad, "EMG_0", emg0All.Average(), true));
            //summary.Write(getDescriptiveStatisticsString(emg1Trackpad, "EMG_1", emg1All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg2Trackpad, "EMG_2", emg2All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg3Trackpad, "EMG_3", emg3All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg4Trackpad, "EMG_4", emg4All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg5Trackpad, "EMG_5", emg5All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg6Trackpad, "EMG_6", emg6All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg7Trackpad, "EMG_7", emg7All.Average(), false));


            summary.Write(getDescriptiveStatisticsString(accXTrackpad, "ACC_X", accXAll.Average(), true));
            summary.Write(getDescriptiveStatisticsString(accYTrackpad, "ACC_Y", accYAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(accZTrackpad, "ACC_Z", accZAll.Average(), false));

            summary.WriteLine();
            summary.Write(getDescriptiveStatisticsString(gyroXTrackpad, "GYRO_X", gyroXAll.Average(), true));
            summary.Write(getDescriptiveStatisticsString(gyroYTrackpad, "GYRO_Y", gyroYAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(gyroZTrackpad, "GYRO_Z", gyroZAll.Average(), false));


            /////////////////////////////////

            //summary.WriteLine("EMG");

            //summary.Write(getDescriptiveStatisticsString(emg0Move, "EMG_0_Move", emg0All.Average(), true));
            //summary.Write(getDescriptiveStatisticsString(emg0Touch, "EMG_0_Touch", emg0All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg0Trackpad, "EMG_0_Trackpad", emg0All.Average(), false));

            //summary.Write(getDescriptiveStatisticsString(emg1Move, "EMG_1_Move", emg1All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg1Touch, "EMG_1_Touch", emg1All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg1Trackpad, "EMG_1_Trackpad", emg1All.Average(), false));

            //summary.Write(getDescriptiveStatisticsString(emg2Move, "EMG_2_Move", emg2All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg2Touch, "EMG_2_Touch", emg2All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg2Trackpad, "EMG_2_Trackpad", emg2All.Average(), false));

            //summary.Write(getDescriptiveStatisticsString(emg3Move, "EMG_3_Move", emg3All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg3Touch, "EMG_3_Touch", emg3All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg3Trackpad, "EMG_3_Trackpad", emg3All.Average(), false));

            //summary.Write(getDescriptiveStatisticsString(emg4Move, "EMG_4_Move", emg4All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg4Touch, "EMG_4_Touch", emg4All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg4Trackpad, "EMG_4_Trackpad", emg4All.Average(), false));

            //summary.Write(getDescriptiveStatisticsString(emg5Move, "EMG_5_Move", emg5All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg5Touch, "EMG_5_Touch", emg5All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg5Trackpad, "EMG_5_Trackpad", emg5All.Average(), false));

            //summary.Write(getDescriptiveStatisticsString(emg6Move, "EMG_6_Move", emg6All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg6Touch, "EMG_6_Touch", emg6All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg6Trackpad, "EMG_6_Trackpad", emg6All.Average(), false));

            //summary.Write(getDescriptiveStatisticsString(emg7Move, "EMG_7_Move", emg7All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg7Touch, "EMG_7_Touch", emg7All.Average(), false));
            //summary.Write(getDescriptiveStatisticsString(emg7Trackpad, "EMG_7_Trackpad", emg7All.Average(), false));

            summary.WriteLine();

            summary.WriteLine("Accelerometer (x,y,z) und Gyroscope (x,y,z) sortiert nach x,y,z Werten.");

            summary.WriteLine("Accelerometer");

            summary.Write(getDescriptiveStatisticsString(accXMove, "ACC_X_Move", accXAll.Average(), true));
            summary.Write(getDescriptiveStatisticsString(accXTouch, "ACC_X_Touch", accXAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(accXTrackpad, "ACC_X_Trackpad", accXAll.Average(), false));
            summary.WriteLine();
            summary.Write(getDescriptiveStatisticsString(accYMove, "ACC_Y_Move", accYAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(accYTouch, "ACC_Y_Touch", accYAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(accYTrackpad, "ACC_Y_Trackpad", accYAll.Average(), false));
            summary.WriteLine();
            summary.Write(getDescriptiveStatisticsString(accZMove, "ACC_Z_Move", accZAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(accZTouch, "ACC_Z_Touch", accZAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(accZTrackpad, "ACC_Z_Trackpad", accZAll.Average(), false));


            summary.WriteLine();
            summary.WriteLine("Gyroscope");
            summary.Write(getDescriptiveStatisticsString(gyroXMove, "GYRO_X_Move", gyroXAll.Average(), true));
            summary.Write(getDescriptiveStatisticsString(gyroXTouch, "GYRO_X_Touch", gyroXAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(gyroXTrackpad, "GYRO_X_Trackpad", gyroXAll.Average(), false));
            summary.WriteLine();
            summary.Write(getDescriptiveStatisticsString(gyroYMove, "GYRO_Y_Move", gyroYAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(gyroYTouch, "GYRO_Y_Touch", gyroYAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(gyroYTrackpad, "GYRO_Y_Trackpad", gyroYAll.Average(), false));
            summary.WriteLine();

            summary.Write(getDescriptiveStatisticsString(gyroZMove, "GYRO_Z_Move", gyroZAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(gyroZTouch, "GYRO_Z_Touch", gyroZAll.Average(), false));
            summary.Write(getDescriptiveStatisticsString(gyroZTrackpad, "GYRO_Z_Trackpad", gyroZAll.Average(), false));

            //////////////////////

            summary.WriteLine();
            summary.WriteLine();



     
            summary.WriteLine("Energy Data: Accelerometer (basierend auf der Summe der x,y,z Werte)");
            summary.Write(getDescriptiveStatisticsString(accMeansMove, "Normal Accelerometer Energy: All Participants, Move Controller", accMeans.Average(), true));
            summary.Write(getDescriptiveStatisticsString(accMeansTrackpad, "Normal Accelerometer Energy: All Participants, Trackpad", accMeans.Average()));
            summary.Write(getDescriptiveStatisticsString(accMeansTouch, "Normal Accelerometer Energy: All Participants, Touch", accMeans.Average()));

            summary.WriteLine();
            summary.WriteLine("Energy Data: Gyroscope (basierend auf der Summe der x,y,z Werte)");
            summary.Write(getDescriptiveStatisticsString(gyroMeansMove, "Normal Gyroscope Energy: All Participants, Move Controller", gyroMeans.Average(), true));
            summary.Write(getDescriptiveStatisticsString(gyroMeansTrackpad, "Normal Gyroscope Energy: All Participants, Trackpad", gyroMeans.Average()));
            summary.Write(getDescriptiveStatisticsString(gyroMeansTouch, "Normal Gyroscope Energy: All Participants, Touch", gyroMeans.Average()));

            summary.WriteLine();
            summary.WriteLine("Energy Data: EMG (Squared) (basierend auf der Summe der quadrierten Sensorwerte)");
            summary.Write(getDescriptiveStatisticsString(emgMeansMoveSquared, "Squared Emg Energy: All Participants, Move Controller", emgMeansSquared.Average(), true));
            summary.Write(getDescriptiveStatisticsString(emgMeansTrackpadSquared, "Squared Emg Energy: All Participants, Trackpad", emgMeansSquared.Average()));
            summary.Write(getDescriptiveStatisticsString(emgMeansTouchSquared, "Squared Emg Energy: All Participants, Touch", emgMeansSquared.Average()));

            summary.WriteLine();
            summary.WriteLine("Energy Data: Accelerometer (Squared) (basierend auf der Summe der x^2,y^2,z^2 Werte)");
            summary.Write(getDescriptiveStatisticsString(accMeansMoveSquared, "Squared Accelerometer Energy: All Participants, Move Controller", accMeansSquared.Average(), true));
            summary.Write(getDescriptiveStatisticsString(accMeansTrackpadSquared, "Squared Accelerometer Energy: All Participants, Trackpad", accMeansSquared.Average()));
            summary.Write(getDescriptiveStatisticsString(accMeansTouchSquared, "Squared Accelerometer Energy: All Participants, Touch", accMeansSquared.Average()));

            summary.WriteLine();
            summary.WriteLine("Energy Data: Gyroscope (Squared) (basierend auf der Summe der x^2,y^2,z^2 Werte)");
            summary.Write(getDescriptiveStatisticsString(gyroMeansMoveSquared, "Squared Gyroscope Energy: All Participants, Move Controller", gyroMeansSquared.Average(), true));
            summary.Write(getDescriptiveStatisticsString(gyroMeansTrackpadSquared, "Squared Gyroscope Energy: All Participants, Trackpad", gyroMeansSquared.Average()));
            summary.Write(getDescriptiveStatisticsString(gyroMeansTouchSquared, "Squared Gyroscope Energy: All Participants, Touch", gyroMeansSquared.Average()));

            summary.WriteLine();
            summary.WriteLine("Time in seconds (durschnittliche Zeit für jede Inputcondition, aller Teilnehmer");
            summary.Write(getDescriptiveStatisticsString(timeListMove, "Move Time", timeList.Average(), true));
            summary.Write(getDescriptiveStatisticsString(timeListTrackpad, "Trackpad Time", timeList.Average(), false));
            summary.Write(getDescriptiveStatisticsString(timeListTouch, "Touch Time", timeList.Average(), false));



            summary.Close();

        }

        private string getDescriptiveStatisticsString(LinkedList<double> data, string title, double overallMean = 0, bool useHeader = false)
        {

            StringBuilder sb = new StringBuilder();

            //sb.AppendLine(title);

            if (useHeader)
            {

                sb.Append("Name");
                sb.Append(";");
                sb.Append("n");
                sb.Append(";");
                sb.Append("Mean");
                sb.Append(";");
                sb.Append("Median");
                sb.Append(";");
                sb.Append("Standard Deviation");
                sb.Append(";");
                sb.Append("Variance");
                sb.Append(";");
                sb.Append("Min");
                sb.Append(";");
                sb.Append("Max");
                sb.Append(";");
                sb.Append("Kurtosis");
                sb.Append(";");
                sb.Append("Skewness");
                sb.Append(";");
                sb.Append("Overall Mean");
                sb.AppendLine("");

            }

            sb.Append(title);
            sb.Append(";");
            sb.Append(data.Count);
            sb.Append(";");
            if (data != null && data.Count > 0)
            {
                sb.Append(data.Average());
                sb.Append(";");
                sb.Append(data.Median());
                sb.Append(";");
                sb.Append(data.StandardDeviation());
                sb.Append(";");
                sb.Append(data.Variance());
                sb.Append(";");
                sb.Append(data.Min());
                sb.Append(";");
                sb.Append(data.Max());
                sb.Append(";");
                sb.Append(data.Kurtosis());
                sb.Append(";");
                sb.Append(data.Skewness());
                sb.Append(";");
                sb.Append(overallMean);

            }
            sb.AppendLine("");


            return sb.ToString();


        }






    }
}
