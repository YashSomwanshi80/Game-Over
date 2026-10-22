using System;
// Pure shared circuit data, used by runtime presentation and offline checks.
public static class HouseLightingLayout
{
    public sealed class Circuit {
        public string name;public float floor,x,z,sx,sz,nx,nz;public bool large;
        public Circuit(string n,float f,float px,float pz,float switchX,float switchZ,float normalX,float normalZ,bool big=false)
        {name=n;floor=f;x=px;z=pz;sx=switchX;sz=switchZ;nx=normalX;nz=normalZ;large=big;}
    }
    public static readonly Circuit[] Circuits={
        new Circuit("Bedroom 1",3.4f,-6,-3.2f,-1.55f,-4.3f,-1,0,true),
        new Circuit("Bedroom 2",3.4f,5,-3.2f,1.55f,-4.3f,1,0,true),
        new Circuit("Kitchen",0,-8.4f,1,-9.77f,-2.4f,1,0,true),
        new Circuit("Dining room",0,-4,1,-2.1f,-6.76f,0,1,true),
        new Circuit("Living room",0,6.7f,.7f,9.77f,1.4f,-1,0,true),
        new Circuit("Entrance hall",0,.1f,-3.3f,1.7f,-6.76f,0,1,true),
        new Circuit("Kitchen store",0,-8,-5,-6.1f,-4.4f,-1,0),
        new Circuit("Garage",0,13,.1f,10.23f,1.4f,1,0,true),
        new Circuit("Garden room",0,-13,-.4f,-10.23f,-2.6f,-1,0,true),
        new Circuit("Library",3.4f,-7,4.5f,-8.1f,2.32f,0,1),
        new Circuit("Tech studio",3.4f,4.5f,4.6f,5.5f,2.32f,0,1),
        new Circuit("Washroom 1",3.4f,-2.8f,4.1f,-4,5.1f,1,0),
        new Circuit("Washroom 2",3.4f,8.6f,4.4f,7.62f,5.2f,1,0),
        new Circuit("Upper landing",3.4f,0,6.1f,0,6.68f,0,-1),
        new Circuit("Basement office",-3.4f,-7.5f,3.5f,-5.23f,4.8f,-1,0),
        new Circuit("Gym",-3.4f,-7.5f,-3.5f,-5.23f,-4.8f,-1,0),
        new Circuit("Games room",-3.4f,-3.2f,.3f,-4.77f,-4.8f,1,0,true),
        new Circuit("Safe room",-3.4f,0,4.3f,-1.18f,3.3f,1,0),
        new Circuit("Storage",-3.4f,4.2f,4.4f,4.4f,2.23f,0,1),
        new Circuit("Utility room",-3.4f,3,-4.4f,4.3f,-2.23f,0,-1),
        new Circuit("Laundry",-3.4f,7.5f,4.4f,8.6f,2.23f,0,1),
        new Circuit("Workshop",-3.4f,7.5f,-3.4f,5.23f,-1.5f,1,0,true),
        new Circuit("Basement passage",-3.4f,2.5f,0,4.3f,1.77f,0,-1),
        new Circuit("West balcony",3.4f,-11,0,-10.27f,2.9f,-1,0),
        new Circuit("Front balcony",3.4f,0,-8,1.2f,-7.24f,0,-1)
    };
    public static int AllOn=>(1<<Circuits.Length)-1;
    public static bool IsOn(int mask,int circuit,bool mains)=>mains&&(circuit<0||(mask&(1<<circuit))!=0);
    public static int CircuitAt(float x,float y,float z)
    {
        if(y>3.4f) {
            if(x< -10&&x>=-12.2f&&Math.Abs(z)<=4.2f)return 23;
            if(Math.Abs(x)<1.6f&&z< -7&&z>=-9.2f)return 24;
            if(x< -10||x>10||z< -7||z>7)return -1;
            if(x>=-4.2f&&x<=-1.4f&&z>=2.5f&&z<=5.5f)return 11;
            if(x>=7.4f&&x<=9.9f&&z>=2.5f)return 12;
            if(x<=-4.3f&&z>=2.1f)return 9;
            if(x>=1.4f&&x<=7.3f&&z>=2.1f)return 10;
            if(x< -1.4f&&z<2.1f)return 0;
            if(x>1.4f&&z<2.1f)return 1;
            return 13;
        }
        if(y<-.1f) {
            if(x< -10||x>10||Math.Abs(z)>7)return -1;
            if(x< -5)return z>=0?14:15;
            if(x< -1.4f)return 16;
            if(x<=1.4f&&z>=2)return 17;
            if(x>=5)return z>=2?20:21;
            if(x>1.4f&&z>=2)return 18;
            if(x>1.4f&&z<=-2)return 19;
            return 22;
        }
        if(x>=-16&&x< -10&&Math.Abs(z)<=5)return 8;
        if(x>=10&&x<=16&&Math.Abs(z)<=7)return 7;
        if(x< -10||x>10||Math.Abs(z)>7)return -1;
        if(x< -6&&z< -3.2f)return 6;
        if(x< -6.4f)return 2;
        if(x< -1.4f)return 3;
        if(x>=3.8f)return 4;
        return 5;
    }
}
