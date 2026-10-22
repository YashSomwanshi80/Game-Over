using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class BuildPrototype
{
    [MenuItem("Game Over/Create Prototype Scene")]
    public static void CreateScene()
    {
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Prototype.unity")!=null) {
            Debug.Log("Existing Prototype scene retained. Open Assets/Scenes/Prototype.unity to continue editing the mansion.");return;
        }
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Game Over Bootstrap").AddComponent<GameOver>();
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/Prototype.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Prototype.unity",true)};
        PlayerSettings.companyName="Game Over Student Project"; PlayerSettings.productName="Game Over";
        PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=800; PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
        PlayerSettings.colorSpace=ColorSpace.Linear;
        AssetDatabase.SaveAssets();
    }
    [MenuItem("Game Over/Build Windows Prototype")]
    public static void Build()
    {
        // A build must not replace the user's saved scene or its authored content.
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Prototype.unity")==null)CreateScene();
        var result=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Prototype.unity"},"../Windows/Game Over.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
        if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.Exception("Build failed: "+result.summary.result);
    }
}
