using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

// One owner for visible weather and its sound. Uses match time, not wall-clock time.
public sealed class GameplayStorm : MonoBehaviour
{
    readonly List<ParticleSystem> particles=new List<ParticleSystem>();
    readonly List<LineRenderer> bolts=new List<LineRenderer>();
    readonly List<Material> materials=new List<Material>();
    AudioSource rain,thunder;AudioLowPassFilter rainFilter,thunderFilter;
    Light lightning;bool running,played,focused=true;int lastThunder=-1,lastBolt=-1;
    float exposure=.2f;
    public bool HorrorInterior;
    public void Build()
    {
        var shader=Resources.Load<Shader>("Presentation/StormRain");
        if(shader!=null) {
            var material=new Material(shader){name="Storm / wind-driven rain",enableInstancing=true};materials.Add(material);
            RainZone("West grounds",new Vector3(-16,10.2f,0),new Vector3(9,.3f,34),material);
            RainZone("East grounds",new Vector3(21,10.2f,0),new Vector3(9,.3f,34),material);
            RainZone("North grounds",new Vector3(2,10.2f,12),new Vector3(28,.3f,10),material);
            RainZone("South west rain curtain",new Vector3(-7,10.2f,-12),new Vector3(9,.3f,10),material);
            RainZone("South east rain curtain",new Vector3(9,10.2f,-12),new Vector3(13,.3f,10),material);
            RainZone("Entrance forecourt",new Vector3(0,10.2f,-14),new Vector3(5,.3f,8),material);
            RainZone("West balcony rain curtain",new Vector3(-11.3f,10.2f,0),new Vector3(2,.3f,9),material);
            var splash=new Material(shader){name="Storm / rain impact rings"};splash.SetFloat("_Splash",1);materials.Add(splash);
            Splashes(new Vector3(-15,.025f,0),new Vector3(8,0,30),splash);
            Splashes(new Vector3(21,.025f,0),new Vector3(8,0,30),splash);
            Splashes(new Vector3(2,.025f,12),new Vector3(27,0,9),splash);
            Splashes(new Vector3(2,.025f,-13),new Vector3(27,0,8),splash);
            Splashes(new Vector3(-11,3.43f,0),new Vector3(1.8f,0,7.7f),splash);
            Splashes(new Vector3(0,3.43f,-8),new Vector3(2.5f,0,1.7f),splash);
            Splashes(new Vector3(-15,.025f,8),new Vector3(4.4f,0,3),splash);
            var mist=new Material(shader){name="Storm / low wind spray"};mist.SetFloat("_Mist",1);materials.Add(mist);
            Mist(new Vector3(-15,.45f,0),new Vector3(4,.3f,28),mist);
            Mist(new Vector3(20,.45f,0),new Vector3(4,.3f,28),mist);
            Mist(new Vector3(2,.45f,-12),new Vector3(24,.3f,4),mist);
            Mist(new Vector3(2,.45f,11),new Vector3(24,.3f,4),mist);
        }
        else Debug.LogWarning("Storm rain shader missing; weather audio remains available.");
        rain=Source("Heavy rain / shelter filtered","HeavyRain",true);rainFilter=rain.gameObject.AddComponent<AudioLowPassFilter>();
        thunder=Source("Storm / delayed spatial thunder","StormThunder",false);thunderFilter=thunder.gameObject.AddComponent<AudioLowPassFilter>();
        thunder.spatialBlend=1;thunder.minDistance=250;thunder.maxDistance=2200;thunder.dopplerLevel=0;thunder.rolloffMode=AudioRolloffMode.Linear;
        lightning=new GameObject("Storm lightning / independent of mains").AddComponent<Light>();lightning.transform.SetParent(transform,false);
        lightning.type=LightType.Directional;lightning.color=new Color(.63f,.76f,1);lightning.intensity=0;
        lightning.shadows=Application.isMobilePlatform?LightShadows.None:LightShadows.Hard;lightning.shadowStrength=.9f;lightning.shadowBias=.025f;lightning.shadowNormalBias=.12f;
        lightning.renderMode=LightRenderMode.ForcePixel;lightning.enabled=false;
        var boltShader=Resources.Load<Shader>("Presentation/StormBolt");
        if(boltShader!=null) {
            var core=new Material(boltShader){name="Storm / bolt core"};core.SetColor("_Color",new Color(2,2.6f,3,1));materials.Add(core);
            var halo=new Material(boltShader){name="Storm / bolt halo"};halo.SetColor("_Color",new Color(.12f,.26f,.5f,.22f));materials.Add(halo);
            Bolt("Main lightning channel",core,.045f,13);Bolt("Lightning glow",halo,.32f,13);Bolt("Lightning branch",core,.022f,7);
        }
        Shader.SetGlobalFloat("_StormStrength",0);Shader.SetGlobalFloat("_StormFlash",0);
    }
    AudioSource Source(string name,string clip,bool loop)
    {
        var s=new GameObject(name).AddComponent<AudioSource>();s.transform.SetParent(transform,false);
        s.clip=Resources.Load<AudioClip>("Presentation/Audio/"+clip);s.playOnAwake=false;s.loop=loop;s.volume=0;s.priority=loop?170:110;
        if(s.clip==null)Debug.LogWarning("Storm audio clip missing: "+clip);return s;
    }
    void RainZone(string name,Vector3 at,Vector3 size,Material material)
    {
        var go=new GameObject("Storm rain / "+name);go.transform.SetParent(transform,false);go.transform.position=at;
        var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.playOnAwake=false;main.loop=true;main.duration=4;main.simulationSpace=ParticleSystemSimulationSpace.World;
        main.startLifetime=.78f;main.startSpeed=0;main.startSize=new ParticleSystem.MinMaxCurve(.012f,.026f);
        main.startColor=new Color(.60f,.76f,.88f,.26f);main.maxParticles=Application.isMobilePlatform?700:1500;
        main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
        var emission=ps.emission;emission.rateOverTime=Application.isMobilePlatform?640:1350;
        var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=size;
        var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;
        velocity.x=new ParticleSystem.MinMaxCurve(.65f,1.15f);velocity.y=new ParticleSystem.MinMaxCurve(-19,-15);velocity.z=new ParticleSystem.MinMaxCurve(-.6f,-.2f);
        var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.Stretch;
        renderer.lengthScale=3.0f;renderer.velocityScale=.025f;renderer.cameraVelocityScale=0;
        renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        particles.Add(ps);
    }
    void Bolt(string name,Material material,float width,int count)
    {
        var line=new GameObject(name).AddComponent<LineRenderer>();line.transform.SetParent(transform,false);line.useWorldSpace=true;
        line.positionCount=count;line.widthMultiplier=width;line.sharedMaterial=material;line.numCornerVertices=1;
        line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.enabled=false;bolts.Add(line);
    }
    void Mist(Vector3 at,Vector3 size,Material material)
    {
        var go=new GameObject("Storm / ground wind spray");go.transform.SetParent(transform,false);go.transform.position=at;
        var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.playOnAwake=false;main.startLifetime=5;main.startSpeed=.18f;
        main.startSize=new ParticleSystem.MinMaxCurve(2,3.5f);main.startColor=new Color(.24f,.32f,.38f,.10f);main.maxParticles=Application.isMobilePlatform?8:16;
        main.simulationSpace=ParticleSystemSimulationSpace.World;
        var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=size;
        var emission=ps.emission;emission.rateOverTime=Application.isMobilePlatform?1:2;
        var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.x=.35f;velocity.y=.015f;velocity.z=-.12f;
        var color=ps.colorOverLifetime;color.enabled=true;
        var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.3f),new GradientAlphaKey(0,1)});color.color=gradient;
        var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;particles.Add(ps);
    }
    void Splashes(Vector3 at,Vector3 size,Material material)
    {
        var go=new GameObject("Storm / pooled surface impacts");go.transform.SetParent(transform,false);go.transform.position=at;
        var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;
        main.startLifetime=.28f;main.startSpeed=0;main.startSize=new ParticleSystem.MinMaxCurve(.08f,.18f);main.startColor=new Color(.5f,.65f,.75f,.18f);main.maxParticles=80;
        var emission=ps.emission;emission.rateOverTime=at.y>1?24:Application.isMobilePlatform?50:100;
        var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(size.x,.005f,size.z);
        var grow=ps.sizeOverLifetime;grow.enabled=true;grow.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.25f,1,1.6f));
        var color=ps.colorOverLifetime;color.enabled=true;
        var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)});color.color=gradient;
        var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.HorizontalBillboard;
        renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;particles.Add(ps);
    }
    Vector3 StrikeDirection(int index)
    {return new Vector3((float)(StormWeatherRules.Seed(index,3)-.5)*1.8f,.4f,-1).normalized;}
    void ShapeBolt(int index)
    {
        lastBolt=index;Vector3 direction=StrikeDirection(index);
        Vector3 foot=new Vector3(direction.x*25,7,-23);
        for(int j=0;j<bolts.Count;j++) {
            var line=bolts[j];bool branch=j==2;
            for(int i=0;i<line.positionCount;i++) {
                float t=(float)i/(line.positionCount-1);
                float jitter=(float)(StormWeatherRules.Seed(index*19+i,4)-.5)*1.4f;
                line.SetPosition(i,foot+new Vector3(jitter+(branch?t*3:0),branch?10-t*6:23-t*23,branch?t*2:0));
            }
        }
        lightning.transform.rotation=Quaternion.LookRotation(-direction);
    }
    public void Tick(ThrillerSoundscape.State state,float dt)
    {
        bool inGame=state.started&&!state.won&&!state.lost;
        bool active=inGame&&!state.paused&&focused;
        Shader.SetGlobalFloat("_StormStrength",inGame?1:0);
        if(!active) {Suspend();return;}
        if(!running) {
            foreach(var ps in particles)ps.Play();
            if(played){rain.UnPause();thunder.UnPause();}else if(rain.clip!=null)rain.Play();
            played=true;running=true;
        }
        Shader.SetGlobalFloat("_StormClock",state.time);
        exposure=Mathf.Lerp(exposure,StormWeatherRules.Exposure(state.position.x,state.position.y,state.position.z,state.hidden),1-Mathf.Exp(-dt*2));
        rain.volume=(MultiplayerOpeningAtmosphere.VoicePlaying?.2f:1)*Mathf.Lerp(HorrorInterior?.14f:.055f,.36f,exposure)*GamePreferences.Atmosphere;
        rainFilter.cutoffFrequency=Mathf.Lerp(700,12000,exposure);
        thunder.volume=(MultiplayerOpeningAtmosphere.VoicePlaying?.18f:1)*Mathf.Lerp(HorrorInterior?.48f:.24f,.85f,exposure)*GamePreferences.Atmosphere;
        thunderFilter.cutoffFrequency=Mathf.Lerp(HorrorInterior?2400:1100,14000,exposure);
        int index=StormWeatherRules.LatestStrike(state.time);float flash=0;
        if(index>=0) {
            if(lastBolt!=index)ShapeBolt(index);
            flash=StormWeatherRules.Flash(state.time-StormWeatherRules.StrikeTime(index),GamePreferences.ReducedLightning);
            double arrival=StormWeatherRules.ThunderTime(index);
            if(lastThunder<index&&state.time>=arrival) {
                // Do not replay old thunder after a late join or a time correction.
                if(StormWeatherRules.ShouldThunder(index,state.time,lastThunder)&&thunder.clip!=null) {
                    thunder.transform.position=StrikeDirection(index)*(float)StormWeatherRules.Distance(index);
                    thunder.pitch=.86f+(float)StormWeatherRules.Seed(index,5)*.22f;thunder.Play();
                }
                lastThunder=index;
            }
        }
        lightning.enabled=flash>.002f;lightning.intensity=flash*3.3f;
        Shader.SetGlobalFloat("_StormFlash",flash);
        foreach(var line in bolts)line.enabled=flash>.04f&&!GamePreferences.ReducedLightning;
    }
    void Suspend()
    {
        if(running){foreach(var ps in particles)ps.Pause();rain.Pause();thunder.Pause();running=false;}
        if(lightning!=null)lightning.enabled=false;foreach(var line in bolts)line.enabled=false;Shader.SetGlobalFloat("_StormFlash",0);
    }
    void OnApplicationFocus(bool value){focused=value;if(!value)Suspend();}
    void OnApplicationPause(bool value){focused=!value;if(value)Suspend();}
    void OnDisable(){Suspend();Shader.SetGlobalFloat("_StormStrength",0);}
    void OnDestroy(){Shader.SetGlobalFloat("_StormStrength",0);Shader.SetGlobalFloat("_StormFlash",0);foreach(var m in materials)if(m!=null)Destroy(m);}
}
