using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Waveplus.DaqSys;
using Waveplus.DaqSys.Definitions;
using Waveplus.DaqSysInterface;
using WaveplusLab.Shared.Definitions;

namespace Playground
{
     class Program
     {
        static void Main()
        {
            /*
             * 1. [Server] Start TCP server, wait for connections
             * 2. [Matlab] Start matlab program that connects
             *  a.[Matlab] Send integer denoting sensor count
             * 3. [Server] When receiving connection, start capture with requested sensor count
             * 4. [Server] Send data to socket as soon as it's received from sensors
             * 5. [Matlab] Read data from socket, GOTO 4
             */

            new Program();
 
        }

        DaqSystem daqSystem;
        TcpListener listener;
        NetworkStream networkStream;

        byte[] buffer = new byte[1000000];

        public Program()
        {
            ConfigureDaq();

            StartServer();
            networkStream = WaitForClient();
          
            Console.WriteLine("Starting capture");
            daqSystem.StartCapturing(DataAvailableEventPeriod.ms_100); // Available: 100, 50, 25, 10

            Console.ReadKey();
        }

        private void StartServer()
        {
            // Start server
            IPAddress localAdd = IPAddress.Parse("127.0.0.1");
            int port = 5000;
            listener = new TcpListener(localAdd, port);
            listener.Start();
            Console.WriteLine("Listening on " + localAdd + " " + port);
        }

        private NetworkStream WaitForClient()
        {
            // Wait for client to connect
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected");

            // Get stream to read/write data
            return client.GetStream();
        }

        private void Send(float[,] values) {
            Buffer.BlockCopy(values, 0, buffer, 0, values.Length * 4) ;
            networkStream.Write(buffer, 0, values.Length * 4); 
        }

        private void ConfigureDaq()
        {
            // Create daqSystem object and assign the event handlers
            daqSystem = new DaqSystem();
            daqSystem.StateChanged += Device_StateChanged;
            daqSystem.DataAvailable += Capture_DataAvailable;

            // Configure sensors
            // .InstalledSensors = 16, not the number of sensed sensors

            for (int sensorNumber = 1; sensorNumber < 9; sensorNumber++)
            {
                //daqSystem.EnableSensor(sensorNumber);
                Console.WriteLine("Configuring sensor #" + sensorNumber + " to EMG sensor");
                daqSystem.ConfigureSensor(
                    new SensorConfiguration { SensorType = SensorType.EMG_SENSOR },
                    
                    sensorNumber
                );

            }

            for (int sensorNumber = 9; sensorNumber <= daqSystem.InstalledSensors; sensorNumber++)
            {
                //daqSystem.EnableSensor(sensorNumber);
                Console.WriteLine("Configuring sensor #" + sensorNumber + " to IMU sensor");
                daqSystem.ConfigureSensor(
                    new SensorConfiguration { SensorType = SensorType.INERTIAL_SENSOR },
                    sensorNumber
                );


                //daqSystem.DisableSensor(sensorNumber);
                //Console.WriteLine("totalSensorNum" + daqSystem.InstalledSensors);
            
            }

            //daqSystem.EnableSensor(13);
            //daqSystem.ConfigureSensor(new SensorConfiguration { SensorType = SensorType.INERTIAL_SENSOR }, 13);



            Console.WriteLine("Configuring capture");



            daqSystem.ConfigureCapture(
                new CaptureConfiguration { SamplingRate = SamplingRate.Hz_2000 , IMU_AcqType = ImuAcqType.RawData }
            );
        }

        // Capturing EMG samples

        //private void Capture_DataAvailable(object sender, DataAvailableEventArgs e)
        //{
        //    int samplesPerChannel = e.ScanNumber;
        //    float[] values = new float[samplesPerChannel*1]; // Change to add more sensors
        //    for (int sampleNumber = 0; sampleNumber < samplesPerChannel; sampleNumber = sampleNumber+1) // This loops captures data from sensor # sampleNumber+1
        //    {
        //        Console.WriteLine("Sensor #" + 1 + ": " + e.Samples[0, sampleNumber]); //write sensor values in console
        //        Console.WriteLine("Sensor #" + 2 + ": " + e.Samples[1, sampleNumber]); //write sensor values in console

