using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Builds a test APK from the existing scene. Never recreates a scene or registers a Hub project.
public static class GameOverAndroidBuild
{
    [Serializable] sealed class Result {public string status,apk,sha256,unityVersion;public long bytes;public int errors,warnings;public double seconds;public bool development=true,deviceTested=false;}
    public static void Build()
    {
        try {
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)throw new InvalidOperationException("Launch Unity with -buildTarget Android before this batch method.");
            string output=Environment.GetEnvironmentVariable("GAMEOVER_ANDROID_OUTPUT");
            if(string.IsNullOrEmpty(output))output=Path.GetFullPath("Builds/Android/GAME-OVER-Android-test.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            const string scene="Assets/Scenes/Prototype.unity";
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(scene)==null)throw new FileNotFoundException("The existing playable scene is missing",scene);
            var target=NamedBuildTarget.Android;
            if(string.IsNullOrEmpty(PlayerSettings.GetApplicationIdentifier(target)))PlayerSettings.SetApplicationIdentifier(target,"com.gameover.survival");
            string package=Environment.GetEnvironmentVariable("GAMEOVER_ANDROID_PACKAGE");
            if(!string.IsNullOrEmpty(package))PlayerSettings.SetApplicationIdentifier(target,package);
            bool development=Environment.GetEnvironmentVariable("GAMEOVER_ANDROID_RELEASE")!="1";
            PlayerSettings.companyName="Game Over";PlayerSettings.productName="Game Over";
            PlayerSettings.bundleVersion="0.3.28";PlayerSettings.Android.bundleVersionCode=Math.Max(58,PlayerSettings.Android.bundleVersionCode);
            PlayerSettings.SetScriptingBackend(target,ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(target,ManagedStrippingLevel.Minimal);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.forceInternetPermission=true;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            PlayerSettings.colorSpace=ColorSpace.Linear;
            EditorUserBuildSettings.androidBuildSubtarget=MobileTextureSubtarget.ASTC;
            EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
            AssetDatabase.SaveAssets();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scene},locationPathName=output,target=BuildTarget.Android,options=(development?BuildOptions.Development:BuildOptions.None)|BuildOptions.CompressWithLz4HC});
            var summary=report.summary;
            var result=new Result{development=development,status=summary.result.ToString(),apk=output,unityVersion=Application.unityVersion,errors=(int)summary.totalErrors,warnings=(int)summary.totalWarnings,seconds=summary.totalTime.TotalSeconds};
            if(File.Exists(output)) {
                result.bytes=new FileInfo(output).Length;
                using(var sha=SHA256.Create())using(var stream=File.OpenRead(output))result.sha256=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
            }
            File.WriteAllText(Path.ChangeExtension(output,".build.json"),JsonUtility.ToJson(result,true));
            Debug.Log("GAMEOVER_ANDROID_BUILD "+JsonUtility.ToJson(result));
            EditorApplication.Exit(summary.result==BuildResult.Succeeded?0:1);
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
