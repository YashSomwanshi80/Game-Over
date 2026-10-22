using System.IO;
using System.Xml;
using UnityEditor.Android;
// Merge normal Wi-Fi permissions into Unity's generated library manifest, without replacing its activity.
public sealed class LanRoomAndroidPermissions : IPostGenerateGradleAndroidProject {
 public int callbackOrder=>20;
 public void OnPostGenerateGradleAndroidProject(string path){AddPermissions(Path.Combine(path,"src/main/AndroidManifest.xml"));}
 public static void AddPermissions(string path){
  const string ns="http://schemas.android.com/apk/res/android";
  var xml=new XmlDocument();xml.Load(path);var root=xml.DocumentElement;
  foreach(string permission in new[]{"android.permission.ACCESS_WIFI_STATE","android.permission.CHANGE_WIFI_MULTICAST_STATE"}){
   bool found=false;foreach(XmlNode node in root.ChildNodes)if(node.Name=="uses-permission"&&node.Attributes["name",ns]?.Value==permission)found=true;
   if(found)continue;var item=xml.CreateElement("uses-permission");var name=xml.CreateAttribute("android","name",ns);name.Value=permission;item.Attributes.Append(name);root.AppendChild(item);
  }
  xml.Save(path);
 }
}
