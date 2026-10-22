using UnityEngine;
using System.Collections.Generic;

// Presentation only: listens to game state; never changes detection or AI destinations.
public sealed class ThrillerSoundscape : MonoBehaviour
{
    public struct State
    {
        public bool started,paused,won,lost,hidden,called,powered,cameras,motion;
        public float danger,callProgress,time,rescue;public Vector3 position,tapsee,don;public bool actors,sensorTapseeHidden,jammed;
    }
    AudioSource room,hum,tension,phoneLine,staticBed,cues,donVictory,tapseeVictory,siren;
    HouseMotionSensors sensors;PoliceArrivalLights policeLights;
    GameplayStorm storm;
    readonly List<AudioSource> loops=new List<AudioSource>();
    readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
    State previous;bool initialized,running,played,ended;bool focused=true,background;
    AudioClip Clip(string name)
    {
        if(clips.TryGetValue(name,out var clip))return clip;
        clip=Resources.Load<AudioClip>("Presentation/Audio/"+name);clips[name]=clip;
        if(clip==null)Debug.LogWarning("Missing soundscape clip: "+name);
        return clip;
    }
    AudioSource Source(string label,string clip,bool loop)
    {
        var go=new GameObject(label);go.transform.SetParent(transform,false);
        var source=go.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=loop;source.spatialBlend=0;
        source.priority=loop?180:90;source.volume=0;source.clip=clip==null?null:Clip(clip);
        if(loop)loops.Add(source);return source;
    }
    public void Build()
    {
        storm=gameObject.AddComponent<GameplayStorm>();storm.Build();
        sensors=gameObject.AddComponent<HouseMotionSensors>();sensors.Build();
        policeLights=gameObject.AddComponent<PoliceArrivalLights>();policeLights.Build();
        siren=Source("Approaching police siren","PoliceSiren",true);
        room=Source("Low room air","RoomTone",true);hum=Source("Mains hum","Hum",true);
        tension=Source("Threat pulse","Tension",true);phoneLine=Source("Telephone connection line","PhoneLine",true);
        staticBed=Source("Offline CCTV static","Static",true);
        cues=Source("Interaction feedback",null,false);cues.volume=.24f;
        donVictory=Source("Don victory / user-provided audio","DonVictory",false);donVictory.volume=.8f;donVictory.priority=30;
        tapseeVictory=Source("Tapsee victory / user-provided audio","TapseeVictory",false);tapseeVictory.volume=.8f;tapseeVictory.priority=30;
        foreach(string name in new[]{"Click","Connected","PowerOff","PowerOn","Hide","Sensor","Capture","Rescue"})Clip(name);
    }
    void Cue(string name,float gain=1){var clip=Clip(name);if(clip!=null)cues.PlayOneShot(clip,gain);}
    void Suspend()
    {
        policeLights?.Off();
        if(!running)return;
        foreach(var source in loops)source.Pause();cues.Pause();donVictory.Pause();tapseeVictory.Pause();running=false;
    }
    void Resume()
    {
        if(running)return;
        foreach(var source in loops) {if(played)source.UnPause();else if(source.clip!=null)source.Play();}
        cues.UnPause();running=true;played=true;
    }
    void Gain(AudioSource source,float target,float dt)
    {source.volume=Mathf.Lerp(source.volume,target,1-Mathf.Exp(-dt*3));}
    public void Tick(State s,float dt)
    {
        if(storm!=null)storm.Tick(s,dt);
        policeLights?.Present(s,s.started&&!s.paused&&!s.won&&!s.lost&&focused&&!background);
        if(sensors!=null)sensors.Tick(s,s.started&&!s.paused&&!s.won&&!s.lost&&focused&&!background);
        if(!initialized){previous=s;initialized=true;}
        if(!s.started||s.paused||!focused||background){Suspend();previous=s;return;}
        if(s.won||s.lost) {
            if(!ended){foreach(var source in loops)source.Stop();cues.Stop();cues.UnPause();if(s.lost&&donVictory.clip!=null){donVictory.Play();Debug.Log("DON_VICTORY_AUDIO started="+donVictory.isPlaying+" clip="+donVictory.clip.name+" seconds="+donVictory.clip.length.ToString("F2"));}else if(s.won&&tapseeVictory.clip!=null){tapseeVictory.Play();Debug.Log("TAPSEE_VICTORY_AUDIO started="+tapseeVictory.isPlaying+" seconds="+tapseeVictory.clip.length.ToString("F2"));}else Cue(s.won?"Rescue":"Capture");ended=true;}
            if(!running){cues.UnPause();donVictory.UnPause();tapseeVictory.UnPause();running=true;}
            previous=s;return;
        }
        Resume();float mix=GamePreferences.Atmosphere;
        siren.volume=SirenGain(s.called,s.rescue)*mix;
        bool basement=s.position.y< -1;
        Gain(room,(basement?.05f:.03f)*mix,dt);
        Gain(hum,s.powered?(basement?.045f:.018f)*mix:0,dt);
        Gain(tension,Mathf.SmoothStep(0,.085f,s.danger)*mix,dt);
        phoneLine.volume=Mathf.MoveTowards(phoneLine.volume,s.callProgress>.01f&&!s.called?.12f:0,dt*1.4f);
        staticBed.volume=s.cameras&&(!s.powered||s.jammed)?.16f*mix:0;
        tension.pitch=Mathf.Lerp(1,1.16f,s.danger);
        if(s.powered!=previous.powered)Cue(s.powered?"PowerOn":"PowerOff",.8f);
        if(s.cameras!=previous.cameras)Cue("Click",.55f);
        if(s.hidden!=previous.hidden)Cue("Hide",.65f);
        if(sensors==null&&s.motion&&!previous.motion)Cue("Sensor",.42f);
        if(s.called&&!previous.called){phoneLine.volume=0;Cue("Connected");}
        else if(s.callProgress<=0&&previous.callProgress>.1f)Cue("Click",.4f);
        previous=s;
    }
    public static float SirenGain(bool called,float remaining)=>called&&remaining>0&&remaining<=10?Mathf.Lerp(.08f,.65f,1-Mathf.Clamp01(remaining/10)):0;
    void OnApplicationFocus(bool focus){focused=focus;if(!focus){Suspend();sensors?.Suspend();}}
    void OnApplicationPause(bool pause){background=pause;if(pause){Suspend();sensors?.Suspend();}}
    void OnDestroy(){if(donVictory!=null)donVictory.Stop();if(tapseeVictory!=null)tapseeVictory.Stop();foreach(var source in loops)if(source!=null)source.Stop();}
}
