using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

// Presentation coordinator. Gameplay geometry, pathfinding and security state live in GameOver.
public sealed class MansionVisuals : MonoBehaviour
{
    MansionEnvironment environment;MansionLighting lighting;
    IntruderVisuals intruder;ThrillerRendering rendering;FirstPersonPresence presence;
    int roomLightMask=HouseLightingLayout.AllOn;
    readonly List<ReflectionProbe> reflections=new List<ReflectionProbe>();bool power=true;
    public void SetRoomLightMask(int mask){roomLightMask=mask;RefreshRoomProbes();}
    void RefreshRoomProbes(){foreach(var probe in reflections)if(probe!=null){var at=probe.transform.position;probe.intensity=HouseLightingLayout.IsOn(roomLightMask,HouseLightingLayout.CircuitAt(at.x,at.y,at.z),power)?.35f:.035f;}}
    public Texture GlassBackground => rendering!=null?rendering.GlassBackground:null;
    public void Build(NavMeshAgent don,Camera eye,List<GameObject> geometry,PbrLibrary pbr)
    {
        environment=gameObject.AddComponent<MansionEnvironment>();environment.Build(geometry,pbr);
        lighting=gameObject.AddComponent<MansionLighting>();lighting.Build(eye,pbr);
        intruder=new GameObject("Don / visual rig").AddComponent<IntruderVisuals>();intruder.Build(don,pbr);
        rendering=eye.gameObject.AddComponent<ThrillerRendering>();rendering.Configure(eye);
        presence=eye.gameObject.AddComponent<FirstPersonPresence>();presence.Build(pbr);
        StartCoroutine(CaptureReflection());
    }
    System.Collections.IEnumerator CaptureReflection()
    {
        yield return null;
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
        Vector3[] rooms={new Vector3(6,1.6f,1),new Vector3(-6,1.6f,0),new Vector3(-6,4.9f,0),new Vector3(5,4.9f,0),new Vector3(-3,-1.9f,-1)};
        foreach(var room in rooms) {
            var probe=new GameObject("Room-local cached reflection").AddComponent<ReflectionProbe>();reflections.Add(probe);probe.transform.SetParent(transform);
            probe.transform.position=room;probe.size=new Vector3(room.y<0?16:8,3,12);probe.boxProjection=true;
            probe.mode=UnityEngine.Rendering.ReflectionProbeMode.Realtime;probe.refreshMode=UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
            probe.timeSlicingMode=UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces;
            probe.resolution=Application.isMobilePlatform?64:128;probe.cullingMask=~((1<<8)|(1<<10));probe.hdr=true;probe.intensity=power?.65f:.08f;
            // Realtime probes render asynchronously. Keep this room lit until all faces finish.
            lighting.BeginReflectionCapture(room);
            try {
                int capture=probe.RenderProbe();
                if(capture>=0)while(!probe.IsFinishedRendering(capture))yield return null;
            } finally {
                lighting.EndReflectionCapture();
            }
            yield return null;
        }
    }
    public void SetPower(bool powered){environment.SetPower(powered);lighting.SetPower(powered);power=powered;
        RenderSettings.ambientSkyColor=powered?new Color(.18f,.20f,.23f):new Color(.055f,.075f,.10f);
        RenderSettings.ambientEquatorColor=powered?new Color(.105f,.12f,.135f):new Color(.025f,.035f,.05f);
        RenderSettings.ambientGroundColor=powered?new Color(.065f,.07f,.08f):new Color(.012f,.018f,.025f);
        RefreshRoomProbes();}
    public void SetState(float danger,bool chase,bool active,float playerSpeed)
    {
        rendering.SetState(danger,active);intruder.SetState(chase,active);presence.SetMotion(playerSpeed,active);
    }
    public void SetInteraction(bool calling,bool hidden,bool cameras,bool powered,bool playing)
    {presence.SetInteraction(calling,hidden,cameras,powered,playing);}
    public void SetEyeHeight(float height,bool snap=false){presence.SetEyeHeight(height,snap);}
    public void SetRivalState(bool asDon,float speed,float donSpeed,bool active,bool cameras,bool hidden,float height,bool underBed=false,bool calling=false,bool donCrouched=false,bool snapEye=false)
    {
        intruder.SetHumanMotion(donSpeed,donCrouched);intruder.SetState(false,active);rendering.SetState(0,active);
        presence.SetWardrobe(asDon);presence.SetMotion(speed,active);presence.SetInteraction(calling,hidden&&!asDon,cameras,true,active&&!(underBed&&!asDon));
        presence.SetEyeHeight(height,snapEye);
    }
}
