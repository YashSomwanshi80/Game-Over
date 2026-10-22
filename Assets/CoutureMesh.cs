using UnityEngine;
using System.Collections.Generic;

// Small, welded cloth meshes retain curved silhouettes instead of bevelled boxes.
public static class CoutureMesh
{
    public static GameObject Cushion(string name,Vector3 at,Vector3 size,float radius,Material material,Transform parent)
    {
        int steps=Application.isMobilePlatform?10:14;
        var vertices=new List<Vector3>();var triangles=new List<int>();var lookup=new Dictionary<Vector3Int,int>();
        Vector3 half=size*.5f;float r=Mathf.Min(radius,Mathf.Min(half.x,Mathf.Min(half.y,half.z))*.92f);
        foreach(var n in new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back}) {
            Vector3 u=Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.up,v=Vector3.Cross(n,u);var indices=new int[(steps+1)*(steps+1)];
            for(int y=0;y<=steps;y++)for(int x=0;x<=steps;x++) {
                Vector3 q=n+u*(x*2f/steps-1)+v*(y*2f/steps-1);q=Vector3.Scale(q,half);
                Vector3 inner=new Vector3(Mathf.Clamp(q.x,-half.x+r,half.x-r),Mathf.Clamp(q.y,-half.y+r,half.y-r),Mathf.Clamp(q.z,-half.z+r,half.z-r));
                Vector3 normal=(q-inner).normalized;q=inner+normal*r;
                float waves=Mathf.Sin(q.x*31+q.z*9)*Mathf.Sin(q.z*23+q.y*12);
                float dimple=Mathf.Exp(-(q.x*q.x+q.z*q.z)*5)*Mathf.Max(0,normal.y)*-.012f;
                q+=normal*(waves*.004f+dimple);
                var key=new Vector3Int(Mathf.RoundToInt(q.x*100000),Mathf.RoundToInt(q.y*100000),Mathf.RoundToInt(q.z*100000));
                int index;if(!lookup.TryGetValue(key,out index)){index=vertices.Count;lookup.Add(key,index);vertices.Add(q);}indices[y*(steps+1)+x]=index;
            }
            for(int y=0;y<steps;y++)for(int x=0;x<steps;x++) {int a=y*(steps+1)+x;triangles.AddRange(new[]{indices[a],indices[a+1],indices[a+steps+2],indices[a],indices[a+steps+2],indices[a+steps+1]});}
        }
        return Attach(name,at,vertices,triangles,material,parent);
    }
    public static GameObject DrapedBlanket(string name,Vector3 at,float width,float length,Material material,Transform parent)
    {
        int nx=Application.isMobilePlatform?24:32,nz=Application.isMobilePlatform?30:40;
        var vertices=new List<Vector3>();var triangles=new List<int>();
        for(int iz=0;iz<=nz;iz++)for(int ix=0;ix<=nx;ix++) {
            float x=(ix/(float)nx-.5f)*width,z=(iz/(float)nz-.5f)*length;
            float ripple=.014f*Mathf.Sin(x*9+z*4)+.008f*Mathf.Sin(x*17-z*11)+.004f*Mathf.Sin(z*25+x*3);
            float side=Mathf.SmoothStep(0,1,Mathf.InverseLerp(width/2-.10f,width/2,Mathf.Abs(x)));
            float foot=Mathf.SmoothStep(0,1,Mathf.InverseLerp(length/2-.06f,length/2,-z));
            vertices.Add(new Vector3(x,ripple-side*.065f-foot*.035f,z));
        }
        int top=vertices.Count;for(int i=0;i<top;i++)vertices.Add(vertices[i]-Vector3.up*.012f);
        for(int iz=0;iz<nz;iz++)for(int ix=0;ix<nx;ix++) {
            int a=iz*(nx+1)+ix,b=a+nx+1;
            triangles.AddRange(new[]{a,b,a+1,a+1,b,b+1,a+top,a+1+top,b+top,a+1+top,b+1+top,b+top});
        }
        var edge=new List<int>();for(int ix=0;ix<=nx;ix++)edge.Add(ix);
        for(int iz=1;iz<=nz;iz++)edge.Add(iz*(nx+1)+nx);
        for(int ix=nx-1;ix>=0;ix--)edge.Add(nz*(nx+1)+ix);
        for(int iz=nz-1;iz>0;iz--)edge.Add(iz*(nx+1));
        for(int i=0;i<edge.Count;i++){int a=edge[i],b=edge[(i+1)%edge.Count];triangles.AddRange(new[]{a,a+top,b,b,a+top,b+top});}
        return Attach(name,at,vertices,triangles,material,parent);
    }
    public static GameObject Curtain(string name,Vector3 top,float width,float height,Material material,Transform parent)
    {
        int nx=24,ny=24;var vertices=new List<Vector3>();var triangles=new List<int>();
        for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++) {
            float u=x/(float)nx,v=y/(float)ny;
            float fold=Mathf.Sin(u*Mathf.PI*10),hem=Mathf.Sin(u*Mathf.PI*10)*.018f*v*v;
            vertices.Add(new Vector3((u-.5f)*width*(.8f+.2f*v),-v*height+hem,fold*(.045f+.018f*v)+Mathf.Sin(v*8+u*9)*.01f*v));
        }
        for(int y=0;y<ny;y++)for(int x=0;x<nx;x++) {int a=y*(nx+1)+x;triangles.AddRange(new[]{a,a+nx+2,a+1,a,a+nx+1,a+nx+2});}
        // Back faces share positions, but have separate normals for opaque fabric.
        int front=vertices.Count;vertices.AddRange(vertices.ToArray());int count=triangles.Count;
        for(int i=0;i<count;i+=3)triangles.AddRange(new[]{triangles[i]+front,triangles[i+2]+front,triangles[i+1]+front});
        return Attach(name,top,vertices,triangles,material,parent);
    }
    static GameObject Attach(string name,Vector3 at,List<Vector3> vertices,List<int> triangles,Material material,Transform parent)
    {
        var mesh=new Mesh{name=name};mesh.SetVertices(vertices);var uv=new List<Vector2>();foreach(var p in vertices)uv.Add(new Vector2(p.x+p.z*.37f,p.y+p.z*.21f));mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
        return VisualMesh.FromMesh(name,mesh,at,material,parent);
    }
}
