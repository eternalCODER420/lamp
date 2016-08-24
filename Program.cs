using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading;
using System.IO;
using System.Net;
using System.Xml;
/*
Program : Ampel2_V0.1
Author  : Simren
Date    : 24.08.16
*/
namespace Ampel2_V0._1
{
    class Program
    {

        public bool continuelog { get; set; }
        public bool error { get; set; }

        public static int postceed { get; set; }

        public static int blankint { get; set; }//blankint should be time of blank + time of signal; Do *1000 for seconds

        public static int signalint { get; set; }//Do *1000 for seconds

        static System.Timers.Timer _timer;

        public enum status : int
        {
            success = 0, failed = 1, building = 2, stop = 3

        }

        public enum SWITCH_IDs : int
        {
            SWITCH_0 = 0x10, SWITCH_1 = 0x11, SWITCH_2 = 0x12/*, SWITCH_3 = 0x13,
            SWITCH_4 = 0x14, SWITCH_5 = 0x15, SWITCH_6 = 0x16, SWITCH_7 = 0x17,
            SWITCH_8 = 0x18, SWITCH_9 = 0x19, SWITCH_10 = 0x1a, SWITCH_11 = 0x1b,
            SWITCH_12 = 0x1c, SWITCH_13 = 0x1d, SWITCH_14 = 0x1e, SWITCH_15 = 0x1f*/
        }
        public enum USBtype_enum : int
        {
            /* ILLEGAL_DEVICE = 0,
             LED_DEVICE = 0x01,
             POWER_DEVICE = 0x02,
             DISPLAY_DEVICE = 0x03,
             WATCHDOG_DEVICE = 0x05,
             AUTORESET_DEVICE = 0x06,
             WATCHDOGXP_DEVICE = 0x07,*/
            SWITCH1_DEVICE = 0x08/*,
            SWITCH2_DEVICE = 0x09, SWITCH3_DEVICE = 0x0a, SWITCH4_DEVICE = 0x0b,
            SWITCH5_DEVICE = 0x0c, SWITCH6_DEVICE = 0x0d, SWITCH7_DEVICE = 0x0e, SWITCH8_DEVICE = 0x0f,
            TEMPERATURE_DEVICE = 0x10,
            TEMPERATURE2_DEVICE = 0x11,
            TEMPERATURE5_DEVICE = 0x15,
            HUMIDITY1_DEVICE = 0x20, HUMIDITY2_DEVICE = 0x21,
            SWITCHX_DEVICE = 0x28,      // new switch 3,4,8
            CONTACT00_DEVICE = 0x30, CONTACT01_DEVICE = 0x31, CONTACT02_DEVICE = 0x32, CONTACT03_DEVICE = 0x33,
            CONTACT04_DEVICE = 0x34, CONTACT05_DEVICE = 0x35, CONTACT06_DEVICE = 0x36, CONTACT07_DEVICE = 0x37,
            CONTACT08_DEVICE = 0x38, CONTACT09_DEVICE = 0x39, CONTACT10_DEVICE = 0x3a, CONTACT11_DEVICE = 0x3b,
            CONTACT12_DEVICE = 0x3c, CONTACT13_DEVICE = 0x3d, CONTACT14_DEVICE = 0x3e, CONTACT15_DEVICE = 0x3f,
            F4_DEVICE = 0x40,
            KEYC01_DEVICE = 0x41, KEYC16_DEVICE = 0x42, MOUSE_DEVICE = 0x43,
            ADC0800_DEVICE = 0x50, ADC0801_DEVICE = 0x51, ADC0802_DEVICE = 0x52, ADC0803_DEVICE = 0x53,
            COUNTER00_DEVICE = 0x60, COUNTER01_DEVICE = 0x61, COUNTER02_DEVICE = 0x62,
            CONTACTTIMER00_DEVICE = 0x70, CONTACTTIMER01_DEVICE = 0x71, CONTACTTIMER02_DEVICE = 0x72,
            ENCODER01_DEVICE = 0x80,
            BUTTON_NODEVICE = 0x1000*/
        };

