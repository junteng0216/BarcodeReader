using System.Net.Sockets;
using System.Text;
using System.IO.Ports;

namespace SmartCodeReader
{
    public class CodeReader
    {
        public (bool result, Exception? ex) tcpConnect(string serverIP,int commandPort)
        {
            try
            {
                TcpClient tcpClient = new TcpClient(serverIP, commandPort);
                    if (tcpClient.Connected)
                    {
                        string dataToSend = "<Set,Acq,1>";
                        byte[] sendData = Encoding.ASCII.GetBytes(dataToSend);
                        NetworkStream stream = tcpClient.GetStream();
                        stream.Write(sendData, 0, sendData.Length);
                        return (true, null);
                    }
                    return (false, null);
                
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }

        public (bool result, Exception? ex) tcpDisconnect(string serverIP, int commandPort)
        {
            try
            {
                // Establish a TCP connection to stop acquisition
                TcpClient tcpClient = new TcpClient(serverIP, commandPort);
                if (tcpClient.Connected)
                {
                    string dataToSend = "<Set,Acq,0>";
                    byte[] sendData = Encoding.ASCII.GetBytes(dataToSend);
                    NetworkStream stream = tcpClient.GetStream();
                    stream.Write(sendData, 0, sendData.Length);
                    return (true, null);
                }
                return (false, null);
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }

        public (string? result, Exception? ex) tcpStart(string serverIP, int serverPort)
        {
            string ans = "";
            try
            {
                TcpClient tcpClient2 = new TcpClient(serverIP, serverPort);
                if (tcpClient2.Connected)
                {
                    // Send data to the server
                    string dataToSend = "start";
                    byte[] sendData = Encoding.ASCII.GetBytes(dataToSend);
                    NetworkStream stream = tcpClient2.GetStream();
                    stream.Write(sendData, 0, sendData.Length);

                    // Receive data from the server
                    byte[] receiveData = new byte[1024];
                    int bytesRead = stream.Read(receiveData, 0, receiveData.Length);
                    string receivedData = Encoding.ASCII.GetString(receiveData, 0, bytesRead);
                    //txtReceive.Text += receivedData;
                    foreach (char c in receivedData)
                    {
                        if (c == ';')
                            ans += "\nAngle: ";
                        else
                            ans += c;
                    }
                    return (ans, null);
                }
                else
                {
                    return (null, null);
                }
            }
            catch (Exception ex)
            {
                return (null, ex);
            }

        }

        public (bool result, Exception? ex) serialConnect(SerialPort serialPort)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
                serialPort.Open();
                if (serialPort.IsOpen)
                {
                    serialPort.Write("<Set,Acq,1>");
                    return (true, null);
                }
                return (false, null);

            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }

        public (bool result, Exception? ex) serialDisconnect(SerialPort serialPort)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Write("<Set,Acq,0>");
                    serialPort.Close();
                    return (true, null);
                }
                return (false, null);

            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }

        public (bool result, Exception? ex) serialStart(SerialPort serialPort)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Write("start");
                    return (true, null);
                }
                return (false, null);

            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }

        public (string? result, Exception? ex) serialReceive(SerialPort serialPort)
        {
            try
            {
                if (serialPort.BytesToRead > 0)
                {
                    byte[] data = new byte[serialPort.BytesToRead];
                    var ans = serialPort.Read(data, 0, data.Length);
                    string myans = Encoding.ASCII.GetString(data);
                    return (myans, null);
                }
                return (null, null);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }
    }
}
