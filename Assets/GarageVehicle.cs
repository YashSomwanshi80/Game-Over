using UnityEngine;
using System.Collections.Generic;

// Mobile-sized, authored sedan mesh. Decorative surfaces share the existing parked-car blocker.
public sealed class GarageVehicle : MonoBehaviour
{
    Material paint,glass,trim,rubber,lamp,tail;
    Transform root;
    public void Build(PbrLibrary p,Transform parent)
    {
        root=VisualMesh.Joint("Garage / detailed graphite sedan",new Vector3(13.45f,0,.5f),parent);
        paint=p.Tint(p.Metal,new Color(.19f,.26f,.32f),"Sedan / graphite metallic paint");paint.SetTexture("_MainTex",null);paint.SetFloat("_Glossiness",.70f);paint.SetFloat("_Metallic",.62f);paint.SetFloat("_NormalStrength",0);
        glass=p.Tint(p.Glass,new Color(.025f,.045f,.058f),"Sedan / smoked glazing");
        glass.SetTexture("_MainTex",null);glass.SetFloat("_NormalStrength",0);
        trim=p.Tint(p.Metal,new Color(.52f,.57f,.62f),"Sedan / satin alloy");rubber=p.Rubber;
        lamp=p.Tint(p.Glass,new Color(.65f,.76f,.82f),"Sedan / headlamp lens");tail=p.Tint(p.Glass,new Color(.4f,.025f,.02f),"Sedan / red tail lens");
        var concrete=p.Tint(p.Plaster,new Color(.30f,.32f,.33f),"Garage / sealed concrete");concrete.SetFloat("_Glossiness",.16f);
        R("Sealed concrete garage slab",new Vector3(-.45f,.006f,-.5f),new Vector3(5.72f,.012f,13.70f),concrete,.004f);
        foreach(float joint in new[]{-4.5f,-.5f,3.5f})R("Concrete expansion joint",new Vector3(-.45f,.014f,joint),new Vector3(5.7f,.005f,.014f),rubber,.002f);
        Body();Cabin();
        foreach(float side in new[]{-1f,1f}){
            foreach(float z in new[]{-1.43f,1.43f})Wheel(side,z,p);
            R("Lower sill",new Vector3(side*.98f,.30f,0),new Vector3(.10f,.10f,1.95f),rubber,.025f);
            foreach(float z in new[]{-.16f,.9f})R("Flush door handle",new Vector3(side*1.01f,.96f,z),new Vector3(.025f,.035f,.20f),trim,.012f);
            Beam("Door shut line",new Vector3(side*1.015f,.43f,.27f),new Vector3(side*1.015f,.94f,.27f),.009f,rubber);
            R("Wing mirror housing",new Vector3(side*1.075f,1.065f,-.73f),new Vector3(.23f,.13f,.25f),paint,.05f);
            R("Mirror glass",new Vector3(side*1.195f,1.065f,-.70f),new Vector3(.012f,.08f,.16f),glass,.015f);
            R("Swept headlamp",new Vector3(side*.65f,.78f,-2.21f),new Vector3(.47f,.10f,.055f),lamp,.025f);
            foreach(float y in new[]{.765f,.8f})R("Headlamp light guide",new Vector3(side*.65f,y,-2.244f),new Vector3(.40f,.012f,.01f),trim,.004f);
            R("Tail light",new Vector3(side*.64f,.81f,2.24f),new Vector3(.47f,.105f,.035f),tail,.02f);
        }
        R("Front lower intake",new Vector3(0,.51f,-2.30f),new Vector3(1.26f,.17f,.035f),rubber,.04f);
        for(int i=0;i<7;i++)R("Grille fin",new Vector3(-.51f+i*.17f,.51f,-2.325f),new Vector3(.018f,.12f,.012f),trim,.004f);
        foreach(float z in new[]{-2.335f,2.335f}){
            R("Plate recess",new Vector3(0,.69f,z),new Vector3(.44f,.13f,.025f),rubber,.014f);
            R("Blank registration plate",new Vector3(0,.69f,z+Mathf.Sign(z)*.015f),new Vector3(.36f,.085f,.01f),trim,.006f);
        }
        R("Rear valance",new Vector3(0,.39f,2.25f),new Vector3(1.6f,.13f,.09f),rubber,.025f);
        foreach(float side in new[]{-1f,1f})R("Garage parking bay stripe",new Vector3(side*1.42f,.012f,.15f),new Vector3(.045f,.012f,5.7f),p.Linen,.003f);
    }
    GameObject R(string n,Vector3 at,Vector3 size,Material mat,float bevel)=>VisualMesh.Rounded(n,at,size,bevel,mat,root);
    void Beam(string n,Vector3 a,Vector3 b,float width,Material mat){var go=R(n,(a+b)*.5f,new Vector3(width,Vector3.Distance(a,b),width),mat,width*.3f);go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
    void Body()
    {
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();const int rings=73,around=8;
        for(int i=0;i<rings;i++){
            float z=Mathf.Lerp(-2.35f,2.35f,(float)i/(rings-1));float end=Mathf.Pow(Mathf.Abs(z)/2.35f,6);
            float width=1.015f-.20f*end,top=.94f-.19f*end;
            float near=Mathf.Min(Mathf.Abs(z+1.43f),Mathf.Abs(z-1.43f));float bottom=near<.43f?.38f+Mathf.Sqrt(.43f*.43f-near*near):.32f;
            var ring=new[]{new Vector2(-width*.80f,bottom),new Vector2(width*.80f,bottom),new Vector2(width*.98f,Mathf.Max(.47f,bottom)),new Vector2(width,top-.10f),new Vector2(width*.90f,top),new Vector2(-width*.90f,top),new Vector2(-width,top-.10f),new Vector2(-width*.98f,Mathf.Max(.47f,bottom))};
            foreach(var v in ring){vertices.Add(new Vector3(v.x,v.y,z));uv.Add(new Vector2(v.x,z));}
            if(i==0)continue;
            for(int j=0;j<around;j++){int a=(i-1)*around+j,b=(i-1)*around+(j+1)%around,c=i*around+(j+1)%around,d=i*around+j;tri.AddRange(new[]{a,b,c,a,c,d});}
        }
        for(int end=0;end<2;end++){
            int center=vertices.Count;vertices.Add(new Vector3(0,.55f,end==0?-2.35f:2.35f));uv.Add(Vector2.zero);int start=end==0?0:(rings-1)*around;
            for(int j=0;j<around;j++){int a=start+j,b=start+(j+1)%around;tri.AddRange(end==0?new[]{center,b,a}:new[]{center,a,b});}
        }
        Mesh("Continuous body with wheel-arch cutaways",vertices,tri,uv,paint);
    }
    void Cabin()
    {
        var z=new[]{-1.15f,-.42f,.64f,1.38f};var lower=new[]{.82f,.90f,.90f,.84f};var upper=new[]{.81f,.70f,.70f,.78f};var height=new[]{.94f,1.43f,1.45f,.94f};
        var verts=new List<Vector3>();var tris=new List<int>();var uv=new List<Vector2>();
        for(int i=0;i<4;i++){foreach(var v in new[]{new Vector3(-lower[i],.91f,z[i]),new Vector3(lower[i],.91f,z[i]),new Vector3(upper[i],height[i],z[i]),new Vector3(-upper[i],height[i],z[i])}){verts.Add(v);uv.Add(new Vector2(v.x,v.z));}if(i>0)for(int j=0;j<4;j++){int a=(i-1)*4+j,b=(i-1)*4+(j+1)%4,c=i*4+(j+1)%4,d=i*4+j;tris.AddRange(new[]{a,b,c,a,c,d});}}
        Mesh("Raked windshield and side glazing",verts,tris,uv,glass);
        R("Painted roof panel",new Vector3(0,1.47f,.12f),new Vector3(1.44f,.055f,1.12f),paint,.025f);
        foreach(float side in new[]{-1f,1f}){
            Beam("A pillar",new Vector3(side*.83f,.92f,-1.14f),new Vector3(side*.70f,1.45f,-.42f),.045f,paint);
            Beam("B pillar",new Vector3(side*.90f,.93f,.25f),new Vector3(side*.71f,1.47f,.25f),.045f,rubber);
            Beam("C pillar",new Vector3(side*.84f,.92f,1.37f),new Vector3(side*.70f,1.46f,.64f),.065f,paint);
            Beam("Window sill trim",new Vector3(side*.91f,.94f,-1.12f),new Vector3(side*.91f,.94f,1.35f),.018f,trim);
            Beam("Roof edge",new Vector3(side*.71f,1.47f,-.42f),new Vector3(side*.71f,1.49f,.64f),.028f,paint);
        }
        foreach(float x in new[]{-.33f,.33f}){var w=R("Resting windscreen wiper",new Vector3(x,1.01f,-1.04f),new Vector3(.40f,.014f,.018f),rubber,.004f);w.transform.localRotation=Quaternion.Euler(0,0,x<0?-7:7);}
    }
    void Wheel(float side,float z,PbrLibrary p)
    {
        var wheel=VisualMesh.Joint("Alloy wheel assembly",new Vector3(side*.985f,.38f,z),root);wheel.localRotation=Quaternion.Euler(0,0,side>0?-90:90);
        VisualMesh.Lathe("Shaped tyre",Vector3.zero,new[]{new Vector2(0,0),new Vector2(.29f,0),new Vector2(.35f,.025f),new Vector2(.37f,.065f),new Vector2(.37f,.17f),new Vector2(.33f,.23f),new Vector2(0,.23f)},rubber,wheel,32);
        VisualMesh.Lathe("Recessed brake disc",Vector3.zero,new[]{new Vector2(0,.232f),new Vector2(.23f,.232f),new Vector2(.23f,.237f),new Vector2(0,.237f)},p.Metal,wheel,24);
        VisualMesh.Lathe("Alloy rim lip",Vector3.zero,new[]{new Vector2(.23f,.235f),new Vector2(.28f,.235f),new Vector2(.28f,.25f),new Vector2(.23f,.26f),new Vector2(.23f,.235f)},trim,wheel,32);
        for(int i=0;i<5;i++){float a=i*72*Mathf.Deg2Rad;var spoke=VisualMesh.Rounded("Five-spoke alloy",new Vector3(Mathf.Sin(a)*.13f,.25f,Mathf.Cos(a)*.13f),new Vector3(.042f,.025f,.24f),.009f,trim,wheel);spoke.transform.localRotation=Quaternion.Euler(0,i*72+10,0);}
        VisualMesh.Lathe("Wheel center cap",Vector3.zero,new[]{new Vector2(0,.255f),new Vector2(.065f,.255f),new Vector2(.065f,.27f),new Vector2(0,.27f)},trim,wheel,20);
    }
    void Mesh(string name,List<Vector3> v,List<int> t,List<Vector2> uv,Material mat){var mesh=new UnityEngine.Mesh{name=name};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();VisualMesh.FromMesh(name,mesh,Vector3.zero,mat,root);}
}
