using System.Collections.Generic;

// Shared layout data: runtime builds these boxes; the clearance check reads the same data.
public static class FirstFloorLayout
{
    public sealed class Part
    {
        public string name, material;
        public float x,y,z,w,h,d;
        public Part(string n,float px,float py,float pz,float width,float height,float depth,string m)
        { name=n;x=px;y=py;z=pz;w=width;h=height;d=depth;material=m; }
    }
    public static List<Part> Create()
    {
        var p=new List<Part>();
        Add(p,"West bedroom floor",-5.7f,3.3f,0,8.6f,.2f,14,"floor");
        Add(p,"East bedroom floor",5.7f,3.3f,0,8.6f,.2f,14,"floor");
        Add(p,"Front connecting hall",0,3.3f,-3.25f,2.8f,.2f,7.5f,"floor");
        Add(p,"Upper hall stair guard",0,3.95f,.45f,2.8f,1.1f,.15f,"wood");
        foreach(float x in new[]{-1.3f,1.3f}) {
            Add(p,"Bedroom hall partition south",x,5,-5.5f,.2f,3.2f,3,"wall");
            Add(p,"Bedroom hall partition north",x,5,-.75f,.2f,3.2f,2.5f,"wall");
            Add(p,"Bedroom doorway lintel",x,6.2f,-3,.2f,.8f,2,"wall");
        }
        NorthWindow(p,"Library",-10,-2.4f,-7);
        NorthWindow(p,"Studio",2.4f,10,4.5f);
        Add(p,"West bedroom solid south wall",-5.7f,5,-7,8.6f,3.2f,.2f,"wall");
        Add(p,"East bedroom solid south wall",5.7f,5,-7,8.6f,3.2f,.2f,"wall");
        Add(p,"East exterior south return",10,5,-4.85f,.2f,3.2f,4.3f,"wall");
        Add(p,"East exterior north return",10,5,3.35f,.2f,3.2f,7.3f,"wall");
        Add(p,"East bedroom side window sill wall",10,4.125f,-1.5f,.2f,1.45f,2.4f,"wall");
        Add(p,"East bedroom side window header wall",10,6.4f,-1.5f,.2f,.4f,2.4f,"wall");
        Add(p,"East bedroom side window glass collision",10,5.525f,-1.5f,.025f,1.35f,2.4f,"screen");
        Add(p,"West exterior north",-10,5,4.2f,.2f,3.2f,5.6f,"wall");
        Add(p,"West exterior south",-10,5,-4.2f,.2f,3.2f,5.6f,"wall");
        Add(p,"West balcony doorway lintel",-10,6.2f,0,.2f,.8f,2.8f,"wall");
        Add(p,"West bedroom roof",-5.7f,6.7f,0,8.6f,.2f,14,"wall");
        Add(p,"East bedroom roof",5.7f,6.7f,0,8.6f,.2f,14,"wall");
        Add(p,"Front hall roof",0,6.7f,-3.25f,2.8f,.2f,7.5f,"wall");
        Add(p,"Front balcony lintel",0,6.2f,-7,2.8f,.8f,.2f,"wall");
        Add(p,"West balcony roof",-11,6.7f,0,2.4f,.2f,8.4f,"wall");
        Add(p,"Front balcony roof",0,6.7f,-8,3.2f,.2f,2.4f,"wall");
        Add(p,"West balcony roof fascia",-12.16f,6.57f,0,.08f,.16f,8.4f,"wood");
        Add(p,"Front balcony roof fascia",0,6.57f,-9.16f,3.2f,.16f,.08f,"wood");
        Add(p,"West balcony floor",-11,3.3f,0,2,.2f,8,"floor");
        Add(p,"West balcony railing",-12,3.95f,0,.15f,1.1f,8,"wood");
        Add(p,"West balcony north railing",-11,3.95f,4,2,1.1f,.15f,"wood");
        Add(p,"West balcony south railing",-11,3.95f,-4,2,1.1f,.15f,"wood");
        Add(p,"Front balcony floor",0,3.3f,-8,2.8f,.2f,2,"floor");
        Add(p,"Front balcony railing",0,3.95f,-9,2.8f,1.1f,.15f,"wood");
        Add(p,"Front balcony west railing",-1.4f,3.95f,-8,.15f,1.1f,2,"wood");
        Add(p,"Front balcony east railing",1.4f,3.95f,-8,.15f,1.1f,2,"wood");
        foreach(float x in new[]{-6f,5f}) {
            int bedStart=p.Count;
            // Hollow beneath the frame so under-bed hiding has a real low viewpoint.
            Add(p,"Bed frame deck",x,3.83f,-1.5f,2.6f,.14f,3.7f,"wood");
            foreach(float dx in new[]{-1.12f,1.12f})foreach(float dz in new[]{-1.65f,1.65f})
                Add(p,"Bed frame leg",x+dx,3.58f,-1.5f+dz,.16f,.36f,.16f,"wood");
            Add(p,"Mattress",x,4,-1.5f,2.5f,.3f,3.6f,"linen");
            Add(p,"Bed headboard",x,4.3f,.35f,2.8f,1.3f,.15f,"wood");
            Add(p,"Bed blanket",x,4.17f,-2,2.5f,.06f,2.5f,"fabric");
            foreach(float offset in new[]{-.65f,.65f}) Add(p,"Pillow",x+offset,4.23f,-.25f,1,.18f,.6f,"linen");
            Add(p,"Bedside cabinet",x+1.9f,3.8f,.1f,.7f,.8f,.7f,"wood");
            for(int i=bedStart;i<p.Count;i++){p[i].x=2*x-p[i].x;p[i].z=-6.38f-p[i].z;if(x<0&&p[i].name=="Bedside cabinet")p[i].x+=3.8f;}
            if(x>0)Add(p,"Bedroom dresser",9.55f,3.745f,-1.5f,.53f,.69f,2.4f,"wood");
            else Add(p,"Bedroom dresser",-9.55f,3.745f,-3.25f,.53f,.69f,2.4f,"wood");
        }
        Bath(p,-4.2f,-1.4f,2.5f,5.5f,"Washroom 1");
        Bath(p,7.4f,9.9f,2.5f,6.8f,"Washroom 2");
        // Two real rooms with 1.6 m doorways, included in collision and navigation.
        SuiteRoom(p,"Library",-9.9f,-4.3f,-7,2.1f);
        SuiteRoom(p,"Tech studio",1.4f,7.3f,4.4f,2.1f);
        // Close the offset joins to each bathroom. These are wall returns, not doorways.
        Add(p,"Library bathroom junction wall",-4.3f,5,2.3f,.16f,3.2f,.56f,"wall");
        Add(p,"Studio bathroom junction wall",7.3f,5,2.3f,.16f,3.2f,.56f,"wall");
        // Existing stairwell walls enclose the sides; keep the rear landing
        // passages open so both suites remain reachable from the stair head.
        Add(p,"Library bookcase collision",-7.1f,4.55f,6.5f,4.5f,2.3f,.65f,"wood");
        Add(p,"Library reading seat collision",-8.7f,3.95f,4.1f,1,1.1f,1,"fabric");
        Add(p,"Studio desk collision",4.6f,3.82f,6.2f,2.8f,.84f,1.05f,"wood");
        Add(p,"Studio chair collision",4.6f,3.95f,5.1f,.65f,1.1f,.65f,"fabric");
        return p;
    }
    static void SuiteRoom(List<Part> p,string name,float left,float right,float door,float z)
    {
        float westEnd=door-.8f,eastStart=door+.8f;
        Add(p,name+" west partition",(left+westEnd)/2,5,z,westEnd-left,3.2f,.16f,"wall");
        Add(p,name+" east partition",(eastStart+right)/2,5,z,right-eastStart,3.2f,.16f,"wall");
        Add(p,name+" doorway lintel",door,6.25f,z,1.6f,.7f,.16f,"wall");
    }
    static void NorthWindow(List<Part> p,string name,float left,float right,float centre)
    {
        float a=centre-1.1f,b=centre+1.1f;
        Add(p,name+" north west wall",(left+a)/2,5,6.9f,a-left,3.2f,.2f,"wall");
        Add(p,name+" north east wall",(b+right)/2,5,6.9f,right-b,3.2f,.2f,"wall");
        float bottom=name=="Library"?5.8f:4.36f,top=name=="Library"?6.35f:5.96f;
        Add(p,name+" window sill wall",centre,(3.4f+bottom)/2,6.9f,2.2f,bottom-3.4f,.2f,"wall");
        Add(p,name+" window header wall",centre,(top+6.6f)/2,6.9f,2.2f,6.6f-top,.2f,"wall");
        Add(p,name+" window glass collision",centre,(bottom+top)/2,6.9f,2.2f,top-bottom,.025f,"screen");
    }
    static void SouthWindow(List<Part> p,string name,float left,float right,float centre)
    {
        float a=centre-1.2f,b=centre+1.2f;
        Add(p,name+" south west wall",(left+a)/2,5,-7,a-left,3.2f,.2f,"wall");
        Add(p,name+" south east wall",(b+right)/2,5,-7,right-b,3.2f,.2f,"wall");
        Add(p,name+" front window sill wall",centre,4.125f,-7,2.4f,1.45f,.2f,"wall");
        Add(p,name+" front window header wall",centre,6.4f,-7,2.4f,.4f,.2f,"wall");
        Add(p,name+" front window glass collision",centre,5.525f,-7,2.4f,1.35f,.025f,"screen");
    }
    static void Bath(List<Part> p,float left,float right,float south,float north,string name)
    {
        float mid=(left+right)/2;
        Add(p,name+" south partition",mid,5,south,right-left,3.2f,.15f,"wall");
        Add(p,name+" north partition",mid,5,north,right-left,3.2f,.15f,"wall");
        // Door faces the bedroom on the west side, with a 1.4 m opening.
        Add(p,name+" doorway south",left,5,south+.5f,.15f,3.2f,1,"wall");
        float remain=north-south-2.4f;
        if(remain>0) Add(p,name+" doorway north",left,5,north-remain/2,.15f,3.2f,remain,"wall");
        Add(p,name+" doorway lintel",left,6.2f,south+1.7f,.15f,.8f,1.4f,"wall");
        Add(p,name+" basin",mid,3.9f,north-.5f,.9f,1,.55f,"linen");
        Add(p,name+" toilet",right-.55f,3.7f,south+(right>9?1.95f:.8f),.65f,.6f,.8f,"linen");
    }
    static void Add(List<Part> parts,string n,float x,float y,float z,float w,float h,float d,string m)
    { parts.Add(new Part(n,x,y,z,w,h,d,m)); }
}
