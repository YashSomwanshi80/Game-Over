using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

// Generates ordinary prefabs/controllers from the shipped FBX files on first import.
// Generated assets are kept outside Models so they cannot collide with source assets.
[InitializeOnLoad]
public sealed class CharacterAssetPipeline : AssetPostprocessor
{
    const string Root="Assets/Resources/Characters/";
    static bool queued,building,sourceChanged,materialsChanged=true;
    static CharacterAssetPipeline(){Queue();EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredEditMode)Queue();};}
    static void Queue(){if(queued)return;queued=true;EditorApplication.delayCall+=BuildIfReady;}
    static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved,string[] old)
    {
        if(building)return;
        if(imported.Any(p=>p.StartsWith(Root+"Models/")||p.StartsWith(Root+"Animations/")))sourceChanged=true;
        if(imported.Any(p=>p.StartsWith(Root+"Textures/")||p==Root+"SurfaceMap.json"))materialsChanged=true;
        if(sourceChanged||materialsChanged)Queue();
    }
    void OnPreprocessModel()
    {
        if(!assetPath.StartsWith(Root))return;
        var model=(ModelImporter)assetImporter;
        model.animationType=ModelImporterAnimationType.Generic;
        model.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        model.motionNodeName="mixamorig1:Hips";
        model.importCameras=false;model.importLights=false;model.importBlendShapes=false;
        model.importVisibility=false;model.addCollider=false;
        model.meshCompression=ModelImporterMeshCompression.Low;
        model.optimizeMeshPolygons=true;model.optimizeMeshVertices=true;
        model.maxBonesPerVertex=4;model.minBoneWeight=.001f;
        model.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
        model.importAnimation=assetPath.Contains("/Animations/");
    }
    void OnPreprocessAnimation()
    {
        if(!assetPath.StartsWith(Root+"Animations/"))return;
        var importer=(ModelImporter)assetImporter;
        var clips=importer.defaultClipAnimations;
        foreach(var clip in clips){clip.name=Path.GetFileNameWithoutExtension(assetPath);clip.loopTime=true;clip.loopPose=true;clip.lockRootRotation=true;clip.lockRootPositionXZ=false;clip.lockRootHeightY=true;clip.keepOriginalPositionY=true;clip.keepOriginalPositionXZ=false;clip.keepOriginalOrientation=true;}
        importer.clipAnimations=clips;
    }
    void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith(Root+"Textures/"))return;
        var texture=(TextureImporter)assetImporter;
        texture.sRGBTexture=!assetPath.Contains("_Normal")&&!assetPath.Contains("_Roughness");
        texture.textureType=assetPath.Contains("_Normal")?TextureImporterType.NormalMap:TextureImporterType.Default;
        texture.maxTextureSize=assetPath.Contains("_Albedo")||assetPath.Contains("_Hair")?2048:1024;texture.mipmapEnabled=true;
        texture.textureCompression=TextureImporterCompression.Compressed;texture.anisoLevel=4;
        texture.alphaIsTransparency=assetPath.Contains("_Hair")||assetPath.Contains("_Lashes");
    }
    static void BuildIfReady()
    {
        queued=false;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating){Queue();return;}
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Models/Don.fbx")==null)return;
        if(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Models/Tapsee.fbx")==null||!File.Exists(Root+"SurfaceMap.json"))return;
        foreach(string folder in new[]{"","Don/"})foreach(string clip in new[]{"Idle","Walk","Run","CrouchIdle","CrouchWalk"})
            if(!AssetDatabase.LoadAllAssetsAtPath(Root+"Animations/"+folder+clip+".fbx").OfType<AnimationClip>().Any(a=>!a.name.StartsWith("__preview")))return;
        if(sourceChanged||!File.Exists(Root+"Don.prefab")||!File.Exists(Root+"Tapsee.prefab")){Build();sourceChanged=false;materialsChanged=false;}
        else if(materialsChanged){RefreshMaterials();materialsChanged=false;}
    }
    [MenuItem("Game Over/Rebuild Rigged Characters")]
    public static void Build()
    {
        if(building)return;building=true;
        try {
            Directory.CreateDirectory(Root+"Generated");AssetDatabase.Refresh();
            foreach(string name in new[]{"Don","Tapsee"}) {
                var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Models/"+name+".fbx");
                if(source==null){Debug.LogWarning("Character source not ready: "+name);continue;}
                var controller=Controller(name);
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(source);
                try {
                    instance.name=name;instance.transform.localScale=Vector3.one;
                    var animator=instance.GetComponent<Animator>()??instance.AddComponent<Animator>();
                    animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;
                    animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                    var levels=new System.Collections.Generic.List<LOD>();
                    var renderers=instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    for(int i=0;i<3;i++) {
                        var group=renderers.Where(r=>r.name.EndsWith("LOD"+i)).ToArray();
                        if(group.Length==0)continue;
                        foreach(var renderer in group) {
                            renderer.enabled=true;renderer.gameObject.SetActive(true);renderer.gameObject.layer=10;
                            renderer.quality=SkinQuality.Bone4;renderer.updateWhenOffscreen=true;renderer.allowOcclusionWhenDynamic=false;
                            var materials=new Material[renderer.sharedMesh.subMeshCount];
                            var importedMaterials=renderer.sharedMaterials;
                            for(int m=0;m<materials.Length;m++){int slot=m;if(name=="Tapsee"&&m<importedMaterials.Length&&importedMaterials[m]!=null){string[] names={"Skin","Overshirt","Denim","Sneakers","Ponytail","Frame","Eyes","WatchCase","WatchGlass","BaseLayer"};int mapped=Array.IndexOf(names,importedMaterials[m].name);if(mapped<0)throw new InvalidOperationException("Unknown Tapsee surface: "+importedMaterials[m].name);slot=mapped;}materials[m]=MaterialFor(name,slot,source,renderer.name);}
                            renderer.sharedMaterials=materials;
                        }
                        levels.Add(new LOD(i==0?.24f:i==1?.10f:.025f,group));
                    }
                    var lod=instance.GetComponent<LODGroup>()??instance.AddComponent<LODGroup>();
                    lod.SetLODs(levels.ToArray());lod.RecalculateBounds();
                    foreach(var collider in instance.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(collider);
                    PrefabUtility.SaveAsPrefabAsset(instance,Root+name+".prefab");
                } finally {UnityEngine.Object.DestroyImmediate(instance);}
            }
            AssetDatabase.SaveAssets();Debug.Log("RIGGED_CHARACTERS_BUILT: Don / Tapsee. Check deformation, lighting and device frame time in Play mode.");
        } finally {building=false;}
    }
    [MenuItem("Game Over/Refresh Character Materials Only")]
    public static void RefreshMaterials()
    {
        if(building||EditorApplication.isPlayingOrWillChangePlaymode)return;
        building=true;
        try {
            var records=JsonUtility.FromJson<SurfaceMap>(File.ReadAllText(Root+"SurfaceMap.json"));
            foreach(var entry in records.surfaces) {
                string path=Root+"Generated/"+entry.character+"_"+entry.slot+".mat";
                if(AssetDatabase.LoadAssetAtPath<Material>(path)==null)continue;
                MaterialFor(entry.character,entry.slot,null,"");
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Character materials refreshed. Existing models, prefabs and animation controllers retained.");
        } finally {building=false;}
    }
    #pragma warning disable 0649 // Fields are populated by JsonUtility.
    [Serializable] sealed class SurfaceMap {public Surface[] surfaces;}
    [Serializable] sealed class Surface {public string character;public int slot;public string albedo;public string detailNormal;public float[] color;public float smoothness;public float normalStrength=.45f;public bool cutout;}
    #pragma warning restore 0649
    static Material MaterialFor(string character,int slot,GameObject source,string rendererName)
    {
        var records=File.Exists(Root+"SurfaceMap.json")?JsonUtility.FromJson<SurfaceMap>(File.ReadAllText(Root+"SurfaceMap.json")):null;
        var entry=records?.surfaces.FirstOrDefault(s=>s.character==character&&s.slot==slot);
        string path=Root+"Generated/"+character+"_"+slot+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,path);}
        material.shader=entry!=null&&entry.cutout?Shader.Find("GameOver/Character hair"):Shader.Find("Standard");
        string albedo=entry?.albedo??character+"_Albedo.png";
        material.mainTexture=string.IsNullOrEmpty(albedo)?null:AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/"+albedo);
        var normal=string.IsNullOrEmpty(albedo)?null:AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/"+albedo.Replace("_Albedo","_Normal"));
        if(material.HasProperty("_BumpMap"))material.SetTexture("_BumpMap",normal);
        if(normal!=null&&albedo.Contains("_Albedo")){material.EnableKeyword("_NORMALMAP");material.SetFloat("_BumpScale",entry?.normalStrength??.45f);}else material.DisableKeyword("_NORMALMAP");
        material.color=entry?.color!=null?new Color(entry.color[0],entry.color[1],entry.color[2],entry.color.Length>3?entry.color[3]:1):Color.white;
        material.SetFloat("_Glossiness",entry?.smoothness??.35f);if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",0);
        if(character=="Tapsee"&&material.HasProperty("_DetailNormalMap")) {
            var detail=string.IsNullOrEmpty(entry?.detailNormal)?null:AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Textures/"+entry.detailNormal);
            material.SetTexture("_DetailNormalMap",detail);
            if(detail!=null){material.SetFloat("_DetailNormalMapScale",.18f);material.SetTextureScale("_DetailAlbedoMap",Vector2.one*12);material.EnableKeyword("_DETAIL_MULX2");}else material.DisableKeyword("_DETAIL_MULX2");
        }
        if(entry!=null&&entry.cutout){material.SetFloat("_Cutoff",.38f);material.SetOverrideTag("RenderType","TransparentCutout");material.renderQueue=2450;}
        else {material.SetOverrideTag("RenderType","");material.renderQueue=-1;material.SetFloat("_Mode",0);material.DisableKeyword("_ALPHATEST_ON");material.DisableKeyword("_ALPHABLEND_ON");material.DisableKeyword("_ALPHAPREMULTIPLY_ON");}
        EditorUtility.SetDirty(material);return material;
    }
    static AnimatorController Controller(string character)
    {
        string path=Root+"Generated/"+character+"Locomotion.controller";
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if(controller!=null)AssetDatabase.DeleteAsset(path);
        controller=AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("Speed",AnimatorControllerParameterType.Float);controller.AddParameter("Crouched",AnimatorControllerParameterType.Bool);controller.AddParameter("Alert",AnimatorControllerParameterType.Float);
        var machine=controller.layers[0].stateMachine;
        var stand=machine.AddState("Standing locomotion");var crouch=machine.AddState("Crouched locomotion");machine.defaultState=stand;
        stand.motion=Tree(controller,character,"Standing",new[]{"Idle","Walk","Run"},new[]{0f,1.7f,4.3f});
        crouch.motion=Tree(controller,character,"Crouching",new[]{"CrouchIdle","CrouchWalk"},new[]{0f,.9f});
        var down=stand.AddTransition(crouch);down.hasExitTime=false;down.duration=.18f;down.AddCondition(AnimatorConditionMode.If,0,"Crouched");
        var up=crouch.AddTransition(stand);up.hasExitTime=false;up.duration=.18f;up.AddCondition(AnimatorConditionMode.IfNot,0,"Crouched");
        EditorUtility.SetDirty(controller);return controller;
    }
    static BlendTree Tree(AnimatorController controller,string character,string name,string[] clips,float[] thresholds)
    {
        var tree=new BlendTree{name=name,blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(tree,controller);
        for(int i=0;i<clips.Length;i++) {
            string folder=character=="Don"?"Don/":"";
            var clip=AssetDatabase.LoadAllAssetsAtPath(Root+"Animations/"+folder+clips[i]+".fbx").OfType<AnimationClip>().FirstOrDefault(a=>!a.name.StartsWith("__preview"));
            if(clip==null)throw new InvalidOperationException("Missing imported character clip "+clips[i]);
            tree.AddChild(clip,thresholds[i]);
        }
        return tree;
    }
}