        [DllImport(@"USBaccess.DLL")]
        public static extern IntPtr FCWInitObject();
        [DllImport(@"USBaccess.DLL")]
        public static extern void FCWUnInitObject(IntPtr cwHdl);
        [DllImport(@"USBaccess.DLL")]
        public static extern int FCWOpenCleware(IntPtr cwHdl);
        [DllImport(@"USBaccess.DLL")]
        public static extern int FCWCloseCleware(IntPtr cwHdl);
        [DllImport(@"USBaccess.DLL")]
        public static extern int FCWGetUSBType(IntPtr cwHdl, int devNum);
        [DllImport(@"USBaccess.DLL")]
        public static extern int FCWSetSwitch(IntPtr cwHdl, int devNum, int Switch, int On);    //	On: 0=off, 1=on
        [DllImport(@"USBaccess.DLL")]
        public static extern int FCWGetSwitch(IntPtr cwHdl, int devNum, int Switch);

        static void Main(string[] args)
        {
            Program lamp = new Program();
            Program.blankint = 3500;// timerinterval = how long the lights should stay off
            Program.signalint = 2000;//Thread.Speep lenght = how long the signal should show
            lamp.repeater();
            bool eternal = true;
            while (eternal == true)
            { }

        }

        public void repeater()
        {
            Program lamp = new Program();
            Program.postceed = 4;
            Program._timer = new System.Timers.Timer(Program.blankint);
            Program._timer.Elapsed += new ElapsedEventHandler(signal);
            Program._timer.AutoReset = true;

            Program._timer.Enabled = true;
        }

