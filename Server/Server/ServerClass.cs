using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ServerClass
{
    private Socket sListener;
    private SocketPermission permission;
     IPHostEntry ipHost = Dns.GetHostEntry("");
     IPAddress ipAddr = ipHost.AddressList[0];
    ipEndPoint = new IPEndPoint(ipAddr, 4510);

    public ServerClass()
	{
        
        //socket needs permission to work, because it will use a closed port number. 
        //A window will appear demanding permission to allow sending data.
        permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", SocketPermission.AllPorts);
        sListener = new Socket(IPAddress.Any, SocketType.Stream, ProtocolType.Tcp);

    }
}
