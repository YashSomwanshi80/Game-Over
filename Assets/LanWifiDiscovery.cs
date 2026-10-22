using System;
using UnityEngine;
// Hold only while discovering a room; release once TCP connects or the match is destroyed.
public sealed class LanWifiDiscovery : IDisposable {
#if UNITY_ANDROID && !UNITY_EDITOR
 AndroidJavaObject wifiLock;
#endif
 public LanWifiDiscovery(){
#if UNITY_ANDROID && !UNITY_EDITOR
  try{using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))using(var context=activity.Call<AndroidJavaObject>("getApplicationContext"))using(var wifi=context.Call<AndroidJavaObject>("getSystemService","wifi")){
   wifiLock=wifi.Call<AndroidJavaObject>("createMulticastLock","GAMEOVER room discovery");wifiLock.Call("setReferenceCounted",false);wifiLock.Call("acquire");
  }}catch(Exception e){Debug.LogWarning("Wi-Fi discovery lock unavailable: "+e.Message);Dispose();}
#endif
 }
 public void Dispose(){
#if UNITY_ANDROID && !UNITY_EDITOR
  if(wifiLock==null)return;try{if(wifiLock.Call<bool>("isHeld"))wifiLock.Call("release");}catch(Exception e){Debug.LogWarning("Wi-Fi discovery lock release: "+e.Message);}finally{wifiLock.Dispose();wifiLock=null;}
#endif
 }
}
