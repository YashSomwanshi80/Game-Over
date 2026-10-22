using UnityEditor;

// .NET LAN sockets also need Android's normal INTERNET permission.
[InitializeOnLoad]
public static class MultiplayerBuildSettings
{
    static MultiplayerBuildSettings(){PlayerSettings.Android.forceInternetPermission=true;}
}
