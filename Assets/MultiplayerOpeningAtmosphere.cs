using UnityEngine;

// Locally rendered from authoritative match state; no independent gameplay timer.
public sealed class MultiplayerOpeningAtmosphere : MonoBehaviour {
 public static readonly Vector3 Phone=new Vector3(-9.02f,-2.28f,5.65f);
 public static readonly Vector3 TapseeSpawn=new Vector3(5,3.46f,-3.5f);
 public static readonly Vector3 DonOutside=new Vector3(0,.08f,-9.8f);
 public static readonly Vector3 DonEntry=new Vector3(0,.08f,-5.8f);
 AudioSource ringing,breakIn,entry;bool entryPlayed,entryPaused;AudioSource lateEntry;bool latePlayed,latePaused;
 public static bool VoicePlaying {get;private set;}
 public void SetCallPoint(Vector3 point){if(ringing!=null)ringing.transform.position=point;}
 public void Build(Camera eye,PbrLibrary pbr) {
  ringing=Sound("Basement phone / audible call ring",Phone,"CallRing",true,25,85,.9f);
  ringing.spatialBlend=.65f;ringing.priority=35;
  entry=Sound("Don opening voice",DonOutside+Vector3.up*1.5f,"DonEntry",false,1,40,.8f);entry.spatialBlend=0;entry.priority=10;entry.volume=1;entry.clip?.LoadAudioData();
  lateEntry=Sound("Don final entry warning",DonOutside+Vector3.up*1.5f,"DonEntryLate",false,1,40,.8f);lateEntry.spatialBlend=0;lateEntry.priority=35;
  breakIn=Sound("Front entrance / forced entry",new Vector3(0,1,-7),"BreakIn",true,5,40,.8f);
 }

 AudioSource Sound(string name,Vector3 at,string clip,bool loop,float near,float far,float volume) {
  var s=new GameObject(name).AddComponent<AudioSource>();s.transform.SetParent(transform,false);s.transform.position=at;s.playOnAwake=false;s.loop=loop;s.clip=Resources.Load<AudioClip>("Presentation/Audio/"+clip);s.spatialBlend=1;s.minDistance=near;s.maxDistance=far;s.rolloffMode=AudioRolloffMode.Linear;s.dopplerLevel=0;s.volume=volume;return s;
 }
 public void Present(float elapsed,bool calling,bool active) {
  if(!active){if(entry.isPlaying){entry.Pause();entryPaused=true;}}
  else if(MultiplayerOpeningRules.Breached(elapsed)){entry.Stop();entryPaused=false;}
  else if(elapsed>=5&&!entryPlayed){entryPlayed=true;if(entry.clip!=null){entry.time=Mathf.Min(elapsed-5,entry.clip.length);if(elapsed-5<entry.clip.length)entry.Play();}}
  else if(entryPaused){entry.UnPause();entryPaused=false;}
  if(!active){if(lateEntry.isPlaying){lateEntry.Pause();latePaused=true;}}
  else if(elapsed>=60){lateEntry.Stop();latePaused=false;}
  else if(elapsed>=48&&!latePlayed){latePlayed=true;if(lateEntry.clip!=null&&elapsed-48<lateEntry.clip.length){lateEntry.time=Mathf.Max(0,elapsed-48);lateEntry.Play();}}
  else if(latePaused){lateEntry.UnPause();latePaused=false;}
  VoicePlaying=entry.isPlaying||lateEntry.isPlaying;
  breakIn.volume=VoicePlaying?.08f:.8f;
  SetPlaying(ringing,active&&calling);SetPlaying(breakIn,active&&!MultiplayerOpeningRules.Breached(elapsed));
 }

 static void SetPlaying(AudioSource s,bool play){if(s==null||s.clip==null)return;if(play&&!s.isPlaying)s.Play();else if(!play&&s.isPlaying)s.Stop();}
 void OnDisable(){VoicePlaying=false;if(lateEntry!=null)lateEntry.Stop();if(entry!=null)entry.Stop();if(ringing!=null)ringing.Stop();if(breakIn!=null)breakIn.Stop();}
 void Suspend(){VoicePlaying=false;if(lateEntry!=null&&lateEntry.isPlaying){lateEntry.Pause();latePaused=true;}if(entry!=null&&entry.isPlaying){entry.Pause();entryPaused=true;}if(ringing!=null)ringing.Stop();if(breakIn!=null)breakIn.Stop();}
 void OnApplicationPause(bool paused){if(paused)Suspend();}
 void OnApplicationFocus(bool focused){if(!focused)Suspend();}
}

