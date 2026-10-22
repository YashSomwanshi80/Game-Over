using System.Collections.Generic;
// Door plane is local X=0; a single leaf extends from its jamb along local Z.
public static class HouseDoorLayout
{
    public sealed class Spec {
        public string name;public float x,y,z,yaw,aperture,width,height,side,hinge;public bool sliding,compact;
        public Spec(string n,float px,float py,float pz,float facing,float opening,float w,float h,float swing=-1,float hingeSide=1,bool slide=false,bool telescope=false)
        {name=n;x=px;y=py;z=pz;yaw=facing;aperture=opening;width=w;height=h;side=swing;hinge=hingeSide;sliding=slide;compact=telescope;}
    }
    public static readonly Spec[] Doors={
        new Spec("Bedroom 1",-1.3f,3.4f,-3,0,2,1.1f,2.4f),
        new Spec("Bedroom 2",1.3f,3.4f,-3,0,2,1.1f,2.4f,1),
        new Spec("Library",-7,3.4f,2.1f,90,1.6f,1.1f,2.5f),
        new Spec("Tech studio",4.4f,3.4f,2.1f,90,1.6f,1.1f,2.5f),
        new Spec("Main entrance",0,0,-7,90,2.2f,1.2f,2.5f),
        new Spec("Garden room",-10,.05f,-1,0,2.4f,1.2f,2.4f),
        new Spec("Garden exit",-16,.05f,-2,0,2.2f,1.2f,2.4f),
        new Spec("West balcony",-10.23f,3.4f,0,0,2.8f,2.8f,2.4f,-1,1,true),
        new Spec("Front balcony",0,3.4f,-7.23f,90,2.8f,2.8f,2.4f,-1,1,true,true),
        new Spec("Washroom 1",-4.2f,3.4f,4.2f,0,1.4f,1,2.4f),
        new Spec("Washroom 2",7.4f,3.4f,4.2f,0,1.4f,1,2.4f),
        new Spec("Kitchen store",-7,0,-3.2f,90,1.2f,.95f,2.4f),
        new Spec("Garage",10,0,0,0,2,1.1f,2.4f,1),
        new Spec("Basement office",-5,-3.4f,3.5f,0,1.8f,1.1f,2.4f),
        new Spec("Gym",-5,-3.4f,-3.5f,0,1.8f,1.1f,2.4f),
        new Spec("Storage",3.2f,-3.4f,2,90,1.6f,1.05f,2.4f),
        new Spec("Laundry",7.5f,-3.4f,2,90,1.6f,1.05f,2.4f),
        new Spec("Utility room",3.2f,-3.4f,-2,90,1.05f,1.05f,2.4f),
        new Spec("Safe room",0,-3.4f,2,90,1.6f,1.05f,2.4f),
        new Spec("Workshop",5,-3.4f,0,0,2.4f,1.1f,2.4f,1),
        new Spec("Basement entrance",2,0,3,90,1.2f,1.05f,2.4f,1)
    };
    public sealed class FixedPart {
        public string name;public float x,y,z,w,h,d;
        public FixedPart(string n,float px,float py,float pz,float width,float height,float depth){name=n;x=px;y=py;z=pz;w=width;h=height;d=depth;}
    }
    public static List<FixedPart> FixedParts()
    {
        var result=new List<FixedPart>();
        foreach(var door in Doors) {
            if(door.sliding)continue;
            float ceiling=door.y>3?3.2f:3;
            float strip=(door.aperture-door.width)/2;
            foreach(float side in new[]{-1f,1f})if(strip>.001f) {
                float offset=side*(door.width/2+strip/2);bool alongX=door.yaw==90;
                result.Add(new FixedPart(door.name+" single door side panel",door.x+(alongX?offset:0),door.y+door.height/2,door.z+(alongX?0:offset),alongX?strip:.15f,door.height,alongX?.15f:strip));
            }
            bool missingLintel=door.name=="Basement office"||door.name=="Gym"||door.name=="Workshop";
            if(missingLintel)result.Add(new FixedPart(door.name+" single door header",door.x,door.y+(ceiling+door.height)/2,door.z,door.yaw==90?door.aperture:.15f,ceiling-door.height,door.yaw==90?.15f:door.aperture));
            if(door.name=="Garden room"||door.name=="Garden exit")result.Add(new FixedPart(door.name+" lintel closure",door.x,2.475f,door.z,.15f,.05f,door.aperture));
        }
        result.Add(new FixedPart("Kitchen store doorway return",-6.2f,1.5f,-3.2f,.4f,3,.12f));
        return result;
    }
}
