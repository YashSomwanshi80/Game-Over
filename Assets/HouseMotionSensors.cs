using UnityEngine;
// Informational alarms, derived from actor positions already replicated by the host.
public sealed class HouseMotionSensors : MonoBehaviour {
 public static readonly Vector3[] Points={new Vector3(-6.8f,5.95f,4.7f),new Vector3(5.7f,5.95f,4.7f),new Vector3(-1.4f,-.85f,.3f),new Vector3(5.8f,2.6f,1.5f)};
 AudioSource[] alarms;bool[] inside;float[] ready;bool suspended;int activeAlarm=-1;float alarmStarted;
 public void Build(){alarms=new AudioSource[Points.Length];inside=new bool[Points.Length];ready=new float[Points.Length];var clip=Resources.Load<AudioClip>("Presentation/Audio/MotionSensor");for(int i=0;i<Points.Length;i++){var go=new GameObject("Discrete motion sensor / "+i);go.transform.SetParent(transform,false);go.transform.position=Points[i];var source=go.AddComponent<AudioSource>();source.clip=clip;source.playOnAwake=false;source.loop=false;source.spatialBlend=1;source.minDistance=1.5f;source.maxDistance=7.5f;source.rolloffMode=AudioRolloffMode.Linear;source.volume=.7f;source.dopplerLevel=0;alarms[i]=source;}}
 public static bool InRange(Vector3 sensor,Vector3 feet){var eye=feet+Vector3.up*1.4f;var delta=eye-sensor;return Mathf.Abs(delta.y)<1.5f&&new Vector2(delta.x,delta.z).sqrMagnitude<2.25f;}
 bool Sees(int index,Vector3 feet){if(!InRange(Points[index],feet))return false;Vector3 delta=feet+Vector3.up*1.4f-Points[index];foreach(var hit in Physics.RaycastAll(Points[index],delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore)){if(hit.collider is CharacterController)continue;if(hit.distance<delta.magnitude-.3f)return false;}return true;}
 public void Tick(ThrillerSoundscape.State s,bool active){
  if(alarms==null)return;
  if(!s.powered||s.jammed||s.won||s.lost){foreach(var a in alarms)a.Stop();activeAlarm=-1;for(int i=0;i<inside.Length;i++)inside[i]=false;return;}
  if(!active){Suspend();return;}if(suspended){foreach(var a in alarms)a.UnPause();suspended=false;}
  if(activeAlarm>=0){float age=s.time-alarmStarted;alarms[activeAlarm].volume=.7f*Mathf.Clamp01((3-age)/.35f);if(age>=3||!alarms[activeAlarm].isPlaying){alarms[activeAlarm].Stop();activeAlarm=-1;}}
  if(!s.actors)return;int selected=-1;float nearest=float.MaxValue;
  for(int i=0;i<alarms.Length;i++){
   bool occupied=(!s.sensorTapseeHidden&&Sees(i,s.tapsee))||Sees(i,s.don);
   if(occupied&&!inside[i]&&s.time>=ready[i]){ready[i]=s.time+6;float distance=(s.position-Points[i]).sqrMagnitude;if(distance<nearest){selected=i;nearest=distance;}}
   inside[i]=occupied;
  }
  // Consume concurrent triggers; no delayed queue of overlapping alarm recordings.
  if(activeAlarm<0&&selected>=0){activeAlarm=selected;alarmStarted=s.time;alarms[selected].volume=.7f;alarms[selected].Play();}
 }
 public void Suspend(){if(alarms==null||suspended)return;foreach(var a in alarms)a.Pause();suspended=true;}
 void OnDisable(){if(alarms!=null)foreach(var a in alarms)if(a!=null)a.Stop();}
}