        //        values[sampleNumber*1] = e.Samples[0, sampleNumber];
        //        //values[sampleNumber*4+1] = e.Samples[1, sampleNumber];
        //        //values[sampleNumber * 4 + 2] = e.Samples[2, sampleNumber];
        //        //values[sampleNumber * 4 + 3] = e.Samples[3, sampleNumber];
        //        //values[sampleNumber * 8 + 4] = e.Samples[4, sampleNumber];
        //        //values[sampleNumber * 8 + 5] = e.Samples[5, sampleNumber];
        //        //values[sampleNumber * 8 + 6] = e.Samples[6, sampleNumber];
        //        //values[sampleNumber * 8 + 7] = e.Samples[7, sampleNumber];



        //    }
        //    Send(values);
        //}

        // capturing IMU samples. The first index identifies the sensor (it can be between 0 and InstalledSensors -1)
        //The second can assume values of 0, 1, 2 that identifies respectively the values x, y and z of the angular velocity vector
        //The third index identifies the sample(it can be between 0 and SamplesNumber -1)

        private void Capture_DataAvailable(object sender, DataAvailableEventArgs e)
        {
            //int samplesPerChannel = e.ScanNumber; // the max scannumber is 20
            int samplesPerChannel = 20;
            float[,] values = new float[samplesPerChannel ,2]; // Change to add more sensors
            for (int sampleNumber = 0; sampleNumber < samplesPerChannel; sampleNumber = sampleNumber + 1) // This loops captures data from sensor # sampleNumber+1
            {
                 Console.WriteLine("Sensor #" + 15 + " angular velocity X: " + e.GyroscopeSamples[14,0, sampleNumber]+" "+ sampleNumber); //write sensor values in console
                 Console.WriteLine("Sensor #" + 15 + " angular velocity Y: " + e.GyroscopeSamples[14,1, sampleNumber] + " " + sampleNumber); //write sensor values in console
                // Console.WriteLine("Sensor #" + 15 + " angular velocity Z: " + e.GyroscopeSamples[14, 2, sampleNumber] + " " + sampleNumber);
                // Console.WriteLine("Sensor #" + 15 + " acceleration X: " + e.AccelerometerSamples[14, 0, sampleNumber] + " " + sampleNumber);
                //Console.WriteLine("Sensor #" + 15 + " acceleration Y: " + e.AccelerometerSamples[14, 1, sampleNumber] + " " + sampleNumber);
                //Console.WriteLine("Sensor #" + 15 + " acceleration Z: " + e.AccelerometerSamples[14, 2, sampleNumber] + " " + sampleNumber);

                values[sampleNumber, 0] = e.GyroscopeSamples[14, 0, sampleNumber];
                values[sampleNumber, 1] = e.GyroscopeSamples[14, 1, sampleNumber];
                //values[sampleNumber, 2] = e.GyroscopeSamples[14, 2, sampleNumber];
                //values[sampleNumber, 3] = e.AccelerometerSamples[14, 0, sampleNumber];
                //values[sampleNumber, 4] = e.AccelerometerSamples[14, 1, sampleNumber];
                //values[sampleNumber, 5] = e.AccelerometerSamples[14, 2, sampleNumber];
                

                //values[sampleNumber*4+1] = e.Samples[1, sampleNumber];
                //values[sampleNumber * 4 + 2] = e.Samples[2, sampleNumber];
                //values[sampleNumber * 4 + 3] = e.Samples[3, sampleNumber];
                //values[sampleNumber * 8 + 4] = e.Samples[4, sampleNumber];
                //values[sampleNumber * 8 + 5] = e.Samples[5, sampleNumber];
                //values[sampleNumber * 8 + 6] = e.Samples[6, sampleNumber];
                //values[sampleNumber * 8 + 7] = e.Samples[7, sampleNumber];



            }
            Send(values);
            //Console.WriteLine("imu values" + values[19,0]);
            //Console.ReadKey();
        }





        private void Device_StateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            Console.WriteLine(e);
        }
    }
}
