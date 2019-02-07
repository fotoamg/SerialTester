using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;


namespace SerialWriter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SerialPort port;
        private FormData formData;
        private StringBuilder stringBuilder = new StringBuilder(200);
        private long lastMillis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        public MainWindow()
        {
            InitializeComponent();
            formData = new FormData();
            this.DataContext = formData;
            var portList = SerialPort.GetPortNames();
            formData.PortList = portList.Select(x => new PortData {Name = x}).ToArray();
            formData.Delay = 200;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
 
            var message = String.Format("{0}\n", messageTextBox.Text);
            SendMessageToPort(message);
            this.Dispatcher.Invoke(() =>
            {
                messageTextBox.Clear();
            });


        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ReadPort(port);
        }

        private void SendMessageToPort(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            if (port != null && port.IsOpen)
            {
                port.Write(data, 0, data.Length);
                this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                (UpdateTheUI)delegate (string text)
                {
                    fileMessageTextBox.Text = text;
                    fileMessageTextBox.ScrollToEnd();
                }, message);
            }
            else
            {
                MessageBox.Show("Port is not opened.");
            }

        }

        

        private void ReadPort(SerialPort readPort)
        {
            if (readPort.BytesToRead > 0)
            {
                var bufferSize = readPort.BytesToRead;
                if (bufferSize > 80)
                {
                    bufferSize = 80;
                }
                //Buffer with data
                byte[] readData = new byte[bufferSize];

                //Handle data
                readPort.Read(readData, 0, readData.Length);
                var reply = Encoding.ASCII.GetString(readData);
                stringBuilder.Append(reply);
                if (stringBuilder.Length > 190 || '\n'.Equals(reply) || '\r'.Equals(reply)
                    || (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) > (lastMillis + 8000))
                {
                    this.Dispatcher.BeginInvoke(
                       System.Windows.Threading.DispatcherPriority.Normal,
                       (UpdateTheUI)delegate (string text)
                       {   replyTextBox.Text += text;
                           replyTextBox.ScrollToEnd();
                       }, stringBuilder.ToString());
                    stringBuilder.Clear();
                    lastMillis = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                }
            }
                
            if (readPort.BytesToRead > 0) ReadPort(readPort);

        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            //formData.Stop = false;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                formData.FileName = openFileDialog.FileName;
                this.Dispatcher.Invoke(() =>
                {
                    fileNameTextBox.Text = formData.FileNotPath;
                   fileNameTextBox.ScrollToEnd();
                }); 
                //Thread thrd = new Thread(() => WriteReply(openFileDialog.FileName));
                //thrd.Start();
            }

        }

        private void WriteReply(string fileName)
        {
            int delay = formData.Delay;
            var line = "";
            System.IO.StreamReader file =
                new System.IO.StreamReader(fileName);
            var lineCounter = 0;
            var maxLine = 2;
            var message = String.Empty;
            while ((line = file.ReadLine()) != null && !formData.Stop)
            {
                message += String.Format("{0}\n", line);
                if (lineCounter < maxLine - 1)
                {
                    lineCounter++;
                }
                else
                {
                    lineCounter = 0;
                    do {
                        Thread.Sleep(delay);
                        SendMessageToPort(message);
                    } while (formData.IsRepeat == true);

                    message = "";
                }
            }
            if (!String.IsNullOrEmpty(message))
            {
                Thread.Sleep(delay);
                SendMessageToPort(message);
            }


            file.Close();

        }

        delegate void UpdateTheUI(string newText);


        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            fileMessageTextBox.Clear();
            replyTextBox.Clear();
        }

        private void portComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var defaultPortName = "COM5";
            if (port != null && port.IsOpen)
            {
                port.Close();
            }

                port = new SerialPort(portComboBox.SelectedValue.ToString());
                port.BaudRate = 9600;
                port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                port.Open();

        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            formData.Stop = true;
        }

        private void replyButton_Click(object sender, RoutedEventArgs e)
        {
            formData.Stop = false;
            Thread thrd = new Thread(() => WriteReply(formData.FileName));
            thrd.Start();
        }

 
    }


}
