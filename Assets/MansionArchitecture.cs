using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

// Decorative architecture and enclosed garden backdrop. Existing collision stays authoritative.
public sealed class MansionArchitecture : MonoBehaviour
{
    PbrLibrary pbr;
    Material stone,leaves,earth,sky;
    bool exterior;
    GameObject R(string name,Vector3 p,Vector3 s,Material m,float bevel=.02f)
    {var go=VisualMesh.Rounded(name,p,s,bevel,m,transform);go.layer=exterior?9:0;return go;}
    GameObject L(string name,Vector3 p,Vector2[] profile,Material m,int segments=16)
    {var go=VisualMesh.Lathe(name,p,profile,m,transform,segments);go.layer=exterior?9:0;return go;}
    void Rod(string name,Vector3 a,Vector3 b,float radius,Material m)
    {
        float h=Vector3.Distance(a,b);
        var go=L(name,a,new[]{new Vector2(0,0),new Vector2(radius,0),new Vector2(radius,h),new Vector2(0,h)},m,12);
        go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
    }
    public static bool IsRailing(string name) => name.ToLowerInvariant().Contains("railing")||name.ToLowerInvariant().Contains("guard");
    public void Build(PbrLibrary library,List<GameObject> geometry)
    {
        pbr=library;
        stone=pbr.Tint(pbr.Plaster,new Color(.32f,.34f,.34f),"Garden honed stone");
        leaves=pbr.Tint(pbr.Fabric,new Color(.075f,.13f,.10f),"Night foliage");
        earth=pbr.Tint(pbr.Plaster,new Color(.07f,.09f,.073f),"Garden ground");
        sky=pbr.ResourceMaterial("Presentation/NightSky","Nocturnal sky");RenderSettings.skybox=sky;
        foreach(var box in geometry) if(IsRailing(box.name)) {
            exterior=box.name.Contains("balcony");Rail(box.name,box.transform.position,box.transform.localScale);
        }
        exterior=false;Stairs(); // EstateRenovation now owns all exterior masses and grounds.
        // Layer 9 is decorative exterior only. Moonlight cannot flatten indoor lighting.
        var moon=new GameObject("Exterior-only moonlight").AddComponent<Light>();moon.transform.SetParent(transform);
        moon.type=LightType.Directional;moon.transform.rotation=Quaternion.LookRotation(-new Vector3(-.5f,.55f,-.65f));
        moon.color=new Color(.30f,.43f,.66f);moon.intensity=.34f;moon.shadows=LightShadows.None;moon.cullingMask=1<<9;
    }
    void Rail(string name,Vector3 p,Vector3 size)
    {
        bool alongX=size.x>size.z,glass=name.Contains("balcony");
        Vector3 axis=alongX?Vector3.right:Vector3.forward;
        float length=alongX?size.x:size.z,bottom=p.y-size.y/2;
        Vector3 a=new Vector3(p.x,bottom,p.z)-axis*(length/2-.025f),b=new Vector3(p.x,bottom,p.z)+axis*(length/2-.025f);
        if(glass) {
            // The original balcony barriers now render as solid stone parapets.
            // Keep one coping and handrail; do not stack another base or glass mesh.
            R("Balcony parapet stone coping",new Vector3(p.x,bottom+size.y+.025f,p.z),alongX?new Vector3(size.x+.025f,.05f,.22f):new Vector3(.22f,.05f,size.z+.025f),pbr.Travertine,.008f);
            Rod("Balcony parapet metal handrail",a+Vector3.up*(size.y+.10f),b+Vector3.up*(size.y+.10f),.025f,pbr.Metal);
            return;
        }
        Rod("Continuous metal handrail",a+Vector3.up*(size.y-.035f),b+Vector3.up*(size.y-.035f),.027f,pbr.Metal);
        Rod("Railing lower stretcher",a+Vector3.up*.12f,b+Vector3.up*.12f,.017f,pbr.Metal);
        int count=Mathf.Max(1,Mathf.CeilToInt(length/.16f));
        for(int i=0;i<=count;i++) {
            Vector3 foot=Vector3.Lerp(a,b,(float)i/count);
            Rod("Metal railing upright",foot,foot+Vector3.up*(size.y-.035f),.013f,pbr.Metal);
        }
    }

