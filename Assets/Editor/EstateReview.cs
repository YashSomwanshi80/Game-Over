#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;

// Opt-in command-line review only. Never changes/saves the user's scene.
[InitializeOnLoad]
public static class EstateReview
{
    const string Key="GameOver.EstateReview";
    static double next;
    static Camera review;
    static int errors,editorInfrastructureErrors;
    static readonly Vector3[] points={new Vector3(19,5.5f,-26),new Vector3(.1f,1.65f,-5.5f),new Vector3(-5.8f,1.65f,-2.7f),new Vector3(-3,5,-2),new Vector3(7.2f,5,-1.2f),new Vector3(3,-1.85f,-3.5f),new Vector3(3,5,3.5f),new Vector3(-3,5,-2),new Vector3(-3,5,-2),new Vector3(11,2.05f,-4.7f),new Vector3(15.1f,1.8f,4.2f)};
    static readonly Vector3[] targets={new Vector3(-1,2.8f,-2),new Vector3(-5.5f,1.3f,-.5f),new Vector3(-8,1.5f,1),new Vector3(-6,4.7f,-5),new Vector3(5,4.7f,-5),new Vector3(3.6f,-2,-5.6f),new Vector3(4.6f,4.7f,6.2f),new Vector3(-6,4.7f,-5),new Vector3(-6,4.7f,-5),new Vector3(13,1.1f,2.3f),new Vector3(11.6f,1.1f,5.6f)};
    static EstateReview(){if(SessionState.GetBool(Key,false)){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}}
    public static void Capture()
    {
        SessionState.SetBool(Key,true);int first;int.TryParse(Environment.GetEnvironmentVariable("GAMEOVER_ESTATE_REVIEW_START"),out first);SessionState.SetInt(Key+"Shot",Mathf.Clamp(first,0,points.Length-1));SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;
        EditorSceneManager.OpenScene("Assets/Scenes/Prototype.unity");EditorApplication.EnterPlaymode();
    }
    static void Log(string text,string stack,LogType type){if(type!=LogType.Error&&type!=LogType.Exception)return;
        if(stack!=null&&stack.Contains("UnityEditor.Search.SearchDatabase")){editorInfrastructureErrors++;return;}errors++;}
    static void Finish(int result)
    {
        SessionState.SetBool(Key,false);EditorApplication.update-=Tick;
        Debug.Log("ESTATE_REVIEW_COMPLETE runtimeErrors="+errors+" editorInfrastructureErrors="+editorInfrastructureErrors+" result="+result);EditorApplication.Exit(result);
    }
    static void Tick()
    {
        if((DateTime.UtcNow-DateTime.Parse(SessionState.GetString(Key+"Start",DateTime.UtcNow.ToString("O")))).TotalSeconds>180){Finish(2);return;}
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(EditorApplication.timeSinceStartup<next)return;
        try {
            var source=Camera.main;if(source==null||UnityEngine.Object.FindFirstObjectByType<BedroomDoors>()==null)return;
            if(review==null) {
                var game=UnityEngine.Object.FindFirstObjectByType<GameOver>();
                typeof(GameOver).GetMethod("BeginGameplay",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(game,null);
                game.enabled=false;
                foreach(var agent in UnityEngine.Object.FindObjectsByType<UnityEngine.AI.NavMeshAgent>(FindObjectsSortMode.None))if(agent.isOnNavMesh)agent.isStopped=true;
                review=new GameObject("Temporary estate review camera").AddComponent<Camera>();review.CopyFrom(source);review.enabled=false;review.cullingMask=~(1<<8);
                review.gameObject.AddComponent<ThrillerRendering>().Configure(review,false);next=EditorApplication.timeSinceStartup+3;return;
            }
            int index=SessionState.GetInt(Key+"Shot",0);
            if(index>=points.Length){Finish(errors==0?0:1);return;}
            if(SessionState.GetInt(Key+"Prepared",-1)!=index) {
                review.transform.position=points[index];review.transform.LookAt(targets[index]);review.fieldOfView=index==0?55:72;
                if(source.transform.parent!=null)source.transform.parent.position=points[index]-Vector3.up*1.55f;
                source.transform.rotation=review.transform.rotation;
                UnityEngine.Object.FindFirstObjectByType<MansionVisuals>().SetPower(index<7||index>=9);
                var torch=UnityEngine.Object.FindFirstObjectByType<PlayerFlashlights>();torch.SetActive(index==8);
                if(index==8&&!torch.On(0))torch.Toggle(0);
                SessionState.SetInt(Key+"Prepared",index);next=EditorApplication.timeSinceStartup+1;return;
            }
            foreach(var l in UnityEngine.Object.FindObjectsByType<MansionLighting>(FindObjectsSortMode.None))l.BeginReflectionCapture(points[index]);
            var target=new RenderTexture(1600,900,24,RenderTextureFormat.ARGBHalf);target.Create();review.targetTexture=target;review.Render();
            var previous=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(1600,900,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1600,900),0,0);pixels.Apply();
            string folder=Environment.GetEnvironmentVariable("GAMEOVER_ESTATE_REVIEW");if(string.IsNullOrEmpty(folder))throw new Exception("Review output path missing");Directory.CreateDirectory(folder);
            File.WriteAllBytes(Path.Combine(folder,"Estate-"+index+".png"),pixels.EncodeToPNG());
            RenderTexture.active=previous;review.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);
            SessionState.SetInt(Key+"Shot",index+1);next=EditorApplication.timeSinceStartup+.2;
        }catch(Exception e){Debug.LogException(e);Finish(1);}
    }
}
#endif
