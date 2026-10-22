using UnityEngine;

// Static set dressing uses the existing furniture footprints. No colliders or AI sources.
public sealed class MansionRoomDetails : MonoBehaviour
{
    PbrLibrary pbr;
    ScannedAssetLibrary scans;
    Material blue,sage,pool,porcelain,lamp;
    bool powered=true;int bedroomMask=3;readonly Material[] bedroomLamps=new Material[2];
    GameObject R(string n,Vector3 p,Vector3 s,Material m,float bevel=.025f) => VisualMesh.Rounded(n,p,s,bevel,m,transform);
    GameObject L(string n,Vector3 p,Vector2[] shape,Material m,int segments=20,float depth=1) => VisualMesh.Lathe(n,p,shape,m,transform,segments,depth);
    void Rod(string n,Vector3 a,Vector3 b,float radius,Material m)
    {
        float length=Vector3.Distance(a,b);
        var go=L(n,a,new[]{new Vector2(0,0),new Vector2(radius,0),new Vector2(radius,length),new Vector2(0,length)},m,12);
        go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
    }
    public static bool Replaces(string name)
    {
        return name=="Pillow"||name=="Mattress"||name=="Bed headboard"||name=="Bed blanket"||name=="Bedside cabinet"||name=="Bedroom dresser"
            ||name.EndsWith(" basin")||name.EndsWith(" toilet")||name=="Office desk"||name=="Office chair"
            ||name=="Pool table"||name=="Pool cloth"||name=="Gym bench"||name=="Weight rack"
            ||name=="Storage shelving"||name=="Workshop bench"||name=="Workshop tool board"
            ||name=="Utility heater"||name=="Safe room cabinet"
            ||name=="Library bookcase collision"||name=="Library reading seat collision"
            ||name=="Studio desk collision"||name=="Studio chair collision";
    }
    public void Build(PbrLibrary library)
    {
        pbr=library;
        scans=GetComponent<ScannedAssetLibrary>();
        blue=pbr.Tint(pbr.Fabric,new Color(.38f,.44f,.49f),"Bedroom / midnight woven fabric");
        sage=pbr.Tint(pbr.Fabric,new Color(.43f,.48f,.39f),"Bedroom / sage woven fabric");
        pool=pbr.Tint(pbr.Fabric,new Color(.045f,.22f,.15f),"Billiard felt");
        porcelain=pbr.Porcelain;
        lamp=pbr.Emitter("Bedside diffusers / switched",new Color(1,.67f,.33f),1.5f);
        Bedroom(-6,blue);Bedroom(5,sage);
        Bathroom(-4.2f,-1.4f,2.5f,5.5f);Bathroom(7.4f,9.9f,2.5f,6.8f);
        Office();Billiards();Gym();Storage();Workshop();Utility();Safe();
    }
    void Bedroom(float x,Material accent)
    {
        int room=x<0?0:1;
        bedroomLamps[room]=pbr.Emitter("Bedroom "+room+" / switched bedside diffuser",new Color(1,.67f,.33f),1.5f);
        int firstBed=transform.childCount;
        R("Upholstered headboard frame",new Vector3(x,4.3f,.35f),new Vector3(2.8f,1.3f,.15f),pbr.Walnut,.045f);
        for(int i=0;i<6;i++) R("Headboard stitched vertical panel",new Vector3(x-1.14f+i*.456f,4.31f,.252f),new Vector3(.43f,1.18f,.05f),accent,.02f);
        CoutureMesh.Cushion("Soft stitched mattress",new Vector3(x,4,-1.5f),new Vector3(2.5f,.3f,3.6f),.12f,pbr.Linen,transform);
        foreach(float pillow in new[]{-.65f,.65f})CoutureMesh.Cushion("Soft linen pillow",new Vector3(x+pillow,4.23f,-.25f),new Vector3(1,.18f,.6f),.08f,pbr.Linen,transform);
        CoutureMesh.DrapedBlanket("Continuous woven duvet",new Vector3(x,4.185f,-2.015f),2.58f,2.63f,accent,transform);
        R("Folded throw hem",new Vector3(x,4.224f,-.84f),new Vector3(2.45f,.035f,.17f),pbr.Linen,.014f);
        foreach(float edge in new[]{-1.18f,1.18f}) R("Throw stitched seam",new Vector3(x+edge,4.213f,-2),new Vector3(.012f,.008f,2.3f),pbr.Linen,.003f);
        float side=x+1.9f;
        if(scans!=null)scans.Place("alarm_clock_01",new Vector3(side-.18f,4.21f,-.13f),new Vector3(.22f,.18f,.10f),180);
        R("Bedside cabinet carcass",new Vector3(side,3.8f,.1f),new Vector3(.7f,.8f,.7f),pbr.Walnut);
        foreach(float y in new[]{3.63f,3.98f}) {
            R("Bedside drawer front",new Vector3(side,y,-.264f),new Vector3(.62f,.30f,.025f),accent,.01f);
            R("Bedside brass drawer pull",new Vector3(side,y,-.29f),new Vector3(.16f,.025f,.025f),pbr.Brass,.008f);
        }
        L("Bedside lamp foot",new Vector3(side,4.21f,.12f),new[]{new Vector2(0,0),new Vector2(.13f,0),new Vector2(.13f,.025f),new Vector2(.04f,.055f),new Vector2(.025f,.25f)},pbr.Brass);
        L("Bedside linen lampshade",new Vector3(side,4.43f,.12f),new[]{new Vector2(.18f,0),new Vector2(.12f,.25f),new Vector2(.105f,.25f),new Vector2(.16f,0)},pbr.Linen);
        L("Bedside glowing diffuser",new Vector3(side,4.445f,.12f),new[]{new Vector2(0,0),new Vector2(.14f,0),new Vector2(.14f,.01f),new Vector2(0,.01f)},bedroomLamps[room]);
        MoveChildren(firstBed,new Vector3(x,0,-1.5f),new Vector3(x,0,-4.88f),180);
        if(x<0)for(int i=firstBed;i<transform.childCount;i++) {
            var child=transform.GetChild(i);
            if(child.name.StartsWith("Bedside")||child.name.StartsWith("alarm_clock"))child.localPosition+=Vector3.right*3.8f;
        }
        int firstDresser=transform.childCount;
        Vector3 dresserLimits=new Vector3(2.4f,1.3f,.8f);
        bool detailedDresser=scans!=null&&scans.Place("modern_wooden_cabinet",new Vector3(x,3.4f,-6.35f),dresserLimits);
        float dresserTop=4.09f;
        if(detailedDresser)dresserTop=3.4f+scans.FittedSize("modern_wooden_cabinet",dresserLimits).y;
        else {
            R("Dresser walnut case",new Vector3(x,3.745f,-6.35f),new Vector3(2.4f,.69f,.53f),pbr.Walnut,.025f);
            for(int row=0;row<2;row++)foreach(float column in new[]{-.59f,.59f}) {
                float y=3.59f+row*.30f;
                R("Dresser separate drawer",new Vector3(x+column,y,-6.074f),new Vector3(1.13f,.27f,.025f),pbr.Walnut,.01f);
                R("Dresser brass handle",new Vector3(x+column,y,-6.047f),new Vector3(.24f,.025f,.03f),pbr.Brass,.009f);
            }
        }
        for(int i=0;i<3;i++)R("Dresser stacked book",new Vector3(x-.7f,dresserTop+.024f+i*.045f,-6.3f),new Vector3(.4f,.042f,.29f),i%2==0?accent:pbr.Linen,.007f);
        MoveChildren(firstDresser,new Vector3(x,0,-6.35f),x>0?new Vector3(9.55f,0,-1.5f):new Vector3(-9.55f,0,-3.25f),x>0?-90:90);
        // South corner wall space now belongs to the real hiding cupboards.
    }
    // Transform complete assemblies, including scanned assets, before static batching.
    void MoveChildren(int start,Vector3 oldAnchor,Vector3 newAnchor,float yaw)
    {
        var rotation=Quaternion.Euler(0,yaw,0);
        for(int i=start;i<transform.childCount;i++) {
            var child=transform.GetChild(i);
            child.localPosition=newAnchor+rotation*(child.localPosition-oldAnchor);
            child.localRotation=rotation*child.localRotation;
        }
    }
    void Art(Vector3 center,Material accent)
    {
        R("Framed geometric print surround",center,new Vector3(1.8f,.92f,.065f),pbr.Walnut,.014f);
        R("Print linen mount",center+Vector3.forward*.04f,new Vector3(1.68f,.8f,.012f),pbr.Linen,.004f);
        R("Print abstract landscape",center+new Vector3(-.18f,-.16f,.05f),new Vector3(1.1f,.28f,.008f),accent,.003f);
        R("Print abstract horizon",center+new Vector3(.25f,.02f,.055f),new Vector3(.9f,.045f,.008f),pbr.Brass,.003f);
        var sun=L("Print brass disc",center+new Vector3(.40f,.17f,.055f),new[]{new Vector2(0,0),new Vector2(.12f,0),new Vector2(.12f,.005f),new Vector2(0,.005f)},pbr.Brass);
        sun.transform.localRotation=Quaternion.Euler(90,0,0);
    }
    void Bathroom(float left,float right,float south,float north)
    {
        float mid=(left+right)/2;
        R("Floating basin vanity",new Vector3(mid,3.86f,north-.5f),new Vector3(.9f,.86f,.55f),pbr.Walnut);
        R("Basin vanity marble",new Vector3(mid,4.305f,north-.5f),new Vector3(.9f,.045f,.55f),pbr.Marble,.02f);
        L("Shaped ceramic vessel basin",new Vector3(mid,4.33f,north-.5f),new[]{new Vector2(.16f,0),new Vector2(.22f,.035f),new Vector2(.25f,.12f),new Vector2(.23f,.13f),new Vector2(.19f,.05f),new Vector2(.05f,.035f),new Vector2(0,.035f)},porcelain,28,.78f);
        Rod("Basin tap upright",new Vector3(mid,4.34f,north-.32f),new Vector3(mid,4.59f,north-.32f),.016f,pbr.Metal);
        Rod("Basin tap spout",new Vector3(mid,4.59f,north-.32f),new Vector3(mid,4.59f,north-.48f),.016f,pbr.Metal);
        R("Bathroom mirror surround",new Vector3(mid,5.05f,north-.10f),new Vector3(.84f,.88f,.04f),pbr.Brass,.02f);
        R("Bathroom smoked mirror face",new Vector3(mid,5.05f,north-.126f),new Vector3(.77f,.81f,.012f),pbr.Glass,.01f);
        float tx=right-.55f,tz=south+(right>9?1.95f:.8f);
        L("Toilet sculpted pedestal",new Vector3(tx,3.4f,tz),new[]{new Vector2(.20f,0),new Vector2(.15f,.12f),new Vector2(.18f,.29f),new Vector2(.30f,.39f),new Vector2(.31f,.47f)},porcelain,28,1.15f);
        R("Closed porcelain toilet lid",new Vector3(tx,3.9f,tz-.02f),new Vector3(.60f,.07f,.67f),porcelain,.10f);
        R("Toilet concealed cistern",new Vector3(tx,3.86f,tz+.29f),new Vector3(.5f,.25f,.14f),porcelain,.03f);
        R("Dual flush plate",new Vector3(tx,4.0f,tz+.29f),new Vector3(.15f,.012f,.055f),pbr.Metal,.008f);
    }
    void Office()
    {
        R("Office desk eased top",new Vector3(-8,-2.53f,5.8f),new Vector3(2.6f,.07f,1),pbr.Walnut);
        foreach(float x in new[]{-9.1f,-6.9f}) foreach(float z in new[]{5.45f,6.15f}) Rod("Office desk leg",new Vector3(x,-3.4f,z),new Vector3(x,-2.56f,z),.035f,pbr.Metal);
        R("Monitor stand foot",new Vector3(-8,-2.475f,6),new Vector3(.45f,.025f,.32f),pbr.Metal,.01f);
        Rod("Monitor stand upright",new Vector3(-8,-2.46f,6.04f),new Vector3(-8,-2.22f,6.04f),.025f,pbr.Metal);
        R("Keyboard chassis",new Vector3(-8.15f,-2.465f,5.5f),new Vector3(.66f,.045f,.21f),pbr.Rubber,.016f);
        for(int row=0;row<3;row++) for(int col=0;col<10;col++) R("Keyboard keycap",new Vector3(-8.43f+col*.062f,-2.435f,5.44f+row*.061f),new Vector3(.049f,.018f,.045f),pbr.Hair,.006f);
        R("Mouse mat",new Vector3(-7.54f,-2.49f,5.52f),new Vector3(.33f,.008f,.27f),blue,.003f);
        R("Contoured mouse",new Vector3(-7.54f,-2.45f,5.52f),new Vector3(.085f,.07f,.13f),pbr.Hair,.03f);
        R("Office chair seat",new Vector3(-8,-2.94f,4.7f),new Vector3(.61f,.13f,.60f),blue,.055f);
        R("Office chair lumbar back",new Vector3(-8,-2.72f,4.96f),new Vector3(.58f,.4f,.075f),blue,.03f);
        Rod("Office chair piston",new Vector3(-8,-3.33f,4.7f),new Vector3(-8,-3,4.7f),.04f,pbr.Metal);
        for(int i=0;i<5;i++) {float a=i*Mathf.PI*2/5;Rod("Chair star base",new Vector3(-8,-3.33f,4.7f),new Vector3(-8+Mathf.Sin(a)*.29f,-3.33f,4.7f+Mathf.Cos(a)*.29f),.025f,pbr.Metal);}
    }
    void Billiards()
    {
        R("Billiards walnut body",new Vector3(-3.2f,-2.85f,2.7f),new Vector3(1.4f,.65f,2.6f),pbr.Walnut,.06f);
        R("Billiards dark green cloth",new Vector3(-3.2f,-2.48f,2.7f),new Vector3(1.21f,.04f,2.4f),pool,.01f);
        foreach(float x in new[]{-3.86f,-2.54f}) R("Billiards cushioned side rail",new Vector3(x,-2.46f,2.7f),new Vector3(.08f,.08f,2.58f),pbr.Walnut,.025f);
        foreach(float z in new[]{1.44f,3.96f}) R("Billiards end rail",new Vector3(-3.2f,-2.46f,z),new Vector3(1.25f,.08f,.08f),pbr.Walnut,.025f);
        foreach(float x in new[]{-3.77f,-2.63f}) foreach(float z in new[]{1.56f,2.7f,3.84f}) L("Billiards dark pocket",new Vector3(x,-2.447f,z),new[]{new Vector2(0,0),new Vector2(.072f,0),new Vector2(.072f,.006f),new Vector2(0,.006f)},pbr.Rubber,16);
        Rod("Pool cue resting on rail",new Vector3(-2.56f,-2.39f,1.6f),new Vector3(-2.56f,-2.39f,3.8f),.012f,pbr.Brass);
    }
    void Gym()
    {
        R("Gym bench stitched pad",new Vector3(-8,-2.7f,-3),new Vector3(.73f,.2f,2.1f),pbr.Hair,.07f);
        Rod("Gym bench spine",new Vector3(-8,-2.87f,-3.9f),new Vector3(-8,-2.87f,-2.1f),.065f,pbr.Metal);
        foreach(float z in new[]{-3.8f,-2.2f}) {
            Rod("Gym bench support",new Vector3(-8,-3.33f,z),new Vector3(-8,-2.8f,z),.05f,pbr.Metal);
            Rod("Gym bench stabilizer",new Vector3(-8.48f,-3.33f,z),new Vector3(-7.52f,-3.33f,z),.05f,pbr.Rubber);
        }
        foreach(float z in new[]{-6.25f,-4.75f}) foreach(float x in new[]{-9.5f,-8.9f}) Rod("Dumbbell rack upright",new Vector3(x,-3.4f,z),new Vector3(x,-1.84f,z),.035f,pbr.Metal);
        foreach(float y in new[]{-2.9f,-2.25f}) {
            R("Dumbbell rack shelf",new Vector3(-9.2f,y,-5.5f),new Vector3(.7f,.045f,1.7f),pbr.Metal,.012f);
            foreach(float z in new[]{-6.05f,-5.5f,-4.95f}) {
                Rod("Knurled dumbbell grip",new Vector3(-9.46f,y+.14f,z),new Vector3(-8.94f,y+.14f,z),.021f,pbr.Brass);
                foreach(float x in new[]{-9.43f,-8.97f}) Rod("Dumbbell weight plate",new Vector3(x-.04f,y+.14f,z),new Vector3(x+.04f,y+.14f,z),.12f,pbr.Rubber);
            }
        }
    }
    void Storage()
    {
        foreach(float x in new[]{4.23f,4.77f}) foreach(float z in new[]{3.85f,6.15f}) Rod("Storage rack upright",new Vector3(x,-3.4f,z),new Vector3(x,-1.4f,z),.025f,pbr.Metal);
        foreach(float y in new[]{-3.32f,-2.7f,-2.08f,-1.46f}) R("Storage shelf plank",new Vector3(4.5f,y,5),new Vector3(.58f,.045f,2.4f),pbr.Walnut,.01f);
        foreach(float y in new[]{-3.09f,-2.47f,-1.85f}) foreach(float z in new[]{4.15f,4.95f,5.75f}) {
            R("Archive box",new Vector3(4.5f,y,z),new Vector3(.48f,.38f,.6f),pbr.Linen,.012f);
            R("Archive box lid",new Vector3(4.5f,y+.19f,z),new Vector3(.49f,.025f,.61f),pbr.Walnut,.006f);
            R("Archive box pull",new Vector3(4.245f,y,z),new Vector3(.015f,.045f,.15f),pbr.Metal,.004f);
        }
    }
    void Workshop()
    {
        R("Workshop heavy worktop",new Vector3(8,-2.44f,-5.8f),new Vector3(2.6f,.08f,1),pbr.Walnut);
        foreach(float x in new[]{6.85f,9.15f}) foreach(float z in new[]{-6.2f,-5.4f}) Rod("Workbench steel leg",new Vector3(x,-3.4f,z),new Vector3(x,-2.48f,z),.045f,pbr.Metal);
        R("Workshop lower shelf",new Vector3(8,-3.15f,-5.8f),new Vector3(2.4f,.065f,.85f),pbr.Walnut);
        R("Workshop perforated tool board",new Vector3(8,-1.7f,-6.7f),new Vector3(2.6f,1,.1f),pbr.Metal,.012f);
        for(int row=0;row<5;row++) for(int col=0;col<13;col++) R("Pegboard perforation",new Vector3(6.85f+col*.19f,-2.05f+row*.18f,-6.641f),new Vector3(.025f,.025f,.009f),pbr.Rubber,.008f);
        for(int i=0;i<5;i++) {
            float x=7.05f+i*.44f;
            Rod("Hanging tool handle",new Vector3(x,-1.95f,-6.60f),new Vector3(x,-1.49f,-6.60f),.024f,i%2==0?pbr.Walnut:pbr.Rubber);
            Rod("Hanging tool head",new Vector3(x-.09f,-1.5f,-6.60f),new Vector3(x+.09f,-1.5f,-6.60f),.038f,pbr.Metal);
        }
        R("Workbench vice body",new Vector3(8.85f,-2.27f,-5.63f),new Vector3(.38f,.25f,.29f),pbr.Metal,.03f);
        Rod("Vice screw handle",new Vector3(8.85f,-2.26f,-5.44f),new Vector3(8.85f,-2.26f,-5.32f),.018f,pbr.Brass);
    }
    void Utility()
    {
        L("Utility hot water cylinder",new Vector3(3.5f,-3.4f,-5.8f),new[]{new Vector2(0,0),new Vector2(.36f,0),new Vector2(.46f,.10f),new Vector2(.46f,1.78f),new Vector2(.35f,1.94f),new Vector2(0,1.98f)},porcelain,28);
        foreach(float y in new[]{-3.17f,-1.63f}) L("Water tank metal band",new Vector3(3.5f,y,-5.8f),new[]{new Vector2(.461f,0),new Vector2(.465f,0),new Vector2(.465f,.035f),new Vector2(.461f,.035f)},pbr.Metal,28);
        R("Water heater thermostat",new Vector3(3.5f,-2.05f,-5.327f),new Vector3(.2f,.25f,.045f),pbr.Metal,.025f);
        Rod("Water heater brass outlet",new Vector3(3.5f,-1.43f,-5.8f),new Vector3(3.5f,-1.23f,-5.8f),.035f,pbr.Brass);
    }
    void Safe()
    {
        R("Safe heavy steel cabinet",new Vector3(0,-2.7f,6.2f),new Vector3(1.3f,1.4f,.8f),pbr.Metal,.035f);
        R("Safe inset door",new Vector3(0,-2.7f,5.785f),new Vector3(1.12f,1.22f,.035f),pbr.Hair,.018f);
        foreach(float x in new[]{-.48f,.48f}) foreach(float y in new[]{-3.2f,-2.2f}) R("Safe door bolt",new Vector3(x,y,5.752f),new Vector3(.04f,.04f,.025f),pbr.Brass,.01f);
        var wheel=L("Safe combination wheel",new Vector3(.15f,-2.7f,5.72f),new[]{new Vector2(.14f,0),new Vector2(.17f,0),new Vector2(.17f,.03f),new Vector2(.14f,.03f),new Vector2(.14f,0)},pbr.Brass,28);wheel.transform.localRotation=Quaternion.Euler(90,0,0);
        Rod("Safe wheel horizontal spoke",new Vector3(-.01f,-2.7f,5.72f),new Vector3(.31f,-2.7f,5.72f),.012f,pbr.Brass);
        Rod("Safe wheel vertical spoke",new Vector3(.15f,-2.86f,5.72f),new Vector3(.15f,-2.54f,5.72f),.012f,pbr.Brass);
        R("Safe keypad",new Vector3(-.30f,-2.55f,5.743f),new Vector3(.18f,.24f,.025f),pbr.Metal,.012f);
        for(int row=0;row<3;row++) for(int col=0;col<3;col++) R("Safe keypad button",new Vector3(-.35f+col*.05f,-2.62f+row*.06f,5.724f),new Vector3(.025f,.025f,.012f),pbr.Linen,.005f);
    }
    public void SetBedroomMask(int mask){bedroomMask=mask&3;RefreshBedroomLamps();}
    void RefreshBedroomLamps()
    {
        for(int i=0;i<2;i++)if(bedroomLamps[i]!=null)bedroomLamps[i].SetColor("_EmissionColor",powered&&(bedroomMask&(1<<i))!=0?new Color(1,.67f,.33f)*1.5f:Color.black);
    }
    public void SetPower(bool on)
    {
        if(powered==on||lamp==null)return;powered=on;RefreshBedroomLamps();
        lamp.SetColor("_EmissionColor",on?new Color(1,.67f,.33f)*1.5f:Color.black);
    }
}
