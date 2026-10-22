using UnityEngine;
// Two bounded exterior lights, synchronized to the authoritative rescue countdown.
public sealed class PoliceArrivalLights : MonoBehaviour {
 Light red,blue;
 public void Build(){red=Make("Police arrival / red",new Vector3(-3,2.2f,-16.5f),new Vector3(-4,2,-6),new Color(1,.015f,.01f));blue=Make("Police arrival / blue",new Vector3(3,2.2f,-16.5f),new Vector3(4,2,-6),new Color(.015f,.12f,1));}
 Light Make(string name,Vector3 at,Vector3 target,Color color){var go=new GameObject(name);go.transform.SetParent(transform,false);go.transform.position=at;go.transform.LookAt(target);var l=go.AddComponent<Light>();l.type=LightType.Spot;l.spotAngle=110;l.innerSpotAngle=55;l.range=30;l.color=color;l.intensity=0;l.enabled=false;l.shadows=Application.isMobilePlatform?LightShadows.None:LightShadows.Hard;l.shadowBias=.03f;l.shadowNormalBias=.1f;return l;}
 public static float Strength(bool called,float remaining)=>called&&remaining>0&&remaining<=10?Mathf.Lerp(.2f,4,1-Mathf.Clamp01(remaining/10)):0;
 public void Present(ThrillerSoundscape.State s,bool active){float strength=active?Strength(s.called,s.rescue):0;if(strength<=0){Off();return;}bool reduced=GamePreferences.ReducedLightning;float phase=Mathf.Sin(s.time*Mathf.PI*1.6f);red.enabled=blue.enabled=true;red.intensity=strength*(reduced?.22f:.08f+.92f*Mathf.SmoothStep(0,1,Mathf.Clamp01(phase)));blue.intensity=strength*(reduced?.22f:.08f+.92f*Mathf.SmoothStep(0,1,Mathf.Clamp01(-phase)));}
 public void Off(){if(red!=null){red.enabled=false;red.intensity=0;}if(blue!=null){blue.enabled=false;blue.intensity=0;}}
 void OnDisable(){Off();}void OnApplicationPause(bool paused){if(paused)Off();}void OnApplicationFocus(bool focused){if(!focused)Off();}
}
