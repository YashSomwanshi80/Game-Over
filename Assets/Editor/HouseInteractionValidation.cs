using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Batch-only engine validation. Creates a disposable scene and preserves the user's scene.
public static class HouseInteractionValidation
{
    public static void Run()
    {
        if(!Application.isBatchMode)throw new InvalidOperationException("Run this validator in a separate batch project.");
        EditorSceneManager.OpenScene("Assets/Scenes/Prototype.unity",OpenSceneMode.Single);
        int result=1;var previous=SceneManager.GetActiveScene();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try {
            var root=new GameObject("Disposable house systems validation");
            foreach(var row in File.ReadAllLines(Environment.GetEnvironmentVariable("GAMEOVER_GEOMETRY"))) {
                if(row.StartsWith("name,"))continue;var p=row.Split(',');
                var box=GameObject.CreatePrimitive(PrimitiveType.Cube);box.name=p[0];box.transform.SetParent(root.transform);
                float[] a=new float[6];for(int i=0;i<6;i++)a[i]=float.Parse(p[i+1],CultureInfo.InvariantCulture);
                box.transform.position=new Vector3(a[0],a[1],a[2]);box.transform.localScale=new Vector3(a[3],a[4],a[5]);
            }
            var art=new GameObject("Actual runtime fixtures and switches");
            var pbr=art.AddComponent<PbrLibrary>();pbr.Build();
            var camera=new GameObject("Validation camera").AddComponent<Camera>();camera.enabled=false;
            var lighting=art.AddComponent<MansionLighting>();lighting.Build(camera,pbr);
            Physics.SyncTransforms();
            int checkedSwitches=0;
            for(int i=0;i<HouseLightingLayout.Circuits.Length;i++) {
                var c=HouseLightingLayout.Circuits[i];var eye=new Vector3(c.sx+c.nx*.8f,c.floor+1.55f,c.sz+c.nz*.8f);
                var target=lighting.SwitchPrompt(i);camera.transform.position=eye;
                if(lighting.FocusSwitch(eye,(target-eye).normalized)!=i)throw new Exception(c.name+" switch failed actual physics focus");
                lighting.BeginReflectionCapture(new Vector3(c.x,c.floor+1.55f,c.z));
                if(!AnyLit(art,c.name))throw new Exception(c.name+" has no enabled real Light");
                lighting.ToggleSwitch(i);
                if(AnyLit(art,c.name))throw new Exception(c.name+" light stayed on after switch press");
                lighting.ToggleSwitch(i);
                if(!AnyLit(art,c.name))throw new Exception(c.name+" light failed to turn back on");
                checkedSwitches++;
            }
            lighting.SetPower(false);
            foreach(var l in art.GetComponentsInChildren<Light>())if(l.enabled&&l.intensity>0)throw new Exception("Room light bypassed main breaker");
            lighting.SetPower(true);
            var doors=new GameObject("Actual runtime doors").AddComponent<BedroomDoors>();doors.Build(pbr);
            for(int i=0;i<HouseDoorLayout.Doors.Length;i++)doors.Toggle(i);
            for(int step=0;step<90;step++)doors.Tick(.016f);
            var fractions=doors.Fractions;
            for(int i=0;i<fractions.Length;i++)if(fractions[i]<.99f)throw new Exception(HouseDoorLayout.Doors[i].name+" actual door stalled at "+fractions[i]);
            Debug.Log("HOUSE_ENGINE_VALIDATION_PASS switches="+checkedSwitches+" doors="+fractions.Length+" breaker=pass");result=0;
        } catch(Exception e){Debug.LogException(e);}
        finally {SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true);EditorApplication.Exit(result);}
    }
    static bool AnyLit(GameObject root,string name)
    {
        foreach(var l in root.GetComponentsInChildren<Light>())if(l.name==name+" / real room light"&&l.enabled&&l.intensity>0)return true;
        return false;
    }
}
