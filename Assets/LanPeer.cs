using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Net.NetworkInformation;
using System.Collections.Generic;

// One TCP peer, bounded newline records. Only the Unity thread consumes messages.
public sealed class LanPeer : IDisposable
{
    TcpListener listener;TcpClient client;IAsyncResult connecting;
    readonly byte[] bytes=new byte[4096];
    // Decode only complete records: TCP can split a UTF-8 character between reads.
    readonly byte[] recordBytes=new byte[32768];int recordLength;
    static readonly Encoding RecordEncoding=new UTF8Encoding(false,true);
    public bool Connected{get;private set;}
    public string Error{get;private set;}
    public Action<string> Message;
    float idle;LanRoomDiscovery discovery;
    public string RoomCode{get;private set;}
    public void HostRoom(int port){Host(port);if(Error!=null)return;discovery=new LanRoomDiscovery();discovery.Host(port+1);RoomCode=discovery.Code;if(discovery.Error!=null)Fail(discovery.Error);}
    public void JoinRoom(string code,int port,IPAddress discoveryTarget=null){roomPort=port;discovery=new LanRoomDiscovery();discovery.Find(code,port+1,discoveryTarget);if(discovery.Error!=null)Fail(discovery.Error);}
    int roomPort;
    public static string LocalAddresses()
    {
        var addresses=new List<string>();
        try{foreach(var adapter in NetworkInterface.GetAllNetworkInterfaces())if(adapter.OperationalStatus==OperationalStatus.Up)
            foreach(var entry in adapter.GetIPProperties().UnicastAddresses)if(entry.Address.AddressFamily==AddressFamily.InterNetwork&&!IPAddress.IsLoopback(entry.Address))addresses.Add(entry.Address.ToString());}
        catch{return "Check the host device's Wi-Fi IP in network settings.";}
        return addresses.Count==0?"No LAN address detected. Connect both devices to the same Wi-Fi.":string.Join(" / ",addresses.ToArray());
    }
    public void Host(int port)
    {
        try{listener=new TcpListener(IPAddress.Any,port);listener.Start(1);}catch(Exception e){Fail(e.Message);}
    }
    public void Join(string address,int port)
    {
        IPAddress ip;
        if(!IPAddress.TryParse(address,out ip)){Fail("Enter the host's numeric local IP address.");return;}
        try{client=new TcpClient();connecting=client.BeginConnect(ip,port,null,null);}catch(Exception e){Fail(e.Message);}
    }
    public void Poll(float dt)
    {
        if(Error!=null)return;
        try {
            if(discovery!=null){discovery.Poll(dt);if(discovery.Error!=null){Fail(discovery.Error);return;}if(discovery.Address!=null){string address=discovery.Address;discovery.Dispose();discovery=null;Join(address,roomPort);if(Error!=null)return;}}
            if(listener!=null&&!Connected&&listener.Pending()){client=listener.AcceptTcpClient();listener.Stop();listener=null;Ready();}
            if(connecting!=null){idle+=dt;if(connecting.IsCompleted){client.EndConnect(connecting);connecting=null;Ready();}else if(idle>12){Fail("Connection timed out. Keep the host room open on the same Wi-Fi.");return;}}
            if(!Connected)return;
            idle+=dt;if(idle>12){Fail("Connection lost: no data from the other player.");return;}
            var stream=client.GetStream();
            int budget=16384;
            while(stream.DataAvailable&&budget>0) {
                int count=stream.Read(bytes,0,Math.Min(bytes.Length,budget));if(count==0){Fail("The other player disconnected.");return;}
                budget-=count;idle=0;
                for(int i=0;i<count;i++) {
                    if(bytes[i]!=10) {
                        if(recordLength==recordBytes.Length){Fail("Network message exceeded the size limit.");return;}
                        recordBytes[recordLength++]=bytes[i];continue;
                    }
                    string record=RecordEncoding.GetString(recordBytes,0,recordLength);recordLength=0;
                    if(record.Length>8192){Fail("Invalid network record.");return;}
                    Message?.Invoke(record);
                    // A handler can reject a protocol version and dispose the connection.
                    if(!Connected||Error!=null)return;
                }
            }
            if(client.Client.Poll(0,SelectMode.SelectRead)&&client.Client.Available==0)Fail("The other player disconnected.");
        }catch(Exception e){Fail("Connection closed: "+e.Message);}
    }
    void Ready(){if(discovery!=null){discovery.Dispose();discovery=null;}Connected=true;idle=0;client.NoDelay=true;client.SendTimeout=100;}
    public void Send(string message)
    {
        if(!Connected||Error!=null)return;
        if(message==null||message.Length>8192||message.IndexOf('\n')>=0){Fail("Outgoing record too large.");return;}
        try{var data=RecordEncoding.GetBytes(message+"\n");client.GetStream().Write(data,0,data.Length);}catch(Exception e){Fail("Send failed: "+e.Message);}
    }
    void Fail(string reason){Error=reason;Dispose();}
    public void Dispose(){if(discovery!=null){discovery.Dispose();discovery=null;}recordLength=0;Connected=false;connecting=null;if(client!=null)client.Close();if(listener!=null)listener.Stop();client=null;listener=null;}
}
