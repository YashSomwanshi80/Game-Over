using UnityEngine;
using System.Collections.Generic;

// Anatomical fallback geometry; this is not a scanned or facially rigged character.
public static class HumanAnatomy
{
    public static GameObject Head(string name,Vector3 at,Material skin,Transform parent)
    {
        const int rings=28,slices=40;var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
        for(int ring=0;ring<=rings;ring++)for(int slice=0;slice<=slices;slice++) {
            float theta=ring*Mathf.PI/rings,angle=slice*Mathf.PI*2/slices;
            float height=.15f+Mathf.Cos(theta)*.18f,jaw=Mathf.Lerp(.75f,1,Mathf.Clamp01((height+.02f)/.16f));
            float x=Mathf.Sin(angle)*Mathf.Sin(theta)*.126f*jaw,z=Mathf.Cos(angle)*Mathf.Sin(theta)*.105f;
            float front=Mathf.Pow(Mathf.Max(0,Mathf.Cos(angle)),14);
            float nose=Mathf.Exp(-Mathf.Pow((height-.137f)/.032f,2))*.059f;
            float bridge=Mathf.Exp(-Mathf.Pow((height-.19f)/.05f,2))*.018f;
            float lips=Mathf.Exp(-Mathf.Pow((height-.063f)/.014f,2))*.009f;
            z+=front*(nose+bridge+lips);
            float cheek=Mathf.Exp(-Mathf.Pow((Mathf.Abs(x)-.074f)/.022f,2))*Mathf.Exp(-Mathf.Pow((height-.12f)/.04f,2));
            z+=cheek*Mathf.Max(0,Mathf.Cos(angle))*.008f;
            vertices.Add(new Vector3(x,height,z));uv.Add(new Vector2(slice/(float)slices,1-ring/(float)rings));
        }
        for(int r=0;r<rings;r++)for(int s=0;s<slices;s++){int a=r*(slices+1)+s;triangles.AddRange(new[]{a,a+slices+1,a+1,a+1,a+slices+1,a+slices+2});}
        var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();
        var normals=mesh.normals;for(int ring=0;ring<=rings;ring++){int first=ring*(slices+1),last=first+slices;Vector3 normal=(normals[first]+normals[last]).normalized;normals[first]=normals[last]=normal;}
        for(int s=0;s<=slices;s++){normals[s]=Vector3.up;normals[rings*(slices+1)+s]=Vector3.down;}mesh.normals=normals;mesh.RecalculateTangents();mesh.RecalculateBounds();
        return VisualMesh.FromMesh(name,mesh,at,skin,parent);
    }
    public static GameObject Hood(Vector3 at,Material cloth,Transform parent)
    {
        const int rows=18,columns=32;var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
        for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++) {
            float t=y*Mathf.PI/rows,a=(65+x*230f/columns)*Mathf.Deg2Rad;
            vertices.Add(new Vector3(Mathf.Sin(a)*Mathf.Sin(t)*.18f,.15f+Mathf.Cos(t)*.23f,Mathf.Cos(a)*Mathf.Sin(t)*.17f));uv.Add(new Vector2(x/(float)columns,y/(float)rows));
        }
        for(int y=0;y<rows;y++)for(int x=0;x<columns;x++){int a=y*(columns+1)+x;triangles.AddRange(new[]{a,a+columns+1,a+1,a+1,a+columns+1,a+columns+2});}
        int count=vertices.Count,front=triangles.Count;vertices.AddRange(vertices.ToArray());uv.AddRange(uv.ToArray());for(int i=0;i<front;i+=3)triangles.AddRange(new[]{triangles[i]+count,triangles[i+2]+count,triangles[i+1]+count});
        var mesh=new Mesh{name="Open cloth hood shell"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return VisualMesh.FromMesh(mesh.name,mesh,at,cloth,parent);
    }
}
