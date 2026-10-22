using UnityEngine;

// Two independent, host-authoritative batteries; local and remote beams are both visible.
[DefaultExecutionOrder(200)]
public sealed class PlayerFlashlights : MonoBehaviour
{
    readonly FlashlightBattery[] batteries={new FlashlightBattery(),new FlashlightBattery()};
    readonly Light[] beams=new Light[2];readonly Transform[] props=new Transform[2];
    readonly Vector3[] positions=new Vector3[2];readonly float[] yaws=new float[2],pitches=new float[2];
    readonly Collider[] actors=new Collider[2];
    Camera eye;int localRole;bool active;
    public void Build(Camera camera,PbrLibrary pbr)
    {
        eye=camera;actors[0]=camera.GetComponentInParent<CharacterController>();
        for(int i=0;i<2;i++) {
            var lamp=new GameObject(i==0?"Tapsee / battery flashlight":"Don / battery flashlight");lamp.transform.SetParent(transform,false);
            beams[i]=lamp.AddComponent<Light>();var l=beams[i];l.type=LightType.Spot;l.spotAngle=58;l.innerSpotAngle=32;l.range=18;l.intensity=4.5f;
            l.color=new Color(.92f,.96f,1);l.renderMode=LightRenderMode.ForcePixel;l.shadowNearPlane=.02f;l.shadowBias=.015f;l.shadowNormalBias=.12f;
            l.shadows=GamePreferences.FlashlightShadows(Application.isMobilePlatform);l.enabled=false;
            var prop=new GameObject("Handheld torch casing").transform;prop.SetParent(transform,false);props[i]=prop;
            var shell=VisualMesh.Lathe("Flashlight metal barrel",Vector3.zero,new[]{new Vector2(.026f,0),new Vector2(.027f,.14f),new Vector2(.045f,.17f),new Vector2(.045f,.20f)},pbr.Metal,prop,20);
            shell.transform.localRotation=Quaternion.Euler(90,0,0);
            VisualMesh.Rounded("Flashlight lens",new Vector3(0,0,.201f),new Vector3(.061f,.061f,.006f),.022f,pbr.Porcelain,prop);
            prop.gameObject.SetActive(false);
        }
    }
    public void ConfigureLocal(int role){localRole=role;actors[role]=eye.GetComponentInParent<CharacterController>();}
    public void SetActive(bool value){active=value;if(!value)for(int i=0;i<2;i++){beams[i].enabled=false;props[i].gameObject.SetActive(false);}}
    public bool Toggle(int role)=>batteries[role].Toggle();
    public void Step(float dt){foreach(var battery in batteries)battery.Step(dt);var shadows=GamePreferences.FlashlightShadows(Application.isMobilePlatform);for(int i=0;i<beams.Length;i++)if(beams[i]!=null&&beams[i].shadows!=shadows)beams[i].shadows=shadows;}
    public void Recharge(int role,float fraction)=>batteries[role].Recharge(fraction);
    public bool On(int role)=>batteries[role].On;
    public float Charge(int role)=>batteries[role].Charge;
    public float Yaw(int role)=>yaws[role];public float Pitch(int role)=>pitches[role];
    public void Aim(int role,float yaw,float pitch){yaws[role]=yaw;pitches[role]=Mathf.Clamp(pitch,-80,80);}
    public void Pose(int role,Vector3 at,Collider observer=null){positions[role]=at;if(observer!=null)actors[role]=observer;}
    public void Apply(int role,float charge,bool on,float yaw,float pitch){batteries[role].Apply(charge,on);Aim(role,yaw,pitch);}
    void LateUpdate()
    {
        if(eye==null)return;
        for(int i=0;i<2;i++) {
            bool visible=active&&batteries[i].On;beams[i].enabled=visible;props[i].gameObject.SetActive(visible);
            if(!visible)continue;
            Quaternion aim=i==localRole?eye.transform.rotation:Quaternion.Euler(pitches[i],yaws[i],0);
            Vector3 at=i==localRole?eye.transform.position:positions[i];
            Vector3 origin=at+aim*new Vector3(.16f,-.13f,.16f);
            Vector3 beamOrigin=origin+aim*Vector3.forward*.22f;RaycastHit hit;
            if(InteractionRaycast.First(at,beamOrigin-at,Vector3.Distance(at,beamOrigin),actors[i],out hit)) {
                beamOrigin=hit.point-(beamOrigin-at).normalized*.04f;
                origin=beamOrigin-aim*Vector3.forward*.22f;
                if(Vector3.Distance(at,beamOrigin)<.25f)props[i].gameObject.SetActive(false);
            }
            props[i].position=origin;props[i].rotation=aim;
            beams[i].transform.position=beamOrigin;beams[i].transform.rotation=aim;
            beams[i].intensity=Mathf.Lerp(2.8f,4.5f,Mathf.Clamp01(batteries[i].Charge*5));
        }
    }
    public string Feedback(int role)=>Charge(role)<=0?"Flashlight battery empty.":On(role)?"Flashlight on. Battery drains while in use.":"Flashlight off. Battery conserved.";
    public void DrawHud(int role)
    {
        int percent=Mathf.CeilToInt(Charge(role)*100);var old=GUI.color;GUI.color=percent<=15?new Color(1,.63f,.3f):Color.white;
        if(!Application.isMobilePlatform)GUI.Label(new Rect(28,666,360,28),"L / FLASHLIGHT  "+percent+"%  "+(On(role)?"ON":"OFF"));
        if(Application.isMobilePlatform)GUI.Box(TacticalHud.FlashlightButton,"LIGHT\n"+percent+"%");
        GUI.color=old;
    }
}
