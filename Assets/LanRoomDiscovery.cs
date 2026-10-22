using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

// LAN-only rendezvous. Room codes are convenience identifiers, not account credentials.
public sealed class LanRoomDiscovery : IDisposable {
 const string Query="GAMEOVER_ROOM_2|",Reply="GAMEOVER_FOUND_2|";
 UdpClient socket;bool hosting;string nonce;int port;float elapsed,nextQuery,firstReply=-1;
 readonly HashSet<string> candidates=new HashSet<string>();
 readonly List<IPAddress> targets=new List<IPAddress>();
 public string Code{get;private set;} public string Address{get;private set;} public string Error{get;private set;}
 public static bool ValidCode(string code){if(code==null||code.Length!=6)return false;foreach(char c in code)if(c<'0'||c>'9')return false;return true;}
 public static string GenerateCode(){using(var rng=RandomNumberGenerator.Create()){var b=new byte[4];uint value;do{rng.GetBytes(b);value=BitConverter.ToUInt32(b,0);}while(value>=4294000000u);return (value%1000000).ToString("D6");}}
 public void Host(int discoveryPort){try{hosting=true;port=discoveryPort;Code=GenerateCode();socket=new UdpClient(new IPEndPoint(IPAddress.Any,port));socket.EnableBroadcast=true;}catch(Exception e){Fail("Room creation failed: "+e.Message);}}
 public void Find(string code,int discoveryPort,IPAddress testTarget=null){
  if(!ValidCode(code)){Fail("Enter the host's six-digit room code.");return;}
  try{Code=code;port=discoveryPort;nonce=Guid.NewGuid().ToString("N");socket=new UdpClient(new IPEndPoint(IPAddress.Any,0));socket.EnableBroadcast=true;if(testTarget!=null)targets.Add(testTarget);else DiscoverTargets();}catch(Exception e){Fail("Room search failed: "+e.Message);}
 }
 // No subnet scanning: only broadcast endpoints and directly attached private gateways.
 public static IPAddress SubnetBroadcast(IPAddress address,IPAddress mask){
  if(address==null||mask==null||address.AddressFamily!=AddressFamily.InterNetwork||mask.AddressFamily!=AddressFamily.InterNetwork)return null;
  var a=address.GetAddressBytes();var m=mask.GetAddressBytes();var b=new byte[4];int ones=0;bool zero=false;
  for(int i=0;i<4;i++)for(int bit=7;bit>=0;bit--){bool set=(m[i]&(1<<bit))!=0;if(set&&zero)return null;if(set)ones++;else zero=true;}
  if(ones<1||ones>30)return null;
  for(int i=0;i<4;i++)b[i]=(byte)(a[i]|(~m[i]&255));return new IPAddress(b);
 }
 static bool Private(IPAddress address){if(address==null||address.AddressFamily!=AddressFamily.InterNetwork)return false;var b=address.GetAddressBytes();return b[0]==10||(b[0]==172&&b[1]>=16&&b[1]<=31)||(b[0]==192&&b[1]==168);}
 void AddTarget(IPAddress address){if(address!=null&&!targets.Contains(address)&&targets.Count<24)targets.Add(address);}
 void DiscoverTargets(){
  AddTarget(IPAddress.Broadcast);
  try{foreach(var adapter in NetworkInterface.GetAllNetworkInterfaces()){
   if(adapter.OperationalStatus!=OperationalStatus.Up||adapter.NetworkInterfaceType==NetworkInterfaceType.Loopback||adapter.NetworkInterfaceType==NetworkInterfaceType.Tunnel)continue;
   try{var properties=adapter.GetIPProperties();
    foreach(var entry in properties.UnicastAddresses)if(Private(entry.Address))AddTarget(SubnetBroadcast(entry.Address,entry.IPv4Mask));
    foreach(var gateway in properties.GatewayAddresses)if(Private(gateway.Address))AddTarget(gateway.Address);
   }catch{/* Android/runtime adapters may not expose all IP properties. */}
  }}catch{/* Keep general broadcast available if interface enumeration is unsupported. */}
 }
 public void Poll(float dt){
  if(socket==null||Error!=null||Address!=null)return;
  try{
   elapsed+=Math.Max(0,Math.Min(dt,.25f));
   if(!hosting&&elapsed>=nextQuery){nextQuery=elapsed+.6f;var data=Encoding.ASCII.GetBytes(Query+Code+"|"+nonce);foreach(var target in targets){try{socket.Send(data,data.Length,new IPEndPoint(target,port));}catch(SocketException){/* One unavailable adapter must not abort the other routes. */}}}
   // Bound work even on a busy/broadcast-heavy network.
   for(int i=0;i<32&&socket.Available>0;i++){
    IPEndPoint sender=new IPEndPoint(IPAddress.Any,0);var data=socket.Receive(ref sender);if(data.Length>160)continue;
    string text=Encoding.ASCII.GetString(data);var parts=text.Split('|');
    if(hosting){if(parts.Length==3&&parts[0]=="GAMEOVER_ROOM_2"&&parts[1]==Code&&parts[2].Length==32){var reply=Encoding.ASCII.GetBytes(Reply+Code+"|"+parts[2]);socket.Send(reply,reply.Length,sender);}}
    else if(parts.Length==3&&parts[0]=="GAMEOVER_FOUND_2"&&parts[1]==Code&&parts[2]==nonce&&sender.Port==port){candidates.Add(sender.Address.ToString());if(firstReply<0)firstReply=elapsed;}
   }
   if(!hosting){
    if(candidates.Count>1){Fail("Two rooms share that code. Ask the host to create a new room.");return;}
    if(firstReply>=0&&elapsed-firstReply>=.8f){foreach(var address in candidates)Address=address;return;}
    if(elapsed>=12)Fail("Room not found. Keep the host open and check the code. Use the same Wi-Fi or connect to the host phone hotspot; guest networks may block discovery.");
   }
  }catch(Exception e){Fail("Room discovery stopped: "+e.Message);}
 }
 void Fail(string message){Error=message;Dispose();}
 public void Dispose(){if(socket!=null)socket.Close();socket=null;}
}
