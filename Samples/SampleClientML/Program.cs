using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Security.Cryptography;
//using System.Numerics;

using System.Threading;
//using System.Runtime.Intrinsics.X86;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
namespace SampleClientML
{
    public class Program {
        public static SensorClient sensor_client = new SensorClient();
        public static WirelessSensorClient wlss_sensor_client = new WirelessSensorClient();
        public static CameraClientML camera_client = new CameraClientML();
        public static string filename = $"rigidbody_orientation_{Regex.Replace(DateTime.Now.ToString(), @"[^\w\.@-]", "")}.csv";
        private static string _comport = "COM6";
        private static string _wlss_comport = "COM8 ";
        public static void Main(string[] args) {

            Console.WriteLine("Log system starting...\n");
            //System.IO.File.AppendAllText(filename, $"Timestamp, pos_x, pos_y, pos_z, cam_qx, cam_qy, cam_qz, cam_qw, ss_qx, ss_qy, ss_qz, ss_qw, ");
            //System.IO.File.AppendAllText(filename, $"gyro_x, gyro_y, gyro_z, acc_x, acc_y, acc_z, mag_qw, mag_qx, mag_qy, stillness\n");

            //Console.WriteLine("Sensor recorder starting...\n");
            //Thread sensor_thread = new Thread(() => sensor_client.sensorRecoder(_comport));
            //sensor_thread.Start();

            //Console.WriteLine(Regex.Replace(DateTime.Now.ToString(), @"[^\w\.@-]", ""));
            //Console.WriteLine("Camera recorder starting...\n");
            //Thread camera_thread = new Thread(camera_client.cameraRecoder);
            //camera_thread.Start();

            Console.WriteLine("Sensor recorder starting...\n");
            Thread wlss_sensor_thread = new Thread(() => wlss_sensor_client.sensorRecoder(_wlss_comport));
            wlss_sensor_thread.Start();
        }
    }
}
