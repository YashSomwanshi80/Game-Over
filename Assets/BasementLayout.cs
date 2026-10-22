using System.Collections.Generic;
public static class BasementLayout
{
    public static List<FirstFloorLayout.Part> Create()
    {
        var p=new List<FirstFloorLayout.Part>();
        Add(p,"Basement slab",0,-3.6f,0,20,.4f,14,"floor");
        Add(p,"Basement north wall",0,-1.9f,7,20,3,.2f,"wall");
        Add(p,"Basement south wall",0,-1.9f,-7,20,3,.2f,"wall");
        Add(p,"Basement west wall",-10,-1.9f,0,.2f,3,14,"wall");
        Add(p,"Basement east wall",10,-1.9f,0,.2f,3,14,"wall");
        // Compact return stair in the former ground-floor washroom: north, turn, south.
        // Ground-floor walls already enclose these sides above y=0.
        // Stop the basement walls at the slab to avoid coplanar overlapping faces.
        Add(p,"Downstairs west side",1.35f,-1.7f,4.85f,.1f,3.4f,3.7f,"wall");
        Add(p,"Downstairs east side",3.85f,-1.7f,4.85f,.1f,3.4f,3.7f,"wall");
        Add(p,"Downstairs north side",2.6f,-1.15f,6.7f,2.6f,4.5f,.1f,"wall");
        Add(p,"Stair central divider",2.6f,-1.15f,4.35f,.1f,4.5f,2.7f,"wall");
        Add(p,"Return flight entrance guard",3.2f,.55f,3,1.2f,1.1f,.1f,"wood");
        Add(p,"Basement stair turn landing",2.6f,-1.9f,6.15f,2.4f,.2f,.9f,"floor");
        for(int i=0;i<9;i++) {
            float top=-(i+1)*.2f, height=3.4f+top;
            Add(p,"Basement first flight "+i,2,top-height/2,3.15f+i*.3f,1.1f,height,.3f,"wood");
        }
        for(int i=0;i<8;i++) {
            float top=-1.8f-(i+1)*.2f, height=System.Math.Max(.1f,3.4f+top);
            Add(p,"Basement return flight "+i,3.2f,top-height/2,5.55f-i*.3f,1.1f,height,.3f,"wood");
        }
        // West rooms open into the long game-room corridor through two doorways.
        Add(p,"Office east partition north",-5,-1.9f,5.7f,.15f,3,2.6f,"wall");
        Add(p,"Office east partition south",-5,-1.9f,1.3f,.15f,3,2.6f,"wall");
        Add(p,"Gym east partition north",-5,-1.9f,-1.3f,.15f,3,2.6f,"wall");
        Add(p,"Gym east partition south",-5,-1.9f,-5.7f,.15f,3,2.6f,"wall");
        // Second office exit: an open 1.6 m archway into the gym, which has its own corridor door.
        Partition(p,"Office gym partition doorway",-7.5f,0,5);
        // East-side corridor at z=0 links the workshop, storage and laundry.
        Add(p,"East room divider south",5,-1.9f,-4.1f,.15f,3,5.8f,"wall");
        Add(p,"East room divider north",5,-1.9f,4.1f,.15f,3,5.8f,"wall");
        Partition(p,"Storage doorway",3.2f,2,3.6f);
        Partition(p,"Laundry doorway",7.5f,2,5);
        Add(p,"Utility west enclosing wall",1.4f,-1.9f,-4.5f,.15f,3,5,"wall");
        Partition(p,"Utility doorway",3.2f,-2,3.6f);
        Add(p,"Safe room west",-1.4f,-1.9f,4.5f,.15f,3,5,"wall");
        Add(p,"Safe room east",1.4f,-1.9f,4.5f,.15f,3,5,"wall");
        Partition(p,"Safe room doorway",0,2,2.8f);
        Add(p,"Office desk",-8,-2.95f,5.8f,2.6f,.9f,1,"wood");
        Add(p,"Gaming monitor",-8,-2.1f,6,1.4f,.8f,.12f,"screen");
        Add(p,"Office chair",-8,-2.95f,4.7f,.65f,.9f,.65f,"fabric");
        Add(p,"Pool table",-3.2f,-2.95f,2.7f,1.4f,.9f,2.6f,"wood");
        Add(p,"Pool cloth",-3.2f,-2.48f,2.7f,1.3f,.04f,2.5f,"fabric");
        Add(p,"Gym bench",-8,-3,-3,1.1f,.8f,2.2f,"fabric");
        Add(p,"Weight rack",-9.2f,-2.6f,-5.5f,.8f,1.6f,1.8f,"screen");
        Add(p,"Storage shelving",4.5f,-2.4f,5,.6f,2,2.4f,"wood");
        Add(p,"Storage cartons",4.5f,-2.95f,6.5f,.6f,.9f,.6f,"linen");
        Add(p,"Washing machine",8.7f,-2.9f,5.9f,1,1,1,"linen");
        Add(p,"Dryer",7.4f,-2.9f,5.9f,1,1,1,"linen");
        Add(p,"Laundry sink",6,-2.9f,5.9f,.8f,1,.8f,"linen");
        Add(p,"Workshop bench",8,-2.9f,-5.8f,2.6f,1,1,"wood");
        Add(p,"Workshop tool board",8,-1.7f,-6.7f,2.6f,1,.1f,"wood");
        Add(p,"Utility heater",3.5f,-2.4f,-5.8f,1,2,1,"linen");
        Add(p,"Safe room cabinet",0,-2.7f,6.2f,1.3f,1.4f,.8f,"screen");
        return p;
    }
    static void Partition(List<FirstFloorLayout.Part> p,string n,float x,float z,float width)
    {
        float opening=n=="Utility doorway"?1.05f:1.6f;
        float span=(width-opening)/2;
        Add(p,n+" left",x-(opening/2+span/2),-1.9f,z,span,3,.15f,"wall");
        Add(p,n+" right",x+(opening/2+span/2),-1.9f,z,span,3,.15f,"wall");
        Add(p,n+" lintel",x,-.7f,z,opening,.6f,.15f,"wall");
    }
    static void Add(List<FirstFloorLayout.Part> p,string n,float x,float y,float z,float w,float h,float d,string m)
    {p.Add(new FirstFloorLayout.Part(n,x,y,z,w,h,d,m));}
}
