using System;
using System.Collections.Generic;
using UnityEngine;

// One authored design drives collision, set dressing and offline architectural review.
public sealed class EstateRenovation : MonoBehaviour
{
    [Serializable] public sealed class Part {public string name,material,shape;public float[] p,s;public float yaw;public int layer;public bool collision;}
    [Serializable] public sealed class Asset {public string id;public float[] p,s;public float yaw;public int layer;}
    [Serializable] public sealed class Lamp {public float[] p;public float range,intensity;public bool outside;}
    [Serializable] public sealed class Zone {public string name;public float floor;public float[] bounds;}
    [Serializable] public sealed class Design {public int version;public string title;public Part[] parts;public Asset[] assets;public Lamp[] lights;public Zone[] zones;}
    static Design design;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetDesign(){design=null;}
    public static Design Plan {
        get {
            if(design==null) {
                var text=Resources.Load<TextAsset>("Estate/EstateDesign");
                if(text==null)throw new InvalidOperationException("Estate design asset is missing.");
                design=JsonUtility.FromJson<Design>(text.text);
                if(design==null||design.version!=1||design.parts==null)throw new InvalidOperationException("Estate design is invalid.");
            }
            return design;
        }
    }
    public static Vector3 V(float[] p)=>new Vector3(p[0],p[1],p[2]);
    public static Material Finish(PbrLibrary p,string name) {
        switch(name) {
            case "stone":return p.EstateStone;
            case "paving":return p.EstatePaving;
            case "cedar":return p.Walnut;
            case "earth":return p.EstateEarth;
            case "clearGlass":return p.ClearLens;
            case "water":return p.EstateWater;
            case "exteriorGlow":return p.EstateGlow;
            case "metal":return p.Metal;
            case "brass":return p.Brass;
            case "warm":return p.WarmEmitter;
            case "ink":case "rug":return p.Fabric;
            case "sage":return p.EstateSage;
            case "plaster":case "wall":return p.Plaster;
            default:return p.Rubber;
        }
    }
    readonly List<Light> lights=new List<Light>();readonly List<bool> exterior=new List<bool>();
    readonly List<float> gains=new List<float>();bool powered=true;Camera eye;
    readonly Material[] circuitEmitters=new Material[HouseLightingLayout.Circuits.Length];int houseMask=HouseLightingLayout.AllOn;
    public void SetHouseMask(int mask){houseMask=mask;RefreshCircuitEmissions();RefreshCircuitLights();}
    void RefreshCircuitEmissions(){for(int i=0;i<circuitEmitters.Length;i++)if(circuitEmitters[i]!=null)circuitEmitters[i].SetColor("_EmissionColor",HouseLightingLayout.IsOn(houseMask,i,powered)?new Color(1,.72f,.42f)*2.5f:Color.black);}
    float selectAt;
    public void Build(PbrLibrary p,ScannedAssetLibrary scans,Camera viewer)
    {
        eye=viewer;
        for(int i=0;i<circuitEmitters.Length;i++)circuitEmitters[i]=p.Emitter(HouseLightingLayout.Circuits[i].name+" / switched architectural diffuser",new Color(1,.72f,.42f),2.5f);
        foreach(var part in Plan.parts) {
            if(part.collision)continue;
            int circuit=HouseLightingLayout.CircuitAt(part.p[0],part.p[1],part.p[2]);
            var material=part.material=="warm"&&circuit>=0?circuitEmitters[circuit]:Finish(p,part.material);
            var g=VisualMesh.Rounded(part.name,V(part.p),V(part.s),.012f,material,transform);
            g.transform.localRotation=Quaternion.Euler(0,part.yaw,0);g.layer=part.layer;
        }
        foreach(var asset in Plan.assets)scans.Place(asset.id,V(asset.p),V(asset.s),asset.yaw,asset.layer);
        foreach(var lamp in Plan.lights) {
            var light=new GameObject(lamp.outside?"Estate / shielded landscape lamp":"Estate / switched room fill").AddComponent<Light>();
            light.transform.SetParent(transform,false);light.transform.localPosition=V(lamp.p);
            light.type=LightType.Point;light.range=lamp.range;light.intensity=lamp.intensity;
            light.color=new Color(1,.78f,.54f);light.shadows=LightShadows.None;
            light.cullingMask=lamp.outside?1<<9:~((1<<8)|(1<<9));
            lights.Add(light);exterior.Add(lamp.outside);gains.Add(lamp.intensity);
        }
    }
    public void SetPower(bool on){powered=on;RefreshCircuitEmissions();RefreshCircuitLights();}
    bool CircuitOn(int index){var at=lights[index].transform.position;return exterior[index]||HouseLightingLayout.IsOn(houseMask,HouseLightingLayout.CircuitAt(at.x,at.y,at.z),powered);}
    void RefreshCircuitLights(){for(int i=0;i<lights.Count;i++)lights[i].intensity=CircuitOn(i)?gains[i]:0;}
    void LateUpdate() {
        if(eye==null||Time.unscaledTime<selectAt)return;selectAt=Time.unscaledTime+.35f;
        for(int i=0;i<lights.Count;i++)lights[i].enabled=CircuitOn(i)&&(lights[i].transform.position-eye.transform.position).sqrMagnitude<100;
    }
    public static string Room(Vector3 position,string fallback) {
        foreach(var zone in Plan.zones) {
            var b=zone.bounds;
            if(Mathf.Abs(position.y-zone.floor)<1.8f&&position.x>=b[0]&&position.x<=b[1]&&position.z>=b[2]&&position.z<=b[3])return zone.name.ToUpperInvariant();
        }
        if(position.y>-.5f&&position.y<2.5f&&(position.x< -10||position.x>16||Mathf.Abs(position.z)>7))return "ESTATE GROUNDS";
        return fallback;
    }
}
