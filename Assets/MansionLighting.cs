using UnityEngine;
using System.Collections.Generic;

// Individual room circuits. Fixtures sit below the ceiling rather than inside its trim.
public sealed class MansionLighting : MonoBehaviour
{
    sealed class Fixture {public Light light;public float gain;public int circuit;public bool shadows;}
    readonly List<Fixture> fixtures=new List<Fixture>();
    Transform[] switches,rockers;Material[] emitters;AudioSource click;AudioClip clickClip,offClip;
    int houseMask=HouseLightingLayout.AllOn;bool applied,powered=true,showcase,capturingReflection;
    Camera viewer;float nextSelection;
    public int HouseMask=>houseMask;
    public int BedroomMask=>houseMask;
    public void SetShowcase(bool value){showcase=value;nextSelection=0;}
    public void Build(Camera camera,PbrLibrary pbr)
    {
        viewer=camera;int count=HouseLightingLayout.Circuits.Length;
        switches=new Transform[count];rockers=new Transform[count];emitters=new Material[count];
        click=new GameObject("Room switch / positional click").AddComponent<AudioSource>();click.transform.SetParent(transform,false);click.playOnAwake=false;click.spatialBlend=1;click.minDistance=1;click.maxDistance=12;click.dopplerLevel=0;
        clickClip=Resources.Load<AudioClip>("Presentation/Audio/SwitchOn");offClip=Resources.Load<AudioClip>("Presentation/Audio/SwitchOff");
        for(int i=0;i<count;i++) {
            var c=HouseLightingLayout.Circuits[i];
            emitters[i]=pbr.Emitter(c.name+" / switched diffuser",new Color(1,.78f,.53f),2.5f);
            var plate=VisualMesh.Rounded(c.name+" / working light switch",new Vector3(c.sx,c.floor+1.2f,c.sz),new Vector3(.10f,.24f,.16f),.012f,pbr.Porcelain,transform);
            plate.transform.localRotation=Quaternion.Euler(0,c.nz!=0?90:0,0);
            var collider=plate.AddComponent<BoxCollider>();collider.size=new Vector3(.12f,.26f,.18f);switches[i]=plate.transform;
            float normal=c.nx!=0?c.nx:-c.nz;
            rockers[i]=VisualMesh.Rounded(c.name+" / brass light rocker",new Vector3(normal*.065f,0,0),new Vector3(.03f,.14f,.085f),.008f,pbr.Brass,plate.transform).transform;
            AddFixture(i,new Vector3(c.x,c.floor+2.62f,c.z),pbr);
            if(c.name=="Garage")AddFixture(i,new Vector3(11.5f,2.62f,5.4f),pbr);
            if(i<2)AddFixture(i,new Vector3(c.x,c.floor+2.62f,-5.65f),pbr);
            var fill=new GameObject(c.name+" / broad room fill").AddComponent<Light>();fill.transform.SetParent(transform,false);
            fill.transform.localPosition=new Vector3(c.x,c.floor+2.12f,c.z);fill.type=LightType.Point;
            fill.color=new Color(1,.85f,.67f);fill.range=c.large?5.2f:3.8f;fill.renderMode=LightRenderMode.ForcePixel;
            fixtures.Add(new Fixture{light=fill,gain=c.floor<0?1.5f:1.1f,circuit=i,shadows=true});
            if(c.floor<0&&i!=18&&i!=22)AddFixture(i,new Vector3(c.x+.5f,c.floor+2.62f,c.z-1.2f),pbr);
            if(c.large) {
                // Bedroom second fixtures run lengthwise; other larger rooms use a second bay.
                float offset=i<2?1.75f:1.3f;
                AddFixture(i,new Vector3(c.x,c.floor+2.62f,c.z+(c.z>3?-offset:offset)),pbr);
            }
        }
        foreach(float bed in new[]{-6f,5f}) {
            var l=new GameObject("Bedside lamp / room circuit").AddComponent<Light>();l.transform.SetParent(transform,false);
            l.transform.localPosition=bed>0?new Vector3(3.1f,4.52f,-6.50f):new Vector3(-4.1f,4.52f,-6.50f);
            l.type=LightType.Point;l.color=new Color(1,.70f,.4f);l.range=2.5f;l.renderMode=LightRenderMode.ForcePixel;
            fixtures.Add(new Fixture{light=l,gain=.8f,circuit=bed<0?0:1});
        }
        ApplyHouseMask(HouseLightingLayout.AllOn);
    }
    void AddFixture(int circuit,Vector3 at,PbrLibrary pbr)
    {
        var c=HouseLightingLayout.Circuits[circuit];
        var housing=VisualMesh.Rounded(c.name+" / ceiling light housing",at,new Vector3(.58f,.10f,.58f),.025f,pbr.Brass,transform);
        var body=housing.AddComponent<BoxCollider>();body.size=new Vector3(.58f,.10f,.58f);
        VisualMesh.Rounded(c.name+" / luminous ceiling diffuser",at+Vector3.down*.06f,new Vector3(.49f,.035f,.49f),.012f,emitters[circuit],transform);
        float ceiling=circuit==13?2.8f:c.floor>3?3.2f:3;
        VisualMesh.Rounded(c.name+" / ceiling mount",new Vector3(at.x,c.floor+(2.67f+ceiling)/2,at.z),new Vector3(.055f,ceiling-2.67f,.055f),.009f,pbr.Metal,transform);
        var l=new GameObject(c.name+" / real room light").AddComponent<Light>();l.transform.SetParent(transform,false);
        l.transform.localPosition=at+Vector3.down*.12f;l.transform.localRotation=Quaternion.Euler(90,0,0);
        l.type=LightType.Spot;l.spotAngle=122;l.innerSpotAngle=75;l.range=5.8f;
        l.color=new Color(1,.85f,.67f);l.renderMode=LightRenderMode.ForcePixel;l.cullingMask=~0;
        l.shadowBias=.025f;l.shadowNormalBias=.15f;l.shadowStrength=.85f;
        fixtures.Add(new Fixture{light=l,gain=6.3f,circuit=circuit,shadows=true});
    }
    public Vector3 SwitchPrompt(int index)=>switches[index].position;
    public string SwitchLabel(int index)=>(houseMask&(1<<index))!=0?"TURN LIGHTS OFF":"TURN LIGHTS ON";
    public string SwitchFeedback(int index) {
        bool on=(houseMask&(1<<index))!=0;string room=HouseLightingLayout.Circuits[index].name;
        return powered?room+" lights "+(on?"on.":"off."):room+" switch set to "+(on?"on":"off")+". Main power is off at the basement breaker.";
    }
    public int FocusSwitch(Camera eye)=>FocusSwitch(eye.transform.position,eye.transform.forward,eye.GetComponentInParent<CharacterController>());
    public int FocusSwitch(Vector3 origin,Vector3 forward,Collider observer=null)
    {
        RaycastHit hit;
        if(InteractionRaycast.First(origin,forward,2.5f,observer,out hit))
            for(int i=0;i<switches.Length;i++)if(hit.transform==switches[i]||hit.transform.IsChildOf(switches[i]))return i;
        return -1;
    }
    public void ToggleBedroom(int index)=>ToggleSwitch(index);
    public void ToggleSwitch(int index)
    {
        if(index<0||index>=switches.Length)return;
        ApplyHouseMask(houseMask^(1<<index));
    }
    public void ApplyBedroomMask(int mask)=>ApplyHouseMask(mask);
    public void ApplyHouseMask(int mask)
    {
        mask&=HouseLightingLayout.AllOn;if(applied&&houseMask==mask)return;
        int changed=applied?houseMask^mask:0;applied=true;houseMask=mask;
        for(int i=0;i<switches.Length;i++)if((changed&(1<<i))!=0){click.transform.position=switches[i].position;var clip=(mask&(1<<i))!=0?clickClip:offClip;if(clip!=null)click.PlayOneShot(clip,.6f);}
        for(int i=0;i<switches.Length;i++)rockers[i].localRotation=Quaternion.Euler(0,0,(mask&(1<<i))!=0?14:-14);
        GetComponent<MansionRoomDetails>()?.SetBedroomMask(mask);
        GetComponent<EstateRenovation>()?.SetHouseMask(mask);
        GetComponent<MansionVisuals>()?.SetRoomLightMask(mask);
        Refresh();
    }
    void Refresh()
    {
        GetComponent<MansionEstateDetails>()?.SetHouseLighting(houseMask,powered);
        for(int i=0;i<emitters.Length;i++)emitters[i].SetColor("_EmissionColor",HouseLightingLayout.IsOn(houseMask,i,powered)?new Color(1,.78f,.53f)*2.5f:Color.black);
        foreach(var f in fixtures)f.light.intensity=HouseLightingLayout.IsOn(houseMask,f.circuit,powered)?f.gain:0;
        nextSelection=0;if(viewer!=null)SelectLights(viewer.transform.position);
    }
    public void SetPower(bool on){if(powered==on)return;powered=on;Refresh();}
    public void BeginReflectionCapture(Vector3 room){capturingReflection=true;SelectLights(room);}
    public void EndReflectionCapture(){capturingReflection=false;nextSelection=0;if(viewer!=null)SelectLights(viewer.transform.position);}
    void LateUpdate(){if(capturingReflection||viewer==null||Time.unscaledTime<nextSelection)return;nextSelection=Time.unscaledTime+.2f;SelectLights(viewer.transform.position);}
    void SelectLights(Vector3 eye)
    {
        int room=HouseLightingLayout.CircuitAt(eye.x,eye.y,eye.z);
        fixtures.Sort((a,b)=>Priority(a,eye,room).CompareTo(Priority(b,eye,room)));
        int shadows=0,enabled=0,maxLights=GamePreferences.MaxLights(Application.isMobilePlatform);
        bool menu=showcase&&(!capturingReflection||room<0);
        foreach(var f in fixtures) {
            bool living=f.circuit==4; // Keep the existing living-room CCTV feed lit.
            bool sameFloor=room<0||Mathf.Abs(HouseLightingLayout.Circuits[f.circuit].floor-HouseLightingLayout.Circuits[room].floor)<.1f;
            bool nearby=(f.light.transform.position-eye).sqrMagnitude<100;
            bool openPlan=room>=2&&room<=5&&f.circuit>=2&&f.circuit<=5;
            bool roomVisible=f.circuit==room||openPlan||InteractionRaycast.Clear(eye,f.light.transform.position,viewer!=null?viewer.GetComponentInParent<CharacterController>():null);
            bool visible=menu||living||sameFloor&&nearby&&roomVisible;
            f.light.enabled=HouseLightingLayout.IsOn(houseMask,f.circuit,powered)&&visible&&(living||enabled<maxLights-3);
            if(f.light.enabled)enabled++;
            bool shadow=f.shadows&&f.light.enabled&&shadows<GamePreferences.MaxShadowLights(Application.isMobilePlatform);
            f.light.shadows=shadow?LightShadows.Soft:LightShadows.None;if(shadow)shadows++;
        }
    }
    static float Priority(Fixture f,Vector3 eye,int room)=>(f.circuit==room?0:1000)+(f.light.transform.position-eye).sqrMagnitude;
}
