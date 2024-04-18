using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO.Ports;
using System.Net.Sockets;
using System;
using System.Net;
using SmartCodeReader;

namespace BarcodeReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort serialPort = new SerialPort();
        string serverIP = "192.168.2.10";
        int serverPort = 2005;
        int commandPort = 4001;
        CodeReader cr = new CodeReader();

        // Create delegates
        public delegate (bool, Exception?) SerialDelegate(SerialPort serialPort);
        public delegate (string?, Exception?) SerialReceiveDelegate(SerialPort serialPort);

        public delegate (bool, Exception?) TcpDelegate(string serverIP, int serverPort);
        public delegate (string?, Exception?) TcpReceiveDelegate(string serverIP, int serverPort);

        // Set up delegates
        SerialDelegate? serialDelegate;
        SerialReceiveDelegate? serialReceiveDelegate;

        TcpDelegate? tcpDelegate;
        TcpReceiveDelegate? tcpReceiveDelegate;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            pathCombo.Items.Add("RS232");
            pathCombo.Items.Add("TCP");
            portCombo.SelectedIndex = -1;
        }

        private async void btnConn_Click(object sender, RoutedEventArgs e) //Establish a connection to the port
        {
            try
            {
                if (pathCombo.SelectedItem.ToString() == "RS232")
                {
                    // Open port and start acquisition
                    serialPort.BaudRate = 9600;
                    serialPort.Parity = Parity.None;
                    serialPort.DataBits = 8;
                    serialPort.StopBits = StopBits.One;
                    serialPort.Handshake = Handshake.None;
                    serialPort.DtrEnable = true;
                    serialPort.RtsEnable = true;
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                    serialPort.PortName = portCombo.Text;
                    serialDelegate = cr.serialConnect;
                    var con = serialDelegate(serialPort);
                    if (con.Item1)
                    {
                        MessageBox.Show("The port is now opened successfully.");
                    }
                    else
                    {
                        if (con.Item2 != null)
                        {
                            MessageBox.Show(con.Item2.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else if(pathCombo.SelectedItem.ToString() == "TCP")
                {
                    // Add a loader here
                    Dispatcher.Invoke(() => { txtLoad.Visibility = Visibility.Visible; });
                    // Establish a TCP connection to start acquisition
                    await Task.Run(() =>
                    {
                        tcpDelegate = cr.tcpConnect;
                        var con = tcpDelegate(serverIP, commandPort);
                        Dispatcher.Invoke(() => { txtLoad.Visibility = Visibility.Collapsed; });

                        if (con.Item1)
                        {
                            MessageBox.Show("TCP connection is established successfully.");
                        }
                        else
                        {
                            if (con.Item2 != null)
                            {
                                MessageBox.Show(con.Item2.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    });
                }
            }
            catch(Exception ex)
            { 
                MessageBox.Show(ex.Message,"Message",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDisconn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (pathCombo.SelectedItem.ToString() == "RS232")
                {
                    // Stop acquisition and close port
                    serialDelegate = cr.serialDisconnect;
                    var con = serialDelegate(serialPort);
                    if (con.Item1)
                    {
                        MessageBox.Show("The port is closed successfully.");
                    }
                    else
                    {
                        if (con.Item2 != null)
                        {
                            MessageBox.Show(con.Item2.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else if (pathCombo.SelectedItem.ToString() == "TCP")
                {
                    // Establish a TCP connection to stop acquisition
                    tcpDelegate = cr.tcpDisconnect;
                    var con = tcpDelegate(serverIP, commandPort);
                    if (con.Item1)
                    {
                        MessageBox.Show("TCP connection is closed successfully.");
                    }
                    else
                    {
                        if (con.Item2 != null)
                        {
                            MessageBox.Show(con.Item2.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtReceive.Text != "")
                    txtReceive.Text += "\n";

                if (pathCombo.SelectedItem.ToString() == "RS232")
                {
                    //CodeReader cr = new CodeReader();
                    serialDelegate = cr.serialStart;
                    var con = cr.serialStart(serialPort);
                    if (con.ex != null)
                    {
                        MessageBox.Show(con.ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else if (pathCombo.SelectedItem.ToString() == "TCP")
                {
                    // Establish a TCP connection to trigger scan
                    tcpReceiveDelegate = cr.tcpStart;
                    var con = tcpReceiveDelegate(serverIP, serverPort);
                    if (con.Item1 != null)
                    {
                        foreach (char c in con.Item1)
                        {
                            if (c == ';')
                                txtReceive.Text += "\nAngle: ";
                            else
                                txtReceive.Text += c;
                        }
                    }
                    else
                    {
                        if (con.Item2 != null)
                        {
                            MessageBox.Show(con.Item2.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                txtReceive.ScrollToLine(txtReceive.LineCount - 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //CodeReader cr = new CodeReader();
            serialReceiveDelegate = cr.serialReceive;
            var con = serialReceiveDelegate(serialPort);
            if (con.Item1 != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    foreach (char c in con.Item1)
                    {
                        if (c == ';')
                            txtReceive.Text += "\nAngle: ";
                        else
                            txtReceive.Text += c;
                    }

                    //txtReceive.Text += myans;
                    txtReceive.ScrollToLine(txtReceive.LineCount - 1);
                }));
            }
            
        }

        private void pathCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pathCombo.SelectedItem.ToString() == "RS232")
            {
                string[] ports = SerialPort.GetPortNames();
                if (ports.Length > 0)
                {
                    portCombo.ItemsSource = ports;
                    portCombo.SelectedIndex = 0;
                }
                else
                {
                    portCombo.Items.Add("No Port");
                    portCombo.SelectedIndex = 0;
                }
            }
            else if (pathCombo.SelectedItem.ToString() == "TCP")
            {
                portCombo.SelectedItem = -1;
                portCombo.ItemsSource = null;
            }
        }

    }
}