    void Stairs()
    {
        foreach(float x in new[]{-1.155f,1.155f}) {
            Rod("Upstairs walnut wall handrail",new Vector3(x,1.05f,.65f),new Vector3(x,4.25f,5.45f),.035f,pbr.Walnut);
            for(int i=0;i<5;i++) {
                float z=.8f+i*1.12f,y=1.15f+i*.7467f;
                Rod("Wall handrail brass bracket",new Vector3(Mathf.Sign(x)*1.2f,y-.10f,z),new Vector3(x,y,z),.015f,pbr.Brass);
            }
        }
        for(int i=0;i<17;i++) R("Upstairs brass stair nosing",new Vector3(0,(i+1)*.2f+.005f,.522f+i*.3f),new Vector3(2.28f,.01f,.035f),pbr.Brass,.004f);
        Rod("Basement first-flight handrail",new Vector3(1.48f,.85f,3),new Vector3(1.48f,-.95f,5.7f),.03f,pbr.Walnut);
        Rod("Basement return-flight handrail",new Vector3(3.72f,-.95f,5.7f),new Vector3(3.72f,-2.55f,3.3f),.03f,pbr.Walnut);
        Rod("Basement turn handrail",new Vector3(1.48f,-.95f,6.57f),new Vector3(3.72f,-.95f,6.57f),.03f,pbr.Walnut);
        foreach(float x in new[]{1.48f,3.72f}) Rod("Basement turn rail return",new Vector3(x,-.95f,5.7f),new Vector3(x,-.95f,6.57f),.03f,pbr.Walnut);
        for(int i=0;i<9;i++) R("Basement first-flight nosing",new Vector3(2,-(i+1)*.2f+.005f,3.022f+i*.3f),new Vector3(1.02f,.01f,.035f),pbr.Brass,.004f);
        for(int i=0;i<8;i++) R("Basement return-flight nosing",new Vector3(3.2f,-1.8f-(i+1)*.2f+.005f,5.678f-i*.3f),new Vector3(1.02f,.01f,.035f),pbr.Brass,.004f);
    }
    void Facade()
    {
        foreach(float x in new[]{-10.12f,10.12f}) {
            R("Facade horizontal floor band",new Vector3(x,3.25f,0),new Vector3(.12f,.22f,14.15f),stone);
            R("Facade roof fascia",new Vector3(x,6.72f,0),new Vector3(.18f,.2f,14.3f),stone);
        }
        foreach(float z in new[]{-7.12f,7.12f}) R("Facade roof end fascia",new Vector3(0,6.72f,z),new Vector3(20.3f,.2f,.18f),stone);
        // Balcony doorway thresholds follow the existing openings exactly.
        R("West balcony stone threshold",new Vector3(-10,3.414f,0),new Vector3(.24f,.028f,2.7f),stone,.006f);
        R("Front balcony stone threshold",new Vector3(0,3.414f,-7),new Vector3(2.7f,.028f,.24f),stone,.006f);
    }
    void Garden()
    {
        // Four strips, rather than a single plane that would cover the basement stair hole.
        R("West garden ground",new Vector3(-18,-.18f,0),new Vector3(15.6f,.08f,42),earth);
        R("East garden ground",new Vector3(24,-.18f,0),new Vector3(15.6f,.08f,42),earth);
        R("North garden ground",new Vector3(3,-.18f,14),new Vector3(26.4f,.08f,13.6f),earth);
        R("South garden ground",new Vector3(3,-.18f,-15),new Vector3(26.4f,.08f,15.6f),earth);
        for(int row=0;row<13;row++) foreach(float x in new[]{-.65f,.65f}) R("Courtyard paving slab",new Vector3(x,-.115f,-8.2f-row*.75f),new Vector3(1.26f,.045f,.71f),stone,.014f);
        foreach(float z in new[]{-18f,18f}) {
            R("Garden perimeter wall",new Vector3(2,1,z),new Vector3(48,2.2f,.25f),stone);
            R("Garden wall coping",new Vector3(2,2.14f,z),new Vector3(48,.1f,.35f),pbr.Marble);
            for(int i=0;i<9;i++) Post(new Vector3(-22+i*6,0,z));
        }
        foreach(float x in new[]{-22f,26f}) {
            R("Side garden perimeter wall",new Vector3(x,1,0),new Vector3(.25f,2.2f,36),stone);
            for(int i=0;i<7;i++) Post(new Vector3(x,0,-18+i*6));
        }
        // Closed gate is backdrop set dressing; rescue and exits retain their current rules.
        R("Front gate surround",new Vector3(0,1.35f,-17.82f),new Vector3(3.4f,2.7f,.10f),pbr.Metal);
        for(int i=0;i<13;i++) R("Front gate walnut slat",new Vector3(0,.12f+i*.20f,-17.74f),new Vector3(3.2f,.13f,.045f),pbr.Walnut,.008f);
        R("Front gate brass seam",new Vector3(0,1.35f,-17.7f),new Vector3(.025f,2.55f,.025f),pbr.Brass,.006f);
        for(int i=0;i<6;i++) {
            Tree(new Vector3(-19.5f,0,-13+i*5.2f),i);
            Tree(new Vector3(23.5f,0,-13+i*5.2f),i+6);
        }
        for(int i=0;i<7;i++) {float x=-8+i*3.8f;if(Mathf.Abs(x)>2) Hedge(new Vector3(x,0,-16.7f),1.0f+(i%3)*.13f);}
        for(int i=0;i<5;i++) Hedge(new Vector3(-13,0,-5+i*2.7f),.8f+(i%2)*.15f);
    }
    void Post(Vector3 p)
    {
        R("Perimeter stone pier",p+Vector3.up*1.15f,new Vector3(.38f,2.5f,.38f),stone);
        R("Garden lantern metal frame",p+Vector3.up*2.52f,new Vector3(.23f,.30f,.23f),pbr.Metal);
        R("Garden lantern luminous inset",p+Vector3.up*2.52f,new Vector3(.17f,.21f,.17f),pbr.WarmEmitter);
        R("Garden lantern cap",p+Vector3.up*2.69f,new Vector3(.28f,.045f,.28f),pbr.Metal);
    }
    void Tree(Vector3 p,int index)
    {
        float h=3.6f+(index%4)*.45f;
        L("Garden tree trunk",p,new[]{new Vector2(.22f,-.12f),new Vector2(.16f,1.5f),new Vector2(.085f,h)},pbr.Walnut,12);
        for(int i=0;i<3;i++) {
            float a=i*Mathf.PI*2/3+index;
            Vector3 canopy=p+new Vector3(Mathf.Sin(a)*.55f,h-.6f+i*.35f,Mathf.Cos(a)*.55f);
            var crown=L("Sculpted evergreen canopy",canopy,new[]{new Vector2(.2f,0),new Vector2(1.25f,.45f),new Vector2(1.45f,1.2f),new Vector2(.95f,2.05f),new Vector2(.08f,2.6f),new Vector2(0,2.6f)},leaves,14);
            crown.transform.localRotation=Quaternion.Euler((i-1)*8,0,(i-1)*6);
        }
    }
    void Hedge(Vector3 p,float height)
    {
        R("Raised stone planting bed",p+new Vector3(0,.1f,0),new Vector3(1.8f,.25f,.8f),stone,.045f);
        for(int i=0;i<3;i++) L("Trimmed garden shrub",p+new Vector3(-.54f+i*.54f,.2f,0),new[]{new Vector2(.12f,0),new Vector2(.36f,.18f),new Vector2(.38f,height*.62f),new Vector2(.2f,height),new Vector2(0,height*1.05f)},leaves,12);
    }
    void OnDestroy(){if(RenderSettings.skybox==sky) RenderSettings.skybox=null;}
}
