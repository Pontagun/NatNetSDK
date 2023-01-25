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

namespace SampleClientML
{
    public class SensorClient
    {
        private static SerialPort _serialPort;
        public int TestKl { get; set; }

        public static byte[] stream_timing_bytes = new byte[15];
        public static byte[] start_stream_bytes = new byte[3];
        public static byte[] interval = BitConverter.GetBytes(10000);
        public static byte[] delay = BitConverter.GetBytes(0);
        public static byte[] duration = BitConverter.GetBytes(0xFFFFFFFF);
        public float Stillness0;
        public Vector3 Gyro0, Accelero0, Magneto0;
        public Quaternion IMUQuat0 = new Quaternion(0, 0, 0, 1);

        static string filename = "";
        public void sensorRecoder(string comport)
        {
            filename = Program.filename;
            string port_number = comport;
            byte[] read_bytes0 = new byte[56];
            int read_counter = 100;
            int byte_idx0 = 0;
            _serialPort = new SerialPort(port_number, 115200, Parity.None, 8, StopBits.One);

            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            try
            {
                _serialPort.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Port {port_number} is opening due to {e}");
            }

            _serialPort.Write(WiredMARGSensor.stream_slots_bytes, 0, WiredMARGSensor.stream_slots_bytes.Length);

            Array.Reverse(interval);    // byte[]
            Array.Reverse(delay);       // byte[]
            Array.Reverse(duration);    // byte[]

            stream_timing_bytes[0] = WiredMARGSensor.TSS_START_BYTE;
            stream_timing_bytes[1] = WiredMARGSensor.TSS_SET_STREAMING_TIMING;
            interval.CopyTo(stream_timing_bytes, 2);
            delay.CopyTo(stream_timing_bytes, 6);
            duration.CopyTo(stream_timing_bytes, 10);

            stream_timing_bytes[14] = (byte)((stream_timing_bytes[1] + stream_timing_bytes[2] + stream_timing_bytes[3] + stream_timing_bytes[4] + stream_timing_bytes[5] + stream_timing_bytes[6] +
                                        stream_timing_bytes[7] + stream_timing_bytes[8] + stream_timing_bytes[9] + stream_timing_bytes[10] + stream_timing_bytes[11] + stream_timing_bytes[12] + 
                                        stream_timing_bytes[13]) % 256);

            _serialPort.Write(stream_timing_bytes, 0, stream_timing_bytes.Length);
            
            _serialPort.Write(WiredMARGSensor.tare_bytes, 0, 3);

            start_stream_bytes[0] = WiredMARGSensor.TSS_START_BYTE;
            start_stream_bytes[1] = WiredMARGSensor.TSS_START_STREAMING;
            start_stream_bytes[2] = WiredMARGSensor.TSS_START_STREAMING;

            _serialPort.Write(start_stream_bytes, 0, start_stream_bytes.Length);

            while (true)
            {
                read_bytes0 = new byte[56];
                read_counter = 100;
                byte_idx0 = 0;

                while (read_counter > 0)
                {
                    try
                    {
                        byte_idx0 += _serialPort.Read(read_bytes0, byte_idx0, 1);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("Error reading from the sensor");
                        // Failed to read from serial port
                    }
                    if (byte_idx0 == 56)
                    { // <----------------- No. of Bytes
                        //Console.WriteLine("Break case");
                        break;
                    }
                    if (read_counter <= 0)
                    {
                        throw new System.Exception("Failed to read quaternion from port too many times." +
                            " This could mean the port is not open or the Mono serial read is not responding.");
                    }
                    read_counter--;
                }

                Stillness0 = WiredMARGSensor.bytesToFloat(read_bytes0, 0);
                Gyro0.X = WiredMARGSensor.bytesToFloat(read_bytes0, 4);
                Gyro0.Y = WiredMARGSensor.bytesToFloat(read_bytes0, 8);
                Gyro0.Z = WiredMARGSensor.bytesToFloat(read_bytes0, 12);
                Accelero0.X = WiredMARGSensor.bytesToFloat(read_bytes0, 16);
                Accelero0.Y = WiredMARGSensor.bytesToFloat(read_bytes0, 20);
                Accelero0.Z = WiredMARGSensor.bytesToFloat(read_bytes0, 24);
                Magneto0.X = WiredMARGSensor.bytesToFloat(read_bytes0, 28);
                Magneto0.Y = WiredMARGSensor.bytesToFloat(read_bytes0, 32);
                Magneto0.Z = WiredMARGSensor.bytesToFloat(read_bytes0, 36);
                IMUQuat0.X = WiredMARGSensor.bytesToFloat(read_bytes0, 40);
                IMUQuat0.Y = WiredMARGSensor.bytesToFloat(read_bytes0, 44);
                IMUQuat0.Z = WiredMARGSensor.bytesToFloat(read_bytes0, 48);
                IMUQuat0.W = WiredMARGSensor.bytesToFloat(read_bytes0, 52);

                Console.WriteLine($"{IMUQuat0.X}, {IMUQuat0.Y}, {IMUQuat0.Z}, {IMUQuat0.W}");

                //long seconds = DateTime.Now.Ticks / 100; 
                //System.IO.File.AppendAllText(filename, $"{seconds}, {IMUQuat0.X}, {IMUQuat0.Y}, {IMUQuat0.Z}, {IMUQuat0.W}\n");
            }
        }
    }
}
