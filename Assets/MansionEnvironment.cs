using UnityEngine;
using System.Collections.Generic;

public sealed class MansionEnvironment : MonoBehaviour
{
    PbrLibrary pbr;
    Renderer powerLed;Light powerGlow,phoneGlow;
    Transform powerLever;
    Material offline,online;
    MansionRoomDetails roomDetails;
    ScannedAssetLibrary scans;
    EstateRenovation estate;
    public void Build(List<GameObject> collisionGeometry,PbrLibrary library)
    {
        pbr=library;online=pbr.GreenEmitter;offline=pbr.RedEmitter;
        scans=gameObject.AddComponent<ScannedAssetLibrary>();
        LivingRoomSlice.DisableLegacyArt(gameObject.scene);
        foreach(var box in collisionGeometry) {
            var r=box.GetComponent<Renderer>();string n=box.name.ToLowerInvariant();
            Vector3 p=box.transform.position,s=box.transform.localScale;
            if(box.name.StartsWith("Estate / ")) {
                r.enabled=false;
                var part=System.Array.Find(EstateRenovation.Plan.parts,item=>"Estate / "+item.name==box.name&&Vector3.Distance(EstateRenovation.V(item.p),p)<.01f);
                if(part!=null&&part.material!="barrier") {
                    var dressed=R(box.name+" / eased stone and timber",p,s,r.sharedMaterial,.012f);dressed.layer=part.layer;
                }
                continue;
            }
            if(n.Contains("window")&&n.Contains("glass collision")){r.enabled=false;continue;}
            if(n.StartsWith("lounge chair")&&scans.Place("modern_arm_chair_01",new Vector3(p.x,0,p.z),new Vector3(.85f,1.05f,1),p.x>6?-90:90)){r.enabled=false;continue;}
            if(n=="living room planter collision"&&scans.Place("potted_plant_01",new Vector3(p.x,0,p.z),new Vector3(.65f,1.45f,.65f))){r.enabled=false;continue;}
            if(box.name.StartsWith("Balcony safety barrier")){r.enabled=false;continue;}
            if(MansionArchitecture.IsRailing(box.name)) {
                // Balcony enclosure uses its existing solid barrier mesh/collider.
                bool balcony=n.Contains("balcony");
                r.enabled=balcony;
                if(balcony){r.sharedMaterial=pbr.Travertine;box.layer=9;}
                continue;
            }
            bool thinPlaster=r.sharedMaterial==pbr.Plaster&&Mathf.Min(s.x,s.z)<=.3f&&s.y>=1.8f;
            bool architecture=thinPlaster||n.Contains("wall")||n.Contains("partition")||n.Contains("lintel")||n.Contains("divider")||n.Contains("roof");
            bool floor=r.sharedMaterial==pbr.Floor||n.Contains("floor")||n.Contains("slab")||n.Contains("landing")&&!n.Contains("guard")&&!n.Contains("wall");
            bool stair=n.Contains("stair ")||n.Contains("flight ");
            if(architecture) {
                r.sharedMaterial=n.StartsWith("north wall")||n.StartsWith("south wall")?pbr.Travertine:pbr.Plaster;
                if(s.y>2&&Mathf.Min(s.x,s.z)<.3f) Trim(p,s);
                if(n.Contains("lintel")) DoorFrame(p,s);
            } else if(floor) {
                r.sharedMaterial=n.Contains("balcony")?pbr.Travertine:pbr.Floor;
                if(n.Contains("balcony"))box.layer=9;
            }
            else if(stair) r.sharedMaterial=pbr.Walnut;
            else {
                bool custom=n.StartsWith("basement play / ")||n=="main entrance locked door"||n=="living sofa"||n=="coffee table"||n=="dining table"||n=="telephone desk"||n=="police telephone"||n=="security fuse box"||n=="parked car collision"||n=="passage motion sensor"||MansionRoomDetails.Replaces(box.name)||HidingDecor.Replaces(box.name);
                if(!custom) {
                    var mat=r.sharedMaterial;
                    if(n.Contains("mattress")||n.Contains("pillow")||n.Contains("blanket")) mat=n.Contains("blanket")?pbr.Fabric:pbr.Linen;
                    if(n.Contains("toilet")||n.Contains("basin")||n.Contains("washing")||n.Contains("dryer")) mat=pbr.Linen;
                    if(n.Contains("monitor")||n.Contains("screen")) mat=pbr.Glass;
                    if(n.Contains("pillow")||n.Contains("mattress"))CoutureMesh.Cushion(box.name+" / filled cloth",p,s,Mathf.Min(.12f,s.y*.45f),mat,transform);
                    else VisualMesh.Rounded(box.name+" / eased edges",p,s,Mathf.Min(.065f,s.y*.16f),mat,transform);
                }
                r.enabled=false;
            }
        }
        // Ceiling panels preserve both stair openings.
        R("West plaster ceiling",new Vector3(-5.7f,3.13f,0),new Vector3(8.6f,.2f,14),pbr.Plaster,.01f);
        R("East plaster ceiling",new Vector3(5.7f,3.13f,0),new Vector3(8.6f,.2f,14),pbr.Plaster,.01f);
        R("Hall ceiling",new Vector3(0,3.13f,-3.3f),new Vector3(2.8f,.2f,7.4f),pbr.Plaster,.01f);
        R("Garage ceiling",new Vector3(13,3.13f,0),new Vector3(6,.2f,14),pbr.Plaster,.01f);
        Sofa();Tables();Kitchen();Television();Car();Interactables();
        var livingRug=pbr.Tint(pbr.Fabric,pbr.Fabric.color,"Living room / rug contact shading");
        // The archived contact map belongs to the removed replacement layout.
        livingRug.SetFloat("_UseLocalAO",0);
        R("Woven living rug",new Vector3(6,.012f,-.45f),new Vector3(4.9f,.014f,3.5f),livingRug,.006f);
        var window=pbr.ResourceMaterial("Presentation/ArchitecturalGlass","Clear night window glass");window.color=new Color(.16f,.25f,.28f,.075f);
        foreach(float x in new[]{-4.5f,7f}) {
            foreach(float side in new[]{-1f,1f})R("Window vertical timber jamb",new Vector3(x+side*1.1f,1.8f,6.84f),new Vector3(.1f,1.65f,.17f),pbr.Walnut,.012f);
            foreach(float y in new[]{1.025f,2.575f})R("Window horizontal timber jamb",new Vector3(x,y,6.84f),new Vector3(2.3f,.1f,.17f),pbr.Walnut,.012f);
            R("Clear night glass",new Vector3(x,1.8f,6.94f),new Vector3(2.1f,1.45f,.015f),window,.004f);
            R("Window mullion",new Vector3(x,1.8f,6.70f),new Vector3(.045f,1.5f,.04f),pbr.Metal,.01f);
        }
        // Recognizable laundry fronts, detailed pool table and office screen.
        foreach(float x in new[]{7.4f,8.7f}) {
            var rim=Lathe("Laundry chrome drum rim",new Vector3(x,-2.86f,5.37f),new[]{new Vector2(.32f,0),new Vector2(.34f,.03f),new Vector2(.29f,.06f),new Vector2(.25f,.04f)},pbr.Metal);
            rim.transform.localRotation=Quaternion.Euler(90,0,0);
            R("Laundry glass door",new Vector3(x,-2.86f,5.36f),new Vector3(.48f,.48f,.035f),pbr.Glass,.2f);
            R("Laundry control panel",new Vector3(x,-2.55f,5.37f),new Vector3(.8f,.12f,.04f),pbr.Metal);
        }
        R("Office monitor glow",new Vector3(-8,-2.1f,5.925f),new Vector3(1.29f,.69f,.015f),pbr.ScreenEmitter,.01f);
        for(int i=0;i<6;i++) Ball("Billiard ball",new Vector3(-3.55f+(i%3)*.23f,-2.41f,2.1f+(i/3)*.25f),new Vector3(.095f,.095f,.095f),i%2==0?pbr.Linen:pbr.Brass);
        roomDetails=gameObject.AddComponent<MansionRoomDetails>();roomDetails.Build(pbr);
        gameObject.AddComponent<MansionArchitecture>().Build(pbr,collisionGeometry);
        gameObject.AddComponent<HidingDecor>().Build(pbr);
        gameObject.AddComponent<MansionFinishDetails>().Build(pbr,collisionGeometry);
        gameObject.AddComponent<MansionEstateDetails>().Build(pbr,scans);
        estate=new GameObject("Courtyard estate / redesigned architecture and grounds").AddComponent<EstateRenovation>();estate.transform.SetParent(transform,false);
        estate.Build(pbr,scans,Camera.main);
        var staticArt=new List<Renderer>(GetComponentsInChildren<Renderer>());
        foreach(var box in collisionGeometry) staticArt.Add(box.GetComponent<Renderer>());
        gameObject.AddComponent<StaticVisualBatch>().Build(staticArt);
    }
    GameObject R(string name,Vector3 p,Vector3 s,Material material,float radius=.04f) => VisualMesh.Rounded(name,p,s,radius,material,transform);
    GameObject Lathe(string name,Vector3 p,Vector2[] profile,Material m) => VisualMesh.Lathe(name,p,profile,m,transform);
    GameObject Ball(string name,Vector3 p,Vector3 size,Material m)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;go.transform.SetParent(transform,false);go.transform.localPosition=p;go.transform.localScale=size;
        go.GetComponent<Renderer>().sharedMaterial=m;var c=go.GetComponent<Collider>();if(c!=null){c.enabled=false;Destroy(c);}return go;
    }
    void Trim(Vector3 p,Vector3 s)
    {
        bool alongX=s.x>s.z;float bottom=p.y-s.y/2;
        foreach(float side in new[]{-1f,1f}) {
            Vector3 offset=alongX?new Vector3(0,0,side*(s.z/2+.018f)):new Vector3(side*(s.x/2+.018f),0,0);
            Vector3 size=alongX?new Vector3(s.x,.14f,.035f):new Vector3(.035f,.14f,s.z);
            R("Recessed walnut skirting",new Vector3(p.x,bottom+.07f,p.z)+offset,size,pbr.Walnut,.006f);
            size.y=.065f;
            R("Crown moulding",new Vector3(p.x,p.y+s.y/2-.09f,p.z)+offset,size,pbr.Linen,.015f);
        }
    }
    void DoorFrame(Vector3 p,Vector3 s)
    {
        bool alongX=s.x>s.z;float height=p.y-s.y/2;
        float bottom=height>3?3.4f:height<0?-3.4f:0;
        if(height-bottom<1.8f)return;
        bool upper=bottom>3;
        float width=alongX?s.x:s.z;
        Material finish=upper?pbr.Travertine:pbr.Walnut;
        foreach(float sign in new[]{-1f,1f}) {
            Vector3 q=p+(alongX?Vector3.right:Vector3.forward)*sign*(width/2-.035f);
            q.y=(height+bottom)/2;
            R(upper?"Upper doorway stone jamb":"Door casing",q,alongX?new Vector3(.065f,height-bottom,s.z+.035f):new Vector3(s.x+.035f,height-bottom,.065f),finish,.008f);
            if(upper) {
                // Slim timber rebate remains within the existing casing footprint.
                var rebate=q+(alongX?Vector3.right:Vector3.forward)*sign*.020f;
                R("Upper doorway recessed timber rebate",rebate,alongX?new Vector3(.012f,height-bottom,s.z+.045f):new Vector3(s.x+.045f,height-bottom,.012f),pbr.Walnut,.003f);
            }
        }
        R(upper?"Upper doorway stone head":"Door casing header",new Vector3(p.x,height-.025f,p.z),alongX?new Vector3(width,.075f,s.z+.04f):new Vector3(s.x+.04f,.075f,width),finish,.008f);
    }
    void Sofa()
    {
        if(scans.Place("sofa_03",new Vector3(7,0,-3),new Vector3(3.96f,1.43f,1.37f)))return;
        foreach(float x in new[]{5.3f,8.7f}) foreach(float z in new[]{-3.48f,-2.53f}) R("Sofa brushed leg",new Vector3(x,.14f,z),new Vector3(.06f,.28f,.06f),pbr.Brass,.01f);
        R("Sofa upholstered plinth",new Vector3(7,.39f,-3),new Vector3(3.96f,.5f,1.37f),pbr.Fabric,.1f);
        R("Sofa tailored back",new Vector3(7,.97f,-3.52f),new Vector3(3.95f,.93f,.28f),pbr.Fabric,.12f);
        foreach(float x in new[]{5.19f,8.81f}) R("Sofa rounded arm",new Vector3(x,.72f,-3),new Vector3(.32f,.73f,1.36f),pbr.Fabric,.12f);
        for(int i=0;i<3;i++) {
            float x=5.83f+i*1.17f;
            R("Separate seat cushion",new Vector3(x,.73f,-2.93f),new Vector3(1.1f,.24f,.98f),pbr.Fabric,.09f);
            var cushion=R("Loose back cushion",new Vector3(x,1.03f,-3.28f),new Vector3(1.06f,.66f,.2f),pbr.Linen,.085f);cushion.transform.localRotation=Quaternion.Euler(-8,0,0);
            R("Cushion piping",new Vector3(x,.73f,-2.438f),new Vector3(1.01f,.012f,.009f),pbr.Linen,.004f);
        }
    }
    void Tables()
    {
        float coffeeTop=.7835f;
        foreach(var p in new[]{new Vector3(6,.70f,0),new Vector3(-4,1.03f,0),new Vector3(7,.83f,4)}) {
            if(p.x==6&&p.z==0&&scans.Place("modern_coffee_table_01",new Vector3(6,0,0),new Vector3(1,.65f,2),90)){coffeeTop=scans.FittedSize("modern_coffee_table_01",new Vector3(1,.65f,2)).y;continue;}
            if(p.x<0&&scans.Place("dining_table",new Vector3(-4,0,0),new Vector3(2.4f,.78f,1.4f),90))continue;
            bool dining=p.x<0;Vector3 size=dining?new Vector3(2.3f,.13f,3):new Vector3(2,.12f,p.z>1?1:1.5f);
            R("Eased walnut table edge",p,size,pbr.Walnut,.045f);
            R("Honed stone table insert",p+Vector3.up*.071f,new Vector3(size.x-.09f,.025f,size.z-.09f),pbr.Marble,.012f);
            foreach(float x in new[]{-.8f,.8f}) foreach(float z in new[]{-.35f,.35f})
                R("Table tapered metal leg",p+new Vector3(x,-p.y/2,z*(dining?3:1)),new Vector3(.065f,p.y-.1f,.065f),pbr.Metal,.012f);
        }
        // MansionEstateDetails owns the six dining place settings.
        if(!scans.Place("ceramic_vase_03",new Vector3(5.5f,coffeeTop+.002f,0),new Vector3(.32f,.34f,.32f)))
            Lathe("Sculpted ceramic vase",new Vector3(5.5f,coffeeTop+.002f,0),new[]{new Vector2(0,0),new Vector2(.09f,0),new Vector2(.14f,.13f),new Vector2(.06f,.3f),new Vector2(.052f,.34f),new Vector2(.04f,.34f),new Vector2(.04f,.3f)},pbr.Porcelain);
        R("Linen bound book",new Vector3(6.45f,coffeeTop+.035f,.15f),new Vector3(.4f,.07f,.55f),pbr.Fabric,.012f);
    }
    void Kitchen()
    {
        R("Bevelled marble worktop",new Vector3(-8.8f,1.035f,(1)+2.5f),new Vector3(1.55f,.09f,5),pbr.Marble,.04f);
        for(int i=0;i<5;i++) {
            R("Walnut cabinet front",new Vector3(-8.025f,.51f,(-1+i)+2.5f),new Vector3(.045f,.83f,.94f),pbr.Walnut,.014f);
            R("Brass cabinet pull",new Vector3(-7.985f,.79f,(-1+i)+2.5f),new Vector3(.045f,.035f,.28f),pbr.Brass,.012f);
        }
        R("Induction glass",new Vector3(-8.8f,1.09f,(-.2f)+2.5f),new Vector3(1.16f,.025f,1.16f),pbr.Glass,.02f);
        foreach(float z in new[]{-.48f,.08f}) foreach(float x in new[]{-9.08f,-8.52f})
            Lathe("Induction hob ring",new Vector3(x,1.106f,(z)+2.5f),new[]{new Vector2(.14f,0),new Vector2(.145f,.004f),new Vector2(.14f,.008f)},pbr.Metal);
        R("Sink rim",new Vector3(-8.8f,1.095f,(2.4f)+2.5f),new Vector3(1.1f,.025f,.85f),pbr.Metal,.035f);
        R("Dark recessed sink",new Vector3(-8.8f,1.111f,(2.4f)+2.5f),new Vector3(.96f,.008f,.7f),pbr.Rubber,.08f);
        R("Faucet stem",new Vector3(-9.35f,1.24f,(2.4f)+2.5f),new Vector3(.055f,.3f,.055f),pbr.Metal,.02f);
        R("Faucet neck",new Vector3(-9.2f,1.39f,(2.4f)+2.5f),new Vector3(.33f,.055f,.055f),pbr.Metal,.02f);
    }
    void Wardrobe()
    {
        foreach(float x in new[]{-8.54f,-7.46f}) {
            R("Wardrobe raised inset",new Vector3(x,1.2f,5.14f),new Vector3(.84f,2.15f,.025f),pbr.Walnut,.016f);
            R("Wardrobe brass handle",new Vector3(x+(x<-8?.32f:-.32f),1.15f,5.105f),new Vector3(.035f,.29f,.055f),pbr.Brass,.014f);
        }
    }
    void Television()
    {
        R("TV aluminium bezel",new Vector3(9.78f,1.65f,3),new Vector3(.07f,1.27f,2.32f),pbr.Metal,.025f);
        R("TV glossy black screen",new Vector3(9.735f,1.65f,3),new Vector3(.016f,1.16f,2.21f),pbr.Glass,.007f);
    }
    void Car(){gameObject.AddComponent<GarageVehicle>().Build(pbr,transform);}
    Light Glow(string name,Vector3 p,Color color,float intensity,float range)
    {
        var l=new GameObject(name).AddComponent<Light>();l.transform.SetParent(transform);l.transform.position=p;l.type=LightType.Point;l.range=range;l.intensity=intensity;l.color=color;l.shadows=LightShadows.None;return l;
    }
    void Interactables()
    {
        gameObject.AddComponent<DeskTelephoneModel>().Build(pbr);
        phoneGlow=Glow("Telephone screen spill",new Vector3(7,1.22f,4),new Color(.16f,.8f,.65f),.32f,.95f);
        Vector3 panel=MainPowerLayout.Panel;
        R("Security steel enclosure",panel,new Vector3(.8f,1,.3f),pbr.Metal,.025f);
        R("Security inset door",panel+new Vector3(0,0,.163f),new Vector3(.7f,.86f,.025f),pbr.Rubber,.012f);
        powerLed=R("Security status diode",panel+new Vector3(-.22f,.3f,.185f),new Vector3(.055f,.055f,.025f),online,.02f).GetComponent<Renderer>();
        powerLever=R("Security breaker lever",panel+new Vector3(.15f,-.02f,.205f),new Vector3(.12f,.23f,.08f),pbr.Brass,.025f).transform;
        powerLever.localRotation=Quaternion.Euler(-25,0,0);
        powerGlow=Glow("Security diode wall spill",panel+new Vector3(-.15f,.3f,.3f),new Color(.08f,.8f,.48f),.35f,1);
        R("Passage PIR housing",new Vector3(1.15f,2.45f,.85f),new Vector3(.10f,.20f,.22f),pbr.Linen,.025f);
        Ball("Passage PIR lens",new Vector3(1.09f,2.43f,.85f),new Vector3(.025f,.09f,.12f),pbr.Glass);
    }
    bool telephoneMoved;
    public void MoveTelephoneToBasement()
    {
        if(telephoneMoved)return;telephoneMoved=true;
        Vector3 offset=new Vector3(-16.02f,-3.39f,1.65f);
        foreach(Transform child in transform)if(child.name.StartsWith("Telephone "))child.position+=offset;
        foreach(var box in FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))if(box.name=="Police telephone")box.transform.position+=offset;
        Physics.SyncTransforms();
    }
    public void SetPower(bool powered)
    {
        if(roomDetails!=null) roomDetails.SetPower(powered);
        if(estate!=null)estate.SetPower(powered);
        pbr.ScreenEmitter.SetColor("_EmissionColor",powered?new Color(.17f,.64f,.57f)*1.7f:Color.black);
        pbr.WarmEmitter.SetColor("_EmissionColor",powered?new Color(1,.67f,.33f)*3:Color.black);
        if(phoneGlow!=null)phoneGlow.intensity=powered?.32f:0;
        if(powerLever!=null)powerLever.localRotation=Quaternion.Euler(powered?-25:25,0,0);
        if(powerLed==null) return;powerLed.sharedMaterial=powered?online:offline;
        powerGlow.intensity=powered?.35f:0;
        powerGlow.color=powered?new Color(.08f,.8f,.48f):new Color(1,.12f,.035f);
    }
}
