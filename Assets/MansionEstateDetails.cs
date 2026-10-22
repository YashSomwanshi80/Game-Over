using UnityEngine;

// Permanent architecture and furnished suites. Large furniture collision is in
// FirstFloorLayout; fine dressing is batched by MansionEnvironment afterwards.
public sealed class MansionEstateDetails : MonoBehaviour
{
    PbrLibrary p;ScannedAssetLibrary scans;Material ink,ochre,green,stone;bool outside=false;
    readonly Material[] circuitEmitters=new Material[HouseLightingLayout.Circuits.Length];
    public void SetHouseLighting(int mask,bool powered) {
        for(int i=0;i<circuitEmitters.Length;i++)if(circuitEmitters[i]!=null)circuitEmitters[i].SetColor("_EmissionColor",HouseLightingLayout.IsOn(mask,i,powered)?new Color(1,.67f,.33f)*3:Color.black);
    }
    Material CircuitMaterial(Material material,Vector3 at) {
        if(outside||material!=p.WarmEmitter)return material;
        int room=HouseLightingLayout.CircuitAt(at.x,at.y,at.z);if(room<0)return material;
        if(circuitEmitters[room]==null)circuitEmitters[room]=p.Emitter(HouseLightingLayout.Circuits[room].name+" / switched decorative diffuser",new Color(1,.67f,.33f),3);
        return circuitEmitters[room];
    }
    GameObject R(string n,Vector3 at,Vector3 size,Material m,float bevel=.012f)
    {var g=VisualMesh.Rounded(n,at,size,bevel,CircuitMaterial(m,at),transform);g.layer=outside?9:0;return g;}
    GameObject L(string n,Vector3 at,Vector2[] profile,Material m,int segments=16)
    {var g=VisualMesh.Lathe(n,at,profile,CircuitMaterial(m,at),transform,segments);g.layer=outside?9:0;return g;}
    void Rod(string n,Vector3 a,Vector3 b,float r,Material m)
    {var g=L(n,a,new[]{new Vector2(0,0),new Vector2(r,0),new Vector2(r,Vector3.Distance(a,b)),new Vector2(0,Vector3.Distance(a,b))},m,10);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
    public void Build(PbrLibrary library,ScannedAssetLibrary assets)
    {
        p=library;scans=assets;
        ink=p.Tint(p.Fabric,new Color(.09f,.15f,.20f),"Estate / indigo wool");
        ochre=p.Tint(p.Leather,new Color(.31f,.16f,.07f),"Estate / cognac leather");
        green=p.Tint(p.Fabric,new Color(.09f,.17f,.12f),"Estate / garden foliage");
        stone=p.Tint(p.Travertine,new Color(.55f,.53f,.46f),"Estate / exterior limestone");
        Library();Studio();Reception();Dining();Kitchen();UpperSuites();Basement();SuiteWindows();
        // Exterior is authored once by EstateRenovation; avoid overlapping generations.
    }
    void Library()
    {
        R("Library woven area carpet",new Vector3(-7,3.414f,4.25f),new Vector3(3.8f,.018f,2.7f),ink);
        R("Library fitted bookcase back",new Vector3(-7.1f,4.55f,6.77f),new Vector3(4.5f,2.3f,.1f),p.Walnut);
        for(int i=0;i<5;i++)R("Library fluted vertical cabinet stile",new Vector3(-9.35f+i*1.125f,4.55f,6.47f),new Vector3(.065f,2.3f,.62f),p.Walnut);
        for(int row=0;row<5;row++) {
            float y=3.49f+row*.53f;
            R("Library shelf with brass lip",new Vector3(-7.1f,y,6.47f),new Vector3(4.5f,.045f,.62f),p.Walnut);
            R("Library brass shelf edge",new Vector3(-7.1f,y,6.15f),new Vector3(4.48f,.015f,.012f),p.Brass);
            if(row==4)continue;
            for(int bay=0;bay<4;bay++)for(int book=0;book<7;book++) {
                float x=-9.2f+bay*1.125f+book*.128f,h=.25f+((book*3+row+bay)%5)*.034f;
                var mat=(book+row)%3==0?ochre:(book+bay)%3==1?ink:p.Linen;
                R("Library individual bound volume",new Vector3(x,y+.023f+h/2,6.37f),new Vector3(.095f,h,.28f),mat,.004f);
                R("Library volume gilt spine band",new Vector3(x,y+.09f,6.224f),new Vector3(.073f,.009f,.005f),p.Brass,.001f);
            }
        }
        if(!scans.Place("modern_arm_chair_01",new Vector3(-8.7f,3.4f,4.1f),new Vector3(1,1.1f,1),120))
            CoutureMesh.Cushion("Library reading seat",new Vector3(-8.7f,3.95f,4.1f),new Vector3(.95f,1,.9f),.12f,ochre,transform);
        R("Reading side table stone top",new Vector3(-9.05f,4.03f,5.15f),new Vector3(.58f,.05f,.55f),p.Marble);
        Rod("Reading side table pedestal",new Vector3(-9.05f,3.42f,5.15f),new Vector3(-9.05f,4.0f,5.15f),.08f,p.Brass);
        Vase(new Vector3(-9.05f,4.06f,5.15f),.65f);
        Coffers(new Vector3(-7.1f,6.49f,4.5f),new Vector2(5.3f,4.1f));
    }
    void Studio()
    {
        R("Studio desk shaped worktop",new Vector3(4.6f,4.21f,6.2f),new Vector3(2.8f,.06f,1.05f),p.Walnut,.025f);
        foreach(float x in new[]{3.4f,5.8f})R("Studio desk side pedestal",new Vector3(x,3.81f,6.3f),new Vector3(.27f,.8f,.8f),p.Metal);
        for(int i=0;i<2;i++) {
            float x=4.12f+i*.98f;
            R("Studio monitor housing",new Vector3(x,4.68f,6.48f),new Vector3(.94f,.55f,.055f),p.Metal);
            R("Studio switched display",new Vector3(x,4.68f,6.445f),new Vector3(.87f,.48f,.008f),p.ScreenEmitter);
            Rod("Studio monitor articulated stand",new Vector3(x,4.25f,6.55f),new Vector3(x,4.48f,6.52f),.025f,p.Metal);
            for(int line=0;line<5;line++)R("Studio display code line",new Vector3(x-.11f,4.82f-line*.065f,6.438f),new Vector3(.40f+line%2*.16f,.009f,.004f),p.Linen,.001f);
        }
        R("Studio extended desk mat",new Vector3(4.65f,4.25f,5.95f),new Vector3(1.6f,.008f,.43f),ink);
        for(int row=0;row<4;row++)for(int col=0;col<12;col++)R("Studio keyboard key",new Vector3(4.18f+col*.045f,4.27f,5.9f+row*.042f),new Vector3(.038f,.016f,.034f),p.Metal,.004f);
        R("Studio mouse",new Vector3(5.12f,4.28f,5.98f),new Vector3(.07f,.045f,.12f),p.Rubber,.018f);
        R("Studio chair mesh back",new Vector3(4.6f,4.17f,4.85f),new Vector3(.60f,.65f,.075f),ink,.03f);
        CoutureMesh.Cushion("Studio chair seat",new Vector3(4.6f,3.88f,5.1f),new Vector3(.62f,.13f,.6f),.05f,ink,transform);
        Rod("Studio chair piston",new Vector3(4.6f,3.5f,5.1f),new Vector3(4.6f,3.84f,5.1f),.04f,p.Metal);
        for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;Rod("Studio chair five star foot",new Vector3(4.6f,3.48f,5.1f),new Vector3(4.6f+Mathf.Sin(a)*.3f,3.48f,5.1f+Mathf.Cos(a)*.3f),.025f,p.Metal);}
        R("Studio acoustic wall panel",new Vector3(6.1f,5.15f,6.83f),new Vector3(1.2f,1.1f,.06f),ink);
        for(int i=0;i<9;i++)R("Studio timber acoustic slat",new Vector3(5.58f+i*.13f,5.15f,6.78f),new Vector3(.045f,1.07f,.04f),p.Walnut);
        Coffers(new Vector3(4.4f,6.49f,4.5f),new Vector2(5.4f,4.1f));
    }
    void Reception()
    {
        // Wall-hugging accents leave the telephone and main entrance approaches free.
        PanelWall(new Vector3(5.6f,0,-6.84f),7.8f);
        R("Reception marble entrance inset",new Vector3(0,.012f,-5.7f),new Vector3(2.0f,.018f,1.6f),p.Marble);
        for(int i=0;i<4;i++)R("Reception inlaid brass floor line",new Vector3(-.75f+i*.5f,.025f,-5.7f),new Vector3(.012f,.005f,1.45f),p.Brass,.001f);
        Coffers(new Vector3(6,2.96f,-1.3f),new Vector2(7.2f,8.4f));
        Chandelier(new Vector3(6,2.60f,-.6f),.62f);
        // Ground-floor drapery belongs to MansionFinishDetails; avoid a second rail/drape set.
        // Keep the television wall clear; the former tall vase overlapped its screen.
        Frame(new Vector3(3,1.8f,-6.71f),new Vector2(1.3f,1),ink);
        Frame(new Vector3(8.7f,1.8f,-6.71f),new Vector2(1.3f,1),ochre);
    }
    void Dining()
    {
        for(int seat=0;seat<6;seat++) {
            float x=-4+(seat%2==0?-.43f:.43f),z=-.70f+(seat/2)*.70f;
            L("Dining porcelain dinner plate",new Vector3(x,.785f,z),new[]{new Vector2(0,0),new Vector2(.13f,0),new Vector2(.19f,.015f),new Vector2(.19f,.025f),new Vector2(.12f,.015f),new Vector2(0,.015f)},p.Porcelain,20);
            R("Dining folded linen napkin",new Vector3(x,.825f,z),new Vector3(.16f,.025f,.22f),p.Linen);
            Rod("Dining cutlery",new Vector3(x-.25f,.797f,z-.1f),new Vector3(x-.25f,.797f,z+.12f),.007f,p.Metal);
            L("Dining water tumbler",new Vector3(x+.12f,.79f,z+.27f),new[]{new Vector2(.045f,0),new Vector2(.05f,.14f),new Vector2(.044f,.14f),new Vector2(.039f,.012f)},p.Glass,16);
        }
        Vase(new Vector3(-4,.80f,0),.9f);
        Chandelier(new Vector3(-4,2.6f,0),.55f);
    }
    void Kitchen()
    {
        for(int i=0;i<5;i++) {
            float z=-.95f+i*.95f;
            R("Kitchen glazed upper cabinet",new Vector3(-9.56f,2.16f,(z)+2.5f),new Vector3(.5f,.9f,.91f),p.Walnut);
            R("Kitchen fluted cabinet face",new Vector3(-9.29f,2.16f,(z)+2.5f),new Vector3(.025f,.80f,.81f),ink);
            Rod("Kitchen upper cabinet brass handle",new Vector3(-9.26f,2.0f,(z+.28f)+2.5f),new Vector3(-9.26f,2.25f,(z+.28f)+2.5f),.009f,p.Brass);
            R("Kitchen marble backsplash",new Vector3(-9.85f,1.39f,(z)+2.5f),new Vector3(.025f,.55f,.94f),p.Marble);
        }
        for(int i=0;i<3;i++)L("Kitchen ceramic storage jar",new Vector3(-8.8f,1.04f,(2.5f+i*.28f)+2.5f),new[]{new Vector2(0,0),new Vector2(.10f,0),new Vector2(.10f,.22f),new Vector2(.07f,.24f),new Vector2(0,.24f)},p.Porcelain);
        R("Kitchen timber chopping board",new Vector3(-8.6f,1.025f,(-.9f)+2.5f),new Vector3(.55f,.035f,.36f),p.Walnut);
        Rod("Kitchen towel bar",new Vector3(-7.99f,.78f,(-.8f)+2.5f),new Vector3(-7.99f,.78f,(-.2f)+2.5f),.014f,p.Metal);
        R("Kitchen folded hanging towel",new Vector3(-7.97f,.59f,(-.5f)+2.5f),new Vector3(.025f,.4f,.32f),p.Linen);
    }
    void UpperSuites()
    {
        foreach(float x in new[]{-6f,5f}) {
            PanelWall(new Vector3(x,3.4f,-6.82f),5.8f);
            R("Bedroom layered bedside rug",new Vector3(x,3.415f,-4.88f),new Vector3(4.7f,.02f,4.8f),ink);
            Coffers(new Vector3(x,6.49f,-2.4f),new Vector2(5.4f,7.8f));
        }
        // HidingLayout and HidingDecor now build real hollow suite cupboards.
    }
    void Basement()
    {
        for(int i=0;i<6;i++) {
            R("Wine cellar wall mounted cradle",new Vector3(-5.72f,-2.2f+i*.19f,-5.9f),new Vector3(.35f,.025f,.9f),p.Walnut);
            for(int j=0;j<3;j++) {
                var bottle=L("Cellar individual green glass bottle",new Vector3(-5.7f,-2.12f+i*.19f,-6.2f+j*.28f),new[]{new Vector2(0,0),new Vector2(.053f,0),new Vector2(.053f,.22f),new Vector2(.022f,.27f),new Vector2(.022f,.34f),new Vector2(0,.34f)},green,12);
                bottle.transform.localRotation=Quaternion.Euler(0,0,-90);
            }
        }
        for(int i=0;i<4;i++)Rod("Basement ceiling service pipe",new Vector3(-9,-.58f,-6+i*.18f),new Vector3(9,-.58f,-6+i*.18f),.035f,p.Metal);
        Frame(new Vector3(-8,-1.5f,-6.78f),new Vector2(1.6f,.9f),ink);
    }
    void Exterior()
    {
        // Forecourt and portico remain beyond the locked entrance. Layer 9 receives exterior moonlight.
        foreach(float x in new[]{-1.75f,1.75f}) {
            R("Portico limestone pier",new Vector3(x,1.45f,-8.05f),new Vector3(.32f,2.9f,.45f),stone);
            R("Portico pier foot",new Vector3(x,.14f,-8.05f),new Vector3(.48f,.28f,.62f),p.Marble);
        }
        R("Portico floating stone canopy",new Vector3(0,3.02f,-8.05f),new Vector3(4.2f,.18f,2.15f),stone);
        R("Portico timber soffit",new Vector3(0,2.91f,-8.05f),new Vector3(3.6f,.035f,1.9f),p.Walnut);
        for(int i=0;i<10;i++)R("Entrance timber soffit slat",new Vector3(-1.6f+i*.35f,2.88f,-8.05f),new Vector3(.05f,.04f,1.85f),p.Walnut);
        foreach(float x in new[]{-9.65f,9.65f}) {
            R("Facade stone corner pilaster",new Vector3(x,3.35f,-7.19f),new Vector3(.55f,6.7f,.16f),stone);
            for(int row=0;row<16;row++)R("Facade masonry horizontal joint",new Vector3(x,.2f+row*.415f,-7.28f),new Vector3(.52f,.012f,.012f),p.Metal,.002f);
        }
        for(int i=0;i<26;i++)R("Upper facade vertical walnut screen",new Vector3(-8.8f+i*.24f,5,-7.19f),new Vector3(.075f,2.8f,.10f),p.Walnut);
        R("Main roof graphite parapet",new Vector3(0,6.95f,-7),new Vector3(20.4f,.34f,.28f),p.Metal);
        R("Garage roof stone coping",new Vector3(13,3.22f,-.0f),new Vector3(6.2f,.18f,14.2f),stone);
        for(int row=0;row<15;row++)for(int col=0;col<5;col++)R("Driveway individual stone paver",new Vector3(11.0f+col*.85f,-.105f,-8-row*.57f),new Vector3(.82f,.05f,.54f),stone);
        for(int i=0;i<7;i++) {
            float x=3.2f+i*.94f;
            R("Front landscape limestone planter",new Vector3(x,.19f,-7.6f),new Vector3(.85f,.38f,.68f),stone);
            for(int shoot=0;shoot<3;shoot++) {
                float z=-7.6f+(shoot-1)*.15f;
                Rod("Landscape grass stem",new Vector3(x,.38f,z),new Vector3(x+(shoot-1)*.13f,.9f+shoot*.1f,z),.018f,green);
                L("Landscape broad leaf",new Vector3(x+(shoot-1)*.1f,.56f,z),new[]{new Vector2(.015f,0),new Vector2(.11f,.2f),new Vector2(.02f,.6f),new Vector2(0,.65f)},green,8);
            }
        }
        R("Courtyard reflecting basin stone rim",new Vector3(-15,.12f,8),new Vector3(5,.26f,3.6f),stone);
        R("Courtyard still reflecting water",new Vector3(-15,.26f,8),new Vector3(4.6f,.025f,3.2f),p.Glass);
        for(int i=0;i<5;i++)R("Courtyard floating stepping slab",new Vector3(-16.7f+i*.85f,.29f,8),new Vector3(.60f,.055f,.7f),p.Marble);
        foreach(float x in new[]{-11f,-19f}) {
            R("Courtyard seating stone base",new Vector3(x,.2f,10.5f),new Vector3(2.4f,.4f,.65f),stone);
            for(int i=0;i<5;i++)R("Courtyard bench timber slat",new Vector3(x,.44f,10.25f+i*.12f),new Vector3(2.45f,.06f,.08f),p.Walnut);
        }
        foreach(float x in new[]{-9.9f,15.9f})Rod("Exterior rainwater downpipe",new Vector3(x,.15f,6.8f),new Vector3(x,x<0?6.65f:3.1f,6.8f),.045f,p.Metal);
    }
    void SuiteWindows()
    {
        var glass=p.ResourceMaterial("Presentation/ArchitecturalGlass","Upper suite night glass");glass.color=new Color(.16f,.25f,.28f,.075f);
        foreach(float x in new[]{-7f,4.5f}) {
            float bottom=x<0?5.8f:4.36f,top=x<0?6.35f:5.96f,mid=(bottom+top)/2;
            R("Suite clear architectural glass",new Vector3(x,mid,6.9f),new Vector3(2.2f,top-bottom,.015f),glass);
            foreach(float side in new[]{-1f,1f})R("Suite window vertical timber jamb",new Vector3(x+side*1.13f,mid,6.76f),new Vector3(.07f,top-bottom+.14f,.12f),p.Walnut);
            foreach(float y in new[]{bottom-.035f,top+.035f})R("Suite window horizontal timber jamb",new Vector3(x,y,6.76f),new Vector3(2.33f,.07f,.12f),p.Walnut);
            R("Suite window centre mullion",new Vector3(x,mid,6.77f),new Vector3(.035f,top-bottom,.05f),p.Metal);
        }
        foreach(var at in new[]{new Vector3(-3.6f,1.65f,-7),new Vector3(6,1.65f,-7),new Vector3(5,5.525f,-7)}) {
            float h=at.y<3?1.5f:1.35f;
            int firstWindow=transform.childCount;
            R("Front elevation clear window",at,new Vector3(2.4f,h,.015f),glass);
            foreach(float side in new[]{-1f,1f}) {
                R("Front elevation timber window jamb",at+new Vector3(side*1.235f,0,.14f),new Vector3(.07f,h+.14f,.15f),p.Walnut);
                R("Front elevation stone window sill",at+new Vector3(0,side*(h/2+.035f),.14f),new Vector3(2.54f,.07f,.20f),p.Travertine);
            }
            R("Front elevation window mullion",at+new Vector3(0,0,.15f),new Vector3(.04f,h,.05f),p.Metal);
            if(at.x==5&&at.y>3) {
                var rotation=Quaternion.Euler(0,-90,0);
                for(int i=firstWindow;i<transform.childCount;i++) {
                    var child=transform.GetChild(i);
                    child.localPosition=new Vector3(10,5.525f,-1.5f)+rotation*(child.localPosition-at);
                    child.localRotation=rotation*child.localRotation;
                }
            }
        }
    }
    void Coffers(Vector3 at,Vector2 size)
    {
        foreach(float side in new[]{-1f,1f}) {
            R("Ceiling coffer perimeter long",at+Vector3.right*side*size.x/2,new Vector3(.09f,.13f,size.y),p.Linen);
            R("Ceiling coffer perimeter cross",at+Vector3.forward*side*size.y/2,new Vector3(size.x,.13f,.09f),p.Linen);
        }
        for(int i=1;i<3;i++)R("Ceiling coffer cross beam",at+Vector3.forward*(-size.y/2+size.y*i/3),new Vector3(size.x,.09f,.065f),p.Linen);
    }
    void Chandelier(Vector3 at,float radius)
    {
        Rod("Chandelier suspension",at,at+Vector3.up*.3f,.012f,p.Brass);
        for(int i=0;i<8;i++) {
            float a=i*Mathf.PI/4;Vector3 b=at+new Vector3(Mathf.Sin(a)*radius,-.13f,Mathf.Cos(a)*radius);
            Rod("Chandelier radial brass arm",at,b,.012f,p.Brass);
            L("Chandelier opal diffuser",b,new[]{new Vector2(0,0),new Vector2(.055f,0),new Vector2(.06f,.16f),new Vector2(0,.17f)},p.WarmEmitter,12);
        }
    }
    void Vase(Vector3 at,float scale)
    {
        if(!scans.Place("ceramic_vase_03",at,new Vector3(.32f,.34f,.32f)*scale))
            L("Estate ceramic vase",at,new[]{new Vector2(0,0),new Vector2(.10f*scale,0),new Vector2(.16f*scale,.18f*scale),new Vector2(.10f*scale,.32f*scale),new Vector2(.07f*scale,.34f*scale),new Vector2(.055f*scale,.33f*scale)},p.Porcelain,20);
        for(int i=0;i<3;i++)Rod("Vase dried botanical stem",at+Vector3.up*.25f*scale,at+new Vector3((i-1)*.15f,.7f+(i%2)*.1f,.04f*i)*scale,.006f,p.Walnut);
    }
    void PanelWall(Vector3 at,float width)
    {
        float h=at.y<1?.6f:1.1f,centreHeight=at.y<1?.4f:.7f;
        for(int i=0;i<4;i++) {
            var centre=at+Vector3.right*(-width/2+(i+.5f)*width/4)+Vector3.up*centreHeight;
            foreach(float side in new[]{-1f,1f}) {
                R("Wall panel vertical moulding",centre+Vector3.right*side*(width/8-.12f),new Vector3(.028f,h,.035f),p.Walnut);
                R("Wall panel horizontal moulding",centre+Vector3.up*side*h/2,new Vector3(width/4-.24f,.028f,.035f),p.Walnut);
            }
        }
    }
    void Frame(Vector3 at,Vector2 size,Material accent)
    {
        R("Gallery framed artwork",at,new Vector3(size.x,size.y,.045f),p.Brass);
        R("Gallery linen mount",at+Vector3.forward*.027f,new Vector3(size.x-.07f,size.y-.07f,.012f),p.Linen);
        R("Gallery abstract colour field",at+new Vector3(-.10f,-.08f,.037f),new Vector3(size.x*.65f,size.y*.48f,.009f),accent);
        R("Gallery abstract horizon line",at+new Vector3(.10f,.12f,.045f),new Vector3(size.x*.62f,.025f,.006f),p.Walnut);
    }
    void Curtains(Vector3 at,float width,float height)
    {
        Rod("Window curtain brass pole",at+new Vector3(-width/2,height,-.08f),at+new Vector3(width/2,height,-.08f),.018f,p.Brass);
        foreach(float side in new[]{-1f,1f})for(int i=0;i<8;i++) {
            float x=side*(width/2-.20f)+i*.035f;
            var curtain=R("Full height pleated linen curtain",at+new Vector3(x,height/2,-.12f+(i%2)*.05f),new Vector3(.045f,height-.14f,.075f),p.Linen,.018f);
            curtain.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
