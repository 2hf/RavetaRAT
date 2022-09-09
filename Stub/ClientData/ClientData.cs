using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Stub
{
    public class ClientData
    {
        public ClientConnectionsListObject DataObject;
        public TcpClient Client;
    }
	public class ClientConnectionsListObject
	{
		public string ClientGroup { get; set; }

		public string Country { get; set; }
		public string PCName { get; set; }

		public string IPAddress { get; set; }

		public string HardwareID { get; set; }
	}
}
