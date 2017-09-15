using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace rpidotnet
{
    class Program
    {
        private static readonly List<ISerialPortListener> listeners = new List<ISerialPortListener>();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var portNames = SerialPort.GetPortNames();

            //var primarySystem = new MaiiSystemInfo() {
            //    Priority = MaiiSystemPriority.Primary,
            //    Port = "/dev/ttyUSB3",
            //    PortBaudRate = 115200,
            //    SourcePath = "/home/pi/maii-systems-primary/"
            //};

            // var listenerPrimary = new SerialPortListener("/dev/ttyUSB0", 115200, "\n");
            // listenerPrimary.OnData = data => Console.Write($"/dev/ttyUSB0:\t{data}");
            // listeners.Add(listenerPrimary);

            // var listenerGps = new SerialPortListener("/dev/ttyUSB1", 9600, "\n");
            // //listenerGps.OnData = data => Console.Write($"/dev/ttyUSB1:\t{data}");
            // listeners.Add(listenerGps);

            foreach (var listener in listeners)
            {
                listener.Start();
            }

            while (true)
            {
                Thread.Sleep(500);
            }
        }
    }

    public class MaiiSystemInfo
    {
        public MaiiSystemPriority Priority { get; set; }

        public string Port { get; set; }

        public int PortBaudRate { get; set; }

        public string SourcePath { get; set; }
    }

    public enum MaiiSystemPriority 
    {
        Primary,        
        Secondary
    }

    public class SystemProgrammer 
    {
        public void Upload(MaiiSystemInfo system) 
        {
            var platformioCommand = $"platformio run --project-dir {system.SourcePath} --target upload --upload-port {system.Port}";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "/bin/bash";
            psi.Arguments = $"-c \"{platformioCommand}\"";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = false;
            Process proc = new Process
            {
                StartInfo = psi
            };
            proc.Start();
            while(!proc.StandardOutput.EndOfStream) {
                Console.WriteLine("O: " + proc.StandardOutput.ReadLine());
                if (proc.StandardError.Peek() > 0) {
                    Console.WriteLine("E: " + proc.StandardError.ReadLine());
                }
            }
        }
    }

    public class SerialPortListener : ISerialPortListener
    {
        private ISerialPort port;
        private string delimiter;
        private Action<string> onDataAction;
        private string dataBuffer = string.Empty;

        public ISerialPort Port => this.port;

        public string Delimiter
        {
            get => this.delimiter;
            set => this.delimiter = value;
        }

        public Action<string> OnData
        {
            get => this.onDataAction;
            set => this.onDataAction = value;
        }


        public SerialPortListener(string portName, int baudRate, string delimiter)
        {
            this.delimiter = delimiter;
            this.port = new SerialPort();
            this.port.BaudRate = baudRate;
            this.port.PortName = portName;
            this.port.DataReceived += this.PortDataReceived;
        }


        public void Start()
        {
            this.port.Open();
        }

        private void PortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = this.port.ReadExisting();
            this.dataBuffer += data;

            // Check if data contains delimiter and raise action if it does
            var delimiterIndex = this.dataBuffer.IndexOf(this.delimiter);
            if (delimiterIndex >= 0)
            {
                var interestingData = this.dataBuffer.Substring(0, delimiterIndex + this.delimiter.Length);
                this.dataBuffer = this.dataBuffer.Substring(delimiterIndex + this.delimiter.Length);
                this.onDataAction?.Invoke(interestingData);
            }
        }
    }

    public interface ISerialPortListener
    {
        ISerialPort Port { get; }

        string Delimiter { get; set; }

        Action<string> OnData { get; set; }

        void Start();
    }

    public interface ISerialPort : IDisposable
    {
        int BaudRate { get; set; }

        string PortName { get; set; }

        bool IsOpen { get; set; }

        string ReadExisting();

        void Open();

        event SerialDataReceivedEventHandler DataReceived;

        void Close();
    }
    public class SerialDataReceivedEventArgs : EventArgs
    {
    }

    public delegate void SerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e);

    public class SerialPort : ISerialPort
    {
        private static int OPEN_READ_WRITE = 2;

        private static int SERIAL_BUFFER_SIZE = 64;

        private byte[] serialDataBuffer = new byte[SERIAL_BUFFER_SIZE];

        [DllImport("libc", EntryPoint = "open")]
        public static extern int Open(string fileName, int mode);

        [DllImport("libc", EntryPoint = "close")]
        public static extern int Close(int handle);

        [DllImport("libc", EntryPoint = "read")]
        public static extern int Read(int handle, byte[] data, int length);

        [DllImport("libc", EntryPoint = "tcgetattr")]
        public static extern int GetAttribute(int handle, [Out] byte[] attributes);

        [DllImport("libc", EntryPoint = "tcsetattr")]
        public static extern int SetAttribute(int handle, int optionalActions, byte[] attributes);

        [DllImport("libc", EntryPoint = "cfsetspeed")]
        public static extern int SetSpeed(byte[] attributes, int baudrate);

        public int BaudRate { get; set; }

        public string PortName { get; set; }

        public bool IsOpen { get; set; }

        public event SerialDataReceivedEventHandler DataReceived;

        public async void Open()
        {
            int handle = Open(this.PortName, OPEN_READ_WRITE);

            if (handle == -1)
            {
                throw new Exception($"Could not open port ({this.PortName})");
            }

            SetBaudRate(handle);

            await Task.Delay(2000);

            var action = Task.Run(() => StartReading(handle));
        }

        public void Close()
        {
            Dispose(true);
        }

        public string ReadExisting()
        {
            return Encoding.UTF8.GetString(serialDataBuffer);
        }

        public static string[] GetPortNames()
        {
            try
            {
                var ports = new List<string>();

                string[] ttyPorts = Directory.GetFiles("/dev/", "tty*");
                foreach (string port in ttyPorts)
                {
                    if (port.StartsWith("/dev/ttyS") || port.StartsWith("/dev/ttyUSB") ||
                        port.StartsWith("/dev/ttyACM") || port.StartsWith("/dev/ttyAMA"))
                    {
                        ports.Add(port);
                    }
                }

                return ports.ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                IsOpen = false;

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

        private void SetBaudRate(int handle)
        {
            byte[] terminalData = new byte[256];

            GetAttribute(handle, terminalData);
            SetSpeed(terminalData, this.BaudRate);
            SetAttribute(handle, 0, terminalData);
        }

        private async void StartReading(int handle)
        {
            while (true)
            {
                Array.Clear(serialDataBuffer, 0, serialDataBuffer.Length);

                int lengthOfDataInBuffer = Read(handle, serialDataBuffer, SERIAL_BUFFER_SIZE);

                if (lengthOfDataInBuffer != -1 && !(lengthOfDataInBuffer == 1 && serialDataBuffer[0] == 10))
                {
                    DataReceived.Invoke(this, new SerialDataReceivedEventArgs());
                }

                await Task.Delay(50);
            }
        }
    }

}