        public int getstatus()
        {
            bool failed = false;
            // Create a request for the URL.         
            WebRequest request = WebRequest.Create("http://dev-build-01/ccnet/XmlStatusReport.aspx");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Display the status.
            Console.WriteLine(response.StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseFromServer);
            XmlNodeList elemList = doc.GetElementsByTagName("Project");
            for (int i = 0; i < elemList.Count; i++)
            {
                Console.WriteLine(elemList[i].OuterXml);
                string check = elemList[i].OuterXml;
                if (check.Contains("Building"))
                {
                    return (int)Program.status.building;
                }
                else if (check.Contains("Sleeping") && check.Contains("failed"))
                {
                    failed = true;
                }
            }
            if (failed == true)
            {
                return (int)Program.status.failed;
            }
            else
            {
                return (int)Program.status.success;
            }

        }
        public static void shutdown()
        {
            IntPtr cwObj = Program.FCWInitObject();
            int devCnt = Program.FCWOpenCleware(cwObj);
            int i = 0;
            int devType = Program.FCWGetUSBType(cwObj, i);
            if (devType == (int)Program.USBtype_enum.SWITCH1_DEVICE)
            {
                Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_0, 0);
                Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_1, 0);
                Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_2, 0);
            }
        }
        public void signal(object sender, System.Timers.ElapsedEventArgs e)
        {
            new Task(lastStand).Start();
            IntPtr cwObj = Program.FCWInitObject();
            int devCnt = Program.FCWOpenCleware(cwObj);
            int i = 0;
            int devType = Program.FCWGetUSBType(cwObj, i);
            if (devType == (int)Program.USBtype_enum.SWITCH1_DEVICE && error == false)
            {
                int text = getstatus() /*System.IO.File.ReadAllLines(@"C:\Users\s.singh.ACMT\Desktop\BuildStatus.txt", Encoding.Default).First()*/;
                new Task(reporter).Start();
                Program.shutdown();
                switch (text)
                {
                    case 1:
                        Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_0, 1);
                        Program.postceed = 0;
                        break;
                    case 2:
                        Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_1, 1);
                        Program.postceed = 1;
                        Thread.Sleep(Program.signalint);
                        Program.shutdown();
                        break;
                    case 0:
                        Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_2, 1);
                        Program.postceed = 2;
                        break;
                    case 4:
                        Program.postceed = 4;
                        //keep the lights sleeping
                        break;
                    default:
                        Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_0, 1);
                        Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_1, 1);
                        Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_2, 1);
                        Program.postceed = 3;
                        break;
                }

            }
            else if (error == true)
            {
                Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_0, 1);
                Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_1, 1);
                Program.FCWSetSwitch(cwObj, i, (int)Program.SWITCH_IDs.SWITCH_2, 1);
                error = false;
            }
        }

        public void reporter()
        {
            int state = Program.postceed;
            Thread.Sleep(250);

            bool exist = true;
            DateTime currentTime = DateTime.Now;
            if (!File.Exists(@"C:\Users\s.singh.ACMT\Documents\Visual Studio 2015\Projects\Ampel2_V0.1\Ampel2_V0.1\log.txt"))
            {
                File.Create(@"C:\Users\s.singh.ACMT\Documents\Visual Studio 2015\Projects\Ampel2_V0.1\Ampel2_V0.1\log.txt").Dispose();
                exist = false;
            }
            try
            {
                using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"C:\Users\s.singh.ACMT\Documents\Visual Studio 2015\Projects\Ampel2_V0.1\Ampel2_V0.1\log.txt", true))
                {
                    if (exist == false)
                    {
                        file.WriteLine(currentTime + " -- created log");
                    }
                    if (state != Program.postceed)
                    {
                        switch (postceed)
                        {
                            case 0:
                                file.WriteLine(currentTime + " -- build upload failed.");
                                break;
                            case 1:
                                file.WriteLine(currentTime + " -- build is being uploaded.");
                                break;
                            case 2:
                                file.WriteLine(currentTime + " -- ok.");
                                break;
                            case 3:
                                file.WriteLine(currentTime + " -- faulty input.");
                                break;
                            case 4:
                                file.WriteLine(currentTime + " -- faulty input.");
                                break;
                        }
                    }
                }
            }
            catch (IOException)
            {
                error = true;
            }
        }

        public static string clean(string s)
        {
            StringBuilder sb = new StringBuilder(s);
            sb.Replace("\r", " ");
            sb.Replace("\n", " ");
            sb.Replace("ü", "ue");
            sb.Replace("gruen", "green");
            sb.Replace("gelb", "yellow");
            sb.Replace("rot", "red");
            return sb.ToString().ToLower();
        }
        public void lastStand()
        {

            DateTime currentTime = DateTime.Now;
            DateTime startTime = Convert.ToDateTime("07:00:00");
            DateTime endDay = Convert.ToDateTime("23:59:59");
            DateTime endTime = Convert.ToDateTime("19:00:00");
            Program lamp = new Program();
            if (currentTime < startTime && currentTime > endTime)//zwischen 07.00 und 19.00
            {
                //proceed without problems and skip all the if's
            }
            else
            {

                bool exist = true;
                if (!File.Exists(@"C:\Users\s.singh.ACMT\Documents\Visual Studio 2015\Projects\Ampel2_V0.1\Ampel2_V0.1\log.txt"))
                {
                    File.Create(@"C:\Users\s.singh.ACMT\Documents\Visual Studio 2015\Projects\Ampel2_V0.1\Ampel2_V0.1\log.txt").Dispose();
                    exist = false;
                }
                try
                {
                    if (continuelog == true)
                    {
                        continuelog = false;
                        using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@"C:\Users\s.singh.ACMT\Documents\Visual Studio 2015\Projects\Ampel2_V0.1\Ampel2_V0.1\log.txt", true))
                        {
                            if (exist == false)
                            {
                                file.WriteLine(currentTime + " -- created log");
                            }

                            if (currentTime < startTime)//vor 7 uhr
                            {
                                Program._timer.AutoReset = false;
                                file.WriteLine(currentTime + " -- lamp offline");
                                Thread.Sleep(Program.signalint);
                                Program.shutdown();
                                Thread.Sleep(Convert.ToInt32(((startTime - currentTime).TotalSeconds) * 1000));//1 second should be written as 1000
                                lamp.repeater();
                                file.WriteLine(currentTime + " -- lamp online");
                                //<---------start main again(main will automaticly set autoreset to true)
                                continuelog = true;
                            }
                            else if (currentTime > endTime)//nach 19.00 Uhr
                            {
                                Program._timer.AutoReset = false;
                                file.WriteLine(currentTime + " -- lamp offline");
                                Thread.Sleep(Program.signalint);
                                Program.shutdown();
                                Thread.Sleep(Convert.ToInt32(((endDay - currentTime).TotalSeconds) * 1000) + 25200000/*7 hours*/);//1 second should be written as 1000
                                lamp.repeater();
                                file.WriteLine(currentTime + " -- lamp online");
                                //<---------start main again(main will automaticly set autoreset to true)
                                continuelog = true;

                            }
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                    error = true;
                }
            }
        }

        public void optionchanged()
        {
            Program lamp = new Program();
            Program._timer.AutoReset = false;
            lamp.repeater();
        }
    }
}
