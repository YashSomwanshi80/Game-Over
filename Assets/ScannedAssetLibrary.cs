using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

// Offline-converted CC0 glTF meshes; no network, importer package or runtime parsing of glTF.
public sealed class ScannedAssetLibrary : MonoBehaviour
{
    [Serializable] sealed class Surface {public string color="",normal="",arm="",ao="";public float metal=1,rough=1,cutoff=.01f;public Color tint=Color.white;}
    [Serializable] sealed class Surfaces {public Surface[] materials=new Surface[0];}
    sealed class Asset {public Mesh[] meshes;public Material[] materials;public int[] slots;public Bounds bounds;}
    readonly Dictionary<string,Asset> cache=new Dictionary<string,Asset>();
    Asset Load(string id)
    {
        Asset result;if(cache.TryGetValue(id,out result))return result;
        var data=Resources.Load<TextAsset>("Art/Scans/"+id+"/Model");var info=Resources.Load<TextAsset>("Art/Scans/"+id+"/Materials");
        if(data==null||info==null)return null;
        result=new Asset();var surfaces=JsonUtility.FromJson<Surfaces>(info.text);
        result.materials=new Material[surfaces.materials.Length];var shader=Resources.Load<Shader>("Presentation/ScannedSurface");
        if(shader==null)return null;
        for(int i=0;i<result.materials.Length;i++) {
            var s=surfaces.materials[i];var material=new Material(shader){name=id+" / scanned finish "+i,color=s.tint,enableInstancing=true};
            material.SetTexture("_MainTex",Resources.Load<Texture2D>(s.color));material.SetTexture("_Normal",Resources.Load<Texture2D>(s.normal));material.SetTexture("_ARM",Resources.Load<Texture2D>(s.arm));
            var ao=string.IsNullOrEmpty(s.ao)?null:Resources.Load<Texture2D>(s.ao);
            material.SetTexture("_AO",ao!=null?ao:Texture2D.whiteTexture);material.SetFloat("_AOStrength",ao!=null?1:0);
            material.SetFloat("_Metallic",s.metal);material.SetFloat("_RoughFactor",s.rough);material.SetFloat("_Cutoff",s.cutoff);result.materials[i]=material;
        }
        using(var reader=new BinaryReader(new MemoryStream(data.bytes))) {
            if(reader.ReadInt32()!=1)throw new InvalidDataException("Unknown scanned mesh version");int parts=reader.ReadInt32();
            result.meshes=new Mesh[parts];result.slots=new int[parts];
            for(int p=0;p<parts;p++) {
                result.slots[p]=reader.ReadInt32();int count=reader.ReadInt32(),indices=reader.ReadInt32();
                var positions=new Vector3[count];var normals=new Vector3[count];var uv=new Vector2[count];var triangles=new int[indices];
                for(int v=0;v<count;v++)positions[v]=new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
                for(int v=0;v<count;v++)normals[v]=new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
                for(int v=0;v<count;v++)uv[v]=new Vector2(reader.ReadSingle(),reader.ReadSingle());
                for(int t=0;t<indices;t++)triangles[t]=reader.ReadInt32();
                var mesh=new Mesh{name=id+" / part "+p,indexFormat=count>65535?UnityEngine.Rendering.IndexFormat.UInt32:UnityEngine.Rendering.IndexFormat.UInt16};
                mesh.vertices=positions;mesh.normals=normals;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateTangents();mesh.RecalculateBounds();result.meshes[p]=mesh;
                if(p==0)result.bounds=mesh.bounds;else result.bounds.Encapsulate(mesh.bounds);
                if(Application.isPlaying)mesh.UploadMeshData(true);
            }
        }
        cache.Add(id,result);return result;
    }
    public bool Place(string id,Vector3 floor,Vector3 limits,float yaw=0,int layer=0)
    {
        var asset=Load(id);if(asset==null){Debug.LogWarning("Scanned asset missing: "+id);return false;}
        var anchor=new GameObject(id+" / fitted mansion asset");anchor.transform.SetParent(transform,false);anchor.layer=layer;anchor.transform.localPosition=floor;anchor.transform.localRotation=Quaternion.Euler(0,yaw,0);
        float scale=Mathf.Min(limits.x/asset.bounds.size.x,Mathf.Min(limits.y/asset.bounds.size.y,limits.z/asset.bounds.size.z));
        var model=new GameObject("Shared scanned mesh");model.transform.SetParent(anchor.transform,false);model.layer=layer;model.transform.localScale=Vector3.one*scale;
        model.transform.localPosition=-new Vector3(id=="tree_small_02"?0:asset.bounds.center.x,asset.bounds.min.y,id=="tree_small_02"?0:asset.bounds.center.z)*scale;
        var renderers=Parts(asset,model.transform,layer);
        var levels=new List<LOD>();
        if(id=="tree_small_02") {
            levels.Add(new LOD(.35f,renderers));
            for(int level=1;level<=2;level++) {
                var lower=Load(id+"_lod"+level);
                if(lower!=null)levels.Add(new LOD(level==1?.12f:.018f,Parts(lower,model.transform,layer)));
            }
        } else levels.Add(new LOD(.025f,renderers));
        var lod=anchor.AddComponent<LODGroup>();lod.SetLODs(levels.ToArray());lod.RecalculateBounds();return true;
    }
    Renderer[] Parts(Asset asset,Transform parent,int layer)
    {
        var renderers=new Renderer[asset.meshes.Length];
        for(int i=0;i<asset.meshes.Length;i++) {
            var part=new GameObject("Scanned geometry "+i);part.transform.SetParent(parent,false);part.layer=layer;part.AddComponent<MeshFilter>().sharedMesh=asset.meshes[i];
            var r=part.AddComponent<MeshRenderer>();r.sharedMaterial=asset.materials[asset.slots[i]];renderers[i]=r;
        }
        return renderers;
    }
    public Vector3 FittedSize(string id,Vector3 limits)
    {
        var asset=Load(id);if(asset==null)return Vector3.zero;
        float scale=Mathf.Min(limits.x/asset.bounds.size.x,Mathf.Min(limits.y/asset.bounds.size.y,limits.z/asset.bounds.size.z));return asset.bounds.size*scale;
    }
    void OnDestroy(){foreach(var a in cache.Values){foreach(var mesh in a.meshes)if(mesh!=null){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}foreach(var material in a.materials)if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}}}
}
