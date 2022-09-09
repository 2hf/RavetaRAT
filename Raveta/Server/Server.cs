using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Windows;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Drawing;
using System.Security.Authentication;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing.Imaging;
using System.Linq;
using System.Resources;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Raveta
{
    public class Server
    {
        public static TcpListener Listener;
        public static Socket Socket;
        public static bool Active = false;
        public static List<ClientData> ClientList = new List<ClientData>();
        public static bool _NewClientAccepted = false;

        public static bool _ClientRemoved = false;
        public static int _RemovedClientIdx = -1;

        public static List<string> ClientTasksByHWID = new List<string>();

        public static void StartServer(string port)
        {
            try
            {
                Listener = new TcpListener(IPAddress.Any, int.Parse(port));
                Listener.Start();
                Socket = Listener.Server;
                Active = true;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                Active = false;
            }
        }
        public static string ParseReceivedPacket(TcpClient client)
        {

            NetworkStream stream = client.GetStream();
            Thread.Sleep(200);
            if (!client.Connected || !stream.CanRead || !stream.DataAvailable)
                return "packet could not be parsed.";

            // Parse packet msg, 5.12mb buffer
            byte[] buffer = new byte[5012000];
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
        public static ClientAccepted NewClientAccepted()
        {
            try
            {
                if (ClientList.Count <= 0)
                    return new ClientAccepted(null, false);

                ClientAccepted clientAccepted = new ClientAccepted(ClientList.ElementAt(ClientList.Count - 1), _NewClientAccepted);

                return clientAccepted;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            return new ClientAccepted(null, false);
        }
        public static ClientRemoved __ClientRemoved()
        {
            try
            {
                ClientRemoved clientRemoved = new ClientRemoved(_RemovedClientIdx, _ClientRemoved);

                return clientRemoved;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            return new ClientRemoved(-1, false);
        }
        public static ClientConnectionsListObject GetDeserializedConnectionPacket(string msg)
        {
            ClientConnectionsListObject clientConnectionsListObject = new ClientConnectionsListObject();
            clientConnectionsListObject = (ClientConnectionsListObject)new XmlSerializer(typeof(ClientConnectionsListObject)).Deserialize(new StringReader(msg));
           
      

            return clientConnectionsListObject;
        }
        public static void ManageClientAcceptance(TcpClient client)
        {
            ClientConnectionsListObject clientConnectionsListObject = new ClientConnectionsListObject();
            string msg = ParseReceivedPacket(client);

            // It couldn't be parsed.
            if (msg.Contains("packet could not be parsed."))
                return;
            

            clientConnectionsListObject = GetDeserializedConnectionPacket(msg);

            // Assuming we have received data..
            ClientData clientData = new ClientData();
            clientData.Client = client;
            clientData.DataObject = clientConnectionsListObject;

            ClientList.Add(clientData);
            _NewClientAccepted = true;
        }
        public static void PollConnection(ClientData client, int i)
        {
            while (true)
            {
    
                if (client.Client.Client.Poll(10, SelectMode.SelectRead))
                {
                    if (_RemovedClientIdx == i)
                    {
                        break;
                    }

                    NetworkStream stream = client.Client.GetStream();

                    // Something happened, this client has to go.
                    if (!stream.DataAvailable)
                    {
                        ClientList.RemoveAt(i);
                        ClientTasksByHWID.RemoveAt(i);
                        // Set these so the connections list is also handled in the main thread
                        _ClientRemoved = true;
                        _RemovedClientIdx = i;
                        break;
                    }
                    // New data is being streamed
                    else
                    {

                    }
                }
            }
        }
        public static void AddConnectionPolls()
        {
            while (true)
            {
                // Adds connection polling tasks for new clients if there are any.
                for (int i = 0; i < ClientList.Count; i++)
                {
                    ClientData client = ClientList.ElementAt(i);
                    bool taskStart = true;

                    foreach (var task in ClientTasksByHWID)
                    {
                        // This client already has an assigned task
                        if (task.Contains(client.DataObject.HardwareID))
                            taskStart = false;
                    }

                    if (taskStart)
                    {
                        Task.Run(() => PollConnection(client, i-1));
                        ClientTasksByHWID.Add(client.DataObject.HardwareID);
                    }
                }
                Thread.Sleep(10);
            }
        }
        public static void Loop()
        {
        startloop:

            try
            {
                while (true)
                {
                    if (!Active)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    TcpClient client = Listener.AcceptTcpClient();
                    Task.Run(() =>  ManageClientAcceptance(client));
                    Thread.Sleep(50);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                goto startloop;
            }
        }
        public static void DisposeListener()
        {
            try
            {
                Active = false;
                Listener.Stop();
                Socket.Dispose();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }
    }
    public struct ClientAccepted
    {
        public ClientData clientData;
        public bool Accepted;
        public ClientAccepted(ClientData _clientData, bool _Accepted)
        {
            clientData = _clientData;
            Accepted = _Accepted;
        }
    }
    public struct ClientRemoved
    {
        public int i;
        public bool Removed;
        public ClientRemoved(int _i, bool _Removed)
        {
            i = _i;
            Removed = _Removed;
        }
    }
}
