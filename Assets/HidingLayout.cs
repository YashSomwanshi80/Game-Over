using System.Collections.Generic;

public static class HidingLayout
{
    public sealed class Spot
    {
        public string name,action;public bool bed;public float x,y,z,yaw,eye,px,py,pz,ex,ey,ez,tubLength;
        public bool tub=>tubLength>0;
        public bool LowView=>bed||tub;
        public bool wooden=>name.Contains("cupboard")||name=="Garden room cabinet";
        public Spot(string n,string a,bool isBed,float bx,float by,float bz,float facing,float eyeHeight,float promptX,float promptY,float promptZ,float exitX,float exitY,float exitZ,float bathLength=0)
        {name=n;action=a;bed=isBed;x=bx;y=by;z=bz;yaw=facing;eye=eyeHeight;px=promptX;py=promptY;pz=promptZ;ex=exitX;ey=exitY;ez=exitZ;tubLength=bathLength;}
    }
    public static List<Spot> Spots()=>new List<Spot> {
        new Spot("Kitchen cupboard","HIDE IN CUPBOARD",false,-1.95f,.1f,4.7f,270,1.55f,-2.57f,1.3f,4.7f,-3.25f,.04f,4.7f),
        new Spot("Bedroom 1 bed","HIDE UNDER BED",true,-6,3.415f,-4.88f,90,.18f,-4.66f,3.62f,-4.88f,-4.1f,3.44f,-4.88f),
        new Spot("Bedroom 2 bed","HIDE UNDER BED",true,5,3.415f,-4.88f,270,.18f,3.66f,3.62f,-4.88f,3.1f,3.44f,-4.88f),
        new Spot("Garden room cabinet","HIDE IN CABINET",false,-12.2f,.15f,3.8f,180,1.55f,-12.2f,1.35f,3.22f,-12.2f,.08f,2.7f),
        new Spot("Garage locker","HIDE IN LOCKER",false,11.5f,.1f,6.375f,180,1.55f,11.5f,1.3f,5.755f,11.5f,.04f,5.075f),
        new Spot("Laundry linen trolley","HIDE IN LINEN TROLLEY",true,9,-3.35f,3.75f,180,.38f,9,-2.72f,3.20f,9,-3.36f,2.65f),
        new Spot("Workshop locker","HIDE IN LOCKER",false,9.35f,-3.30f,-.2f,270,1.55f,8.77f,-2.10f,-.2f,8.05f,-3.36f,-.2f),
        new Spot("Bedroom 1 north cupboard","HIDE IN CUPBOARD",false,-2.9f,3.5f,1.925f,180,1.55f,-2.9f,4.7f,1.305f,-2.9f,3.44f,.625f),
        new Spot("Bedroom 1 south cupboard","HIDE IN CUPBOARD",false,-8.8f,3.5f,-6.4f,0,1.55f,-8.8f,4.7f,-5.78f,-8.8f,3.44f,-5.05f),
        new Spot("Bedroom 2 north cupboard","HIDE IN CUPBOARD",false,9.4f,3.5f,.85f,270,1.55f,8.78f,4.7f,.85f,8.1f,3.44f,.85f),
        new Spot("Bedroom 2 south cupboard","HIDE IN CUPBOARD",false,8.75f,3.5f,-6.4f,0,1.55f,8.75f,4.7f,-5.78f,8.75f,3.44f,-5.05f),
        new Spot("Washroom 1 bathtub","HIDE IN BATHTUB",false,-3.25f,3.56f,3.05f,0,.43f,-3.25f,4.3f,3.78f,-3.25f,3.44f,4.1f,1.6f),
        new Spot("Washroom 2 bathtub","HIDE IN BATHTUB",false,8.65f,3.56f,3.05f,0,.43f,8.65f,4.3f,3.78f,8.3f,3.44f,4.1f,1.8f)
    };
    public static List<FirstFloorLayout.Part> Create()
    {
        var parts=new List<FirstFloorLayout.Part>();
        foreach(var spot in Spots()) {
            if(spot.bed)continue;
            if(spot.tub)Bath(parts,spot);
            else Cabinet(parts,spot);
        }
        return parts;
    }
    // Filled navigation exclusions are separate from hollow physical enclosures.
    public static List<FirstFloorLayout.Part> NavigationBlockers()
    {
        var parts=new List<FirstFloorLayout.Part>();
        foreach(var s in Spots()) {
            if(s.bed)continue;
            float floor=s.y-(s.tub?.16f:.1f);
            bool side=s.yaw==90||s.yaw==270;
            parts.Add(new FirstFloorLayout.Part(s.name+" navigation exclusion",s.x,floor+(s.tub?.4f:1.2f),s.z,s.tub?s.tubLength:side?1:2,s.tub?.8f:2.4f,s.tub?1:side?2:1,"blocked"));
        }
        return parts;
    }
    static void Cabinet(List<FirstFloorLayout.Part> p,Spot s)
    {
        string mat=s.wooden?"wood":"metal";
        Add(p,s," back",0,1.2f,.45f,2,2.4f,.1f,mat);
        foreach(float side in new[]{-.95f,.95f})Add(p,s," side",side,1.2f,0,.1f,2.4f,1,mat);
        foreach(float y in new[]{.05f,2.35f})Add(p,s," cap",0,y,0,2,.1f,1,mat);
        float center=s.name.StartsWith("Bedroom")?.525f:.54f,width=s.name.StartsWith("Bedroom")?.95f:.92f;
        foreach(float side in new[]{-center,center})Add(p,s," door",side,1.2f,-.5f,width,2.3f,.1f,mat);
    }
    static void Bath(List<FirstFloorLayout.Part> p,Spot s)
    {
        float floor=s.y-.16f;
        p.Add(new FirstFloorLayout.Part(s.name+" basin floor",s.x,floor+.10f,s.z,s.tubLength-.2f,.12f,.62f,"porcelain"));
        foreach(float side in new[]{-1f,1f}) {
            p.Add(new FirstFloorLayout.Part(s.name+" long rim",s.x,floor+.37f,s.z+side*.36f,s.tubLength-.12f,.58f,.08f,"porcelain"));
            p.Add(new FirstFloorLayout.Part(s.name+" end rim",s.x+side*(s.tubLength/2-.06f),floor+.37f,s.z,.12f,.58f,.64f,"porcelain"));
            p.Add(new FirstFloorLayout.Part(s.name+" curtain end",s.x+side*s.tubLength/2,floor+1.405f,s.z,.14f,1.49f,.90f,"curtain"));
            float width=(s.tubLength-.14f)/2;
            p.Add(new FirstFloorLayout.Part(s.name+" curtain front",s.x+side*(.07f+width/2),floor+1.405f,s.z+.45f,width,1.49f,.14f,"curtain"));
        }
    }
    static void Add(List<FirstFloorLayout.Part> p,Spot s,string suffix,float lx,float ly,float lz,float w,float h,float d,string mat)
    {
        int turn=((int)s.yaw+180)%360/90;
        float x=turn==0?lx:turn==1?lz:turn==2?-lx:-lz;
        float z=turn==0?lz:turn==1?-lx:turn==2?-lz:lx;
        bool side=turn==1||turn==3;
        p.Add(new FirstFloorLayout.Part(s.name+suffix,s.x+x,s.y-.1f+ly,s.z+z,side?d:w,h,side?w:d,mat));
    }
}
