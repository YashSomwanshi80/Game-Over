using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Compatibility shim for earlier imports. The original mansion owns all room geometry.
// Keep this script GUID so existing scene components can migrate safely.
public sealed class LivingRoomSlice : MonoBehaviour
{
    public const string AssetPath="Environment/VerticalSlice/LivingRoomSlice";
    public const string AuthoredPath="Environment/VerticalSlice/AuthoredLivingRoom";
    public bool Ready => false;
    public bool AuthoredFurniture => false;
    public bool HasRoomShell => false;
    public void Build(PbrLibrary pbr,List<GameObject> collision)
    {
        DisableLegacyArt(gameObject.scene);
        foreach(var part in collision)
            if(part!=null&&part.name=="Ground floor east")part.GetComponent<Renderer>().enabled=true;
    }
    public void BuildNative(PbrLibrary pbr,List<GameObject> collision) => Build(pbr,collision);
    public static int DisableLegacyArt(Scene scene)
    {
        int count=0;
        foreach(var anchor in Object.FindObjectsByType<LivingRoomArtAnchor>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
            if(anchor.gameObject.scene!=scene||!anchor.gameObject.activeSelf)continue;
            anchor.gameObject.SetActive(false);count++;
        }
        return count;
    }
    public static bool ConfigureFloorOcclusion(Material material)
    {
        if(material!=null&&material.HasProperty("_UseLocalAO"))material.SetFloat("_UseLocalAO",0);
        return false;
    }
}
