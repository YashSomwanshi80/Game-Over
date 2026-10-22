using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

// Small spatial groups retain room culling and avoid giving each light a mansion-sized mesh.
public sealed class StaticVisualBatch : MonoBehaviour
{
    readonly List<Mesh> owned=new List<Mesh>();
    public void Build(IEnumerable<Renderer> renderers)
    {
        var groups=new Dictionary<string,List<Renderer>>();
        foreach(var renderer in renderers) {
            if(renderer.GetComponentInParent<LODGroup>()!=null||renderer.name.StartsWith("Telephone "))continue;
            if(!renderer.enabled||renderer.name=="Security status diode"||renderer.name=="Security breaker lever") continue;
            var filter=renderer.GetComponent<MeshFilter>();if(filter==null||filter.sharedMesh==null) continue;
            Vector3 p=renderer.bounds.center;
            string key=renderer.sharedMaterial.GetInstanceID()+"/"+renderer.gameObject.layer+"/"+renderer.shadowCastingMode+"/"+renderer.receiveShadows+"/"+Mathf.FloorToInt((p.y+.2f)/3.4f)+"/"+Mathf.FloorToInt(p.x/5)+"/"+Mathf.FloorToInt(p.z/5);
            if(!groups.ContainsKey(key)) groups[key]=new List<Renderer>();groups[key].Add(renderer);
        }
        foreach(var group in groups.Values) {
            if(group.Count<2) continue;
            var combines=new CombineInstance[group.Count];
            for(int i=0;i<group.Count;i++) combines[i]=new CombineInstance{mesh=group[i].GetComponent<MeshFilter>().sharedMesh,transform=transform.worldToLocalMatrix*group[i].localToWorldMatrix};
            var mesh=new Mesh{name="Mansion static art batch",indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(combines,true,true);owned.Add(mesh);
            var go=new GameObject(mesh.name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.layer=group[0].gameObject.layer;
            var combined=go.AddComponent<MeshRenderer>();combined.sharedMaterial=group[0].sharedMaterial;
            combined.shadowCastingMode=group[0].shadowCastingMode;combined.receiveShadows=group[0].receiveShadows;
            foreach(var renderer in group) renderer.enabled=false;
        }
    }
    void OnDestroy(){foreach(var mesh in owned) if(mesh!=null){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}}
}
