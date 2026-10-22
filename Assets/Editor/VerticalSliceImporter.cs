using UnityEditor;
using UnityEngine;

public sealed class VerticalSliceImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith("Assets/Resources/Environment/VerticalSlice/")||!assetPath.EndsWith("_AO.png"))return;
        var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;
        importer.sRGBTexture=false;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Clamp;
        importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.CompressedHQ;
    }
    void OnPreprocessModel()
    {
        if(!assetPath.StartsWith("Assets/Resources/Environment/VerticalSlice/"))return;
        var importer=(ModelImporter)assetImporter;
        importer.globalScale=1;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
        importer.materialImportMode=ModelImporterMaterialImportMode.None;
        importer.isReadable=false;importer.generateSecondaryUV=true;
        importer.meshCompression=ModelImporterMeshCompression.Off;
    }
    // Archived asset validator; intentionally excluded from the game menus.
    public static void Check()
    {
        var model=Resources.Load<GameObject>(LivingRoomSlice.AssetPath);
        if(model==null){Debug.LogError("SLICE FAIL: authored living-room FBX missing or not imported.");return;}
        int high=0,low=0;long triangles=0;
        foreach(var filter in model.GetComponentsInChildren<MeshFilter>()) {
            var mesh=filter.sharedMesh;if(mesh==null)continue;
            if(filter.name.EndsWith("LOD0"))high++;if(filter.name.EndsWith("LOD1"))low++;
            for(int i=0;i<mesh.subMeshCount;i++)triangles+=(long)mesh.GetIndexCount(i)/3;
        }
        if(high==0||low==0){Debug.LogError("SLICE FAIL: both authored mesh LODs are required.");return;}
        var map=Resources.Load<Texture2D>("Environment/VerticalSlice/LivingRoomFloor_AO");
        if(map==null){Debug.LogError("SLICE FAIL: floor contact map is missing.");return;}
        var aoImporter=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(map))as TextureImporter;
        if(aoImporter==null||aoImporter.sRGBTexture||!aoImporter.mipmapEnabled){Debug.LogError("SLICE FAIL: floor AO must import as linear data with mipmaps.");return;}
        Debug.Log("SLICE ASSET PASS: "+high+" main and "+low+" reduced meshes; "+triangles+" triangles across both levels. Lighting and playability still need Play-mode review.");
    }
}
