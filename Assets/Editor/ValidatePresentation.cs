using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ValidatePresentation
{
    static ValidatePresentation(){EditorApplication.delayCall+=ApplySettings;}
    [MenuItem("Game Over/Apply Presentation Settings")]
    public static void ApplySettings()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(PlayerSettings.colorSpace==ColorSpace.Linear)return;
        PlayerSettings.colorSpace=ColorSpace.Linear;
        Debug.Log("Presentation settings applied: Color Space = Linear.");
    }
    [MenuItem("Game Over/Check Presentation Assets")]
    public static void CheckFromMenu(){Check();}
    public static void Batch(){EditorApplication.Exit(Check()==0?0:1);}
    static int Check()
    {
        int failed=0;
        foreach(string name in new[]{"MansionPBR","CoatRim","ThrillerBlur","ThrillerComposite","ArchitecturalGlass","NightSky","ScannedSurface","StormRain","StormBolt"}) {
            var shader=Resources.Load<Shader>("Presentation/"+name);
            if(shader==null) {Debug.LogError("ART FAIL: missing shader "+name);failed++;continue;}
            foreach(var message in ShaderUtil.GetShaderMessages(shader))
                if(message.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error) {Debug.LogError("ART FAIL: "+name+" "+message.message);failed++;}
        }
        foreach(string surface in new[]{"Parquet","Walnut","Plaster","Fabric","Marble","Skin","Leather","BrushedMetal","Porcelain"})
            foreach(string map in new[]{"Color","Normal","Roughness"}) {
                string path="Assets/Resources/Presentation/Textures/"+surface+"_"+map+".png";
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                bool srgb=map=="Color";
                if(texture==null||importer==null||importer.sRGBTexture!=srgb||!importer.mipmapEnabled) {Debug.LogError("ART FAIL: texture/import settings "+path);failed++;}
            }
        if(PlayerSettings.colorSpace!=ColorSpace.Linear) {Debug.LogError("ART FAIL: set Player > Other Settings > Color Space to Linear");failed++;}
        foreach(string id in new[]{"sofa_03","modern_arm_chair_01","modern_coffee_table_01","potted_plant_01","ceramic_vase_03","alarm_clock_01","modern_wooden_cabinet","dining_table","dining_chair_02","tree_small_02","tree_small_02_lod1","tree_small_02_lod2"})
            if(Resources.Load<TextAsset>("Art/Scans/"+id+"/Model")==null||Resources.Load<TextAsset>("Art/Scans/"+id+"/Materials")==null){Debug.LogError("ART FAIL: missing converted mesh/material data "+id);failed++;}
        foreach(string surface in new[]{"Sandstone","Oak"})foreach(string map in new[]{"Color","Normal","Roughness"}) {
            string path="Assets/Resources/Estate/Textures/"+surface+"_"+map+".jpg";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(AssetDatabase.LoadAssetAtPath<Texture2D>(path)==null||importer==null||importer.sRGBTexture!=(map=="Color")||!importer.mipmapEnabled){Debug.LogError("ART FAIL: estate surface "+path);failed++;}
        }
        if(Resources.Load<TextAsset>("Estate/EstateDesign")==null){Debug.LogError("ART FAIL: estate design missing");failed++;}
        if(Resources.Load<Texture2D>("Presentation/Textures/Travertine_Color")==null){Debug.LogError("ART FAIL: missing travertine albedo");failed++;}
        foreach(string name in ProceduralSoundDesign.Names) {
            var clip=Resources.Load<AudioClip>("Presentation/Audio/"+name);
            if(clip==null||clip.samples<=0){Debug.LogError("ART FAIL: missing or empty audio clip "+name);failed++;}
        }
        Debug.Log("PRESENTATION_ASSETS_COMPLETE failures="+failed+". Render and device performance still require Play-mode validation.");
        return failed;
    }
}
