using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Ports;
using System.Numerics;

namespace SampleClientML
{
    public class WirelessSensorClient
    {
        private static SerialPort _serialPort;
        public int TestKl { get; set; }

        public static byte[] stream_timing_bytes = new byte[16];
        public static byte[] start_stream_bytes = new byte[4];

        public static byte[] stop_command = { 0xF8, 0x00, 0x56, 0x56 };

        public Boolean failure_byte = false;
        public float Stillness0;
        public Vector3 Gyro0, Accelero0, Magneto0;
        public Quaternion IMUQuat0;

        static string filename = "";

        static long initTime = 0;
        public void sensorRecoder(string comport)
        {
            filename = Program.filename;
            string port_number = comport;
            byte[] read_bytes0 = new byte[59];

            Queue<float> alphaBuffer = new Queue<float>();

            int num_row = 1;
            float alpha0 = 0;
            initTime = DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond);


            Console.WriteLine("PRESS ESC TO EXIT\n");
            System.IO.File.AppendAllText(filename, $"Timestamp, gyro_x, gyro_y, gyro_z, acc_x, acc_y, acc_z, mag_qw, mag_qx, mag_qy, ss_qx, ss_qy, ss_qz, ss_qw, stillness\n");

            _serialPort = new SerialPort(port_number, 115200, Parity.None, 8, StopBits.One);

            _serialPort.ReadTimeout = 5000;
            _serialPort.WriteTimeout = 5000;

            try
            {
                _serialPort.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Port {port_number} is opening due to {e}");
            }


            _serialPort.Write(WirelessMARGSensor.stream_slots_bytes, 0, WirelessMARGSensor.stream_slots_bytes.Length);
            //_serialPort.Write(test_command, 0, test_command.Length);

            // The data must be flipped to big endian before sending to sensor
            Array.Reverse(WirelessMARGSensor.interval);    // byte[]
            Array.Reverse(WirelessMARGSensor.delay);       // byte[]
            Array.Reverse(WirelessMARGSensor.duration);    // byte[]

            stream_timing_bytes[0] = WirelessMARGSensor.TSS_START_BYTE;
            stream_timing_bytes[1] = WirelessMARGSensor.TSS_LOGICAL_ID;
            stream_timing_bytes[2] = WirelessMARGSensor.TSS_SET_STREAMING_TIMING;
            WirelessMARGSensor.interval.CopyTo(stream_timing_bytes, 3);
            WirelessMARGSensor.delay.CopyTo(stream_timing_bytes, 7);
            WirelessMARGSensor.duration.CopyTo(stream_timing_bytes, 11);
            stream_timing_bytes[15] = (byte)((stream_timing_bytes[1] 
                + stream_timing_bytes[2] 
                + stream_timing_bytes[3] 
                + stream_timing_bytes[4] 
                + stream_timing_bytes[5] 
                + stream_timing_bytes[6] 
                + stream_timing_bytes[7] 
                + stream_timing_bytes[8] 
                + stream_timing_bytes[9] 
                + stream_timing_bytes[10] 
                + stream_timing_bytes[11] 
                + stream_timing_bytes[12] 
                + stream_timing_bytes[13]) % 256);

            _serialPort.Write(stream_timing_bytes, 0, stream_timing_bytes.Length);

            start_stream_bytes[0] = WirelessMARGSensor.TSS_START_BYTE;
            start_stream_bytes[1] = WirelessMARGSensor.TSS_LOGICAL_ID;
            start_stream_bytes[2] = WirelessMARGSensor.TSS_START_STREAMING;
            start_stream_bytes[3] = (byte)((WirelessMARGSensor.TSS_LOGICAL_ID + WirelessMARGSensor.TSS_START_STREAMING) % 256);

            _serialPort.Write(start_stream_bytes, 0, start_stream_bytes.Length);

            _serialPort.Write(WirelessMARGSensor.tare_bytes, 0, 4);
            
            while (!(Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape))
            {
                read_bytes0 = Enumerable.Repeat((byte)1, 59).ToArray(); //TODO: Need to be initialised by other values except 0
                //Array.Clear(read_bytes0, 0, read_bytes0.Length);
                failure_byte = false;


                for (int i = 0; i < 59; i++) {
                    _serialPort.Read(read_bytes0, i, 1);
                    if (i == 2)
                        if (read_bytes0[0] == 0 && read_bytes0[1] == 0 && read_bytes0[2] == 56)
                        {
                            failure_byte = false;
                        }
                        else {
                            failure_byte = true;
                            break;
                        }
                }


                if (failure_byte)
                {
                    continue;
                }

                Stillness0 = WirelessMARGSensor.bytesToFloat(read_bytes0, 0 + 3);
                Gyro0.X = WirelessMARGSensor.bytesToFloat(read_bytes0, 4 + 3);
                Gyro0.Y = WirelessMARGSensor.bytesToFloat(read_bytes0, 8 + 3);
                Gyro0.Z = WirelessMARGSensor.bytesToFloat(read_bytes0, 12 + 3);
                Accelero0.X = WirelessMARGSensor.bytesToFloat(read_bytes0, 16 + 3);
                Accelero0.Y = WirelessMARGSensor.bytesToFloat(read_bytes0, 20 + 3);
                Accelero0.Z = WirelessMARGSensor.bytesToFloat(read_bytes0, 24 + 3);
                Magneto0.X = WirelessMARGSensor.bytesToFloat(read_bytes0, 28 + 3);
                Magneto0.Y = WirelessMARGSensor.bytesToFloat(read_bytes0, 32 + 3);
                Magneto0.Z = WirelessMARGSensor.bytesToFloat(read_bytes0, 36 + 3);
                IMUQuat0.X = WirelessMARGSensor.bytesToFloat(read_bytes0, 40 + 3);
                IMUQuat0.Y = WirelessMARGSensor.bytesToFloat(read_bytes0, 44 + 3);
                IMUQuat0.Z = WirelessMARGSensor.bytesToFloat(read_bytes0, 48 + 3);
                IMUQuat0.W = WirelessMARGSensor.bytesToFloat(read_bytes0, 52 + 3);

                long sampligTime = DateTime.Now.Ticks / (TimeSpan.TicksPerMillisecond);

                //Console.WriteLine($"{sampligTime - initTime}, {IMUQuat0.X}, {IMUQuat0.Y}, {IMUQuat0.Z}, {IMUQuat0.W}");

                if (num_row % 100 == 0) {
                    Console.Write("#");
                }

                if (num_row % 1000 == 0)
                {
                    Console.WriteLine("\n");
                }

                if (alphaBuffer.Count < 3)
                {
                    alphaBuffer.Enqueue(Stillness0);
                }
                else {
                    alphaBuffer.Dequeue();
                    alphaBuffer.Enqueue(Stillness0);
                }

                alpha0 = alphaBuffer.Sum() / 3;

                num_row++;
                System.IO.File.AppendAllText(filename, $"{sampligTime - initTime}, {Gyro0.X}, {Gyro0.Y}, {Gyro0.Z}, ");
                System.IO.File.AppendAllText(filename, $"{Accelero0.X}, {Accelero0.Y}, {Accelero0.Z}, ");
                System.IO.File.AppendAllText(filename, $"{Magneto0.X}, {Magneto0.Y}, {Magneto0.Z}, ");
                System.IO.File.AppendAllText(filename, $"{IMUQuat0.X}, {IMUQuat0.Y}, {IMUQuat0.Z}, {IMUQuat0.W}, {alpha0}\n");
            }

            _serialPort.Write(stop_command, 0, stop_command.Length);
        }
    }
}
