using UnityEngine;
using System.Collections.Generic;

// Meshes are decorative. Existing gameplay colliders and NavMesh sources remain authoritative.
public sealed class VisualMesh : MonoBehaviour
{
    Mesh owned;
    public static GameObject FromMesh(string name,Mesh mesh,Vector3 at,Material material,Transform parent)=>Attach(name,mesh,at,material,parent);
    void OnDestroy(){if(owned!=null){if(Application.isPlaying)Destroy(owned);else DestroyImmediate(owned);}}
    static GameObject Attach(string name,Mesh mesh,Vector3 p,Material material,Transform parent)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=p;
        go.AddComponent<MeshFilter>().sharedMesh=mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial=material;
        go.AddComponent<VisualMesh>().owned=mesh;return go;
    }
    public static GameObject Rounded(string name,Vector3 p,Vector3 size,float radius,Material material,Transform parent)
    {
        Vector3 half=size*.5f;float r=Mathf.Min(radius,Mathf.Min(half.x,Mathf.Min(half.y,half.z))*.95f);
        var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
        Vector3[] axes={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
        foreach(var normal in axes) {
            Vector3 u=Mathf.Abs(normal.y)>.5f?Vector3.right:Vector3.up;
            Vector3 v=Vector3.Cross(normal,u);int first=vertices.Count;
            float hu=Vector3.Dot(half,new Vector3(Mathf.Abs(u.x),Mathf.Abs(u.y),Mathf.Abs(u.z)));
            float hv=Vector3.Dot(half,new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z)));
            float hn=Vector3.Dot(half,new Vector3(Mathf.Abs(normal.x),Mathf.Abs(normal.y),Mathf.Abs(normal.z)));
            float[] us={-hu,-hu+r,hu-r,hu};float[] vs={-hv,-hv+r,hv-r,hv};
            for(int y=0;y<4;y++) for(int x=0;x<4;x++) {
                Vector3 point=normal*hn+u*us[x]+v*vs[y];
                Vector3 inner=new Vector3(Mathf.Clamp(point.x,-half.x+r,half.x-r),Mathf.Clamp(point.y,-half.y+r,half.y-r),Mathf.Clamp(point.z,-half.z+r,half.z-r));
                Vector3 n=(point-inner).normalized;vertices.Add(inner+n*r);normals.Add(n);uv.Add(new Vector2(us[x],vs[y]));
            }
            for(int y=0;y<3;y++) for(int x=0;x<3;x++) {int a=first+y*4+x;triangles.AddRange(new[]{a,a+1,a+5,a,a+5,a+4});}
        }
        var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateTangents();mesh.RecalculateBounds();
        return Attach(name,mesh,p,material,parent);
    }
    // Lathed profiles make hollow glasses, shaped porcelain, tailored body segments and coat panels.
    public static GameObject Lathe(string name,Vector3 p,Vector2[] profile,Material material,Transform parent,int segments=24,float depth=1)
    {
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();
        for(int y=0;y<profile.Length;y++) for(int s=0;s<=segments;s++) {
            float a=s*Mathf.PI*2/segments;
            vertices.Add(new Vector3(Mathf.Sin(a)*profile[y].x,profile[y].y,Mathf.Cos(a)*profile[y].x*depth));uv.Add(new Vector2((float)s/segments,profile[y].y));
        }
        for(int y=0;y<profile.Length-1;y++) for(int s=0;s<segments;s++) {int a=y*(segments+1)+s,b=a+segments+1;tri.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
        var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return Attach(name,mesh,p,material,parent);
    }
    public static Transform Joint(string name,Vector3 position,Transform parent)
    {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
}
