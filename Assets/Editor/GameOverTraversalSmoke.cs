using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
// Actual engine-frame movement, not CalculatePath-only validation. Never saves the scene.
[InitializeOnLoad]
public static class GameOverTraversalSmoke
{
    const string Key="GameOver.TraversalSmoke";const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static GameOver game;static NavMeshAgent agent;static BedroomDoors doors;static Vector3[] targets;
    static int index,frame=-1,checks,errors;static float segmentStart;static Vector3 previous;
    static GameOverTraversalSmoke(){if(SessionState.GetBool(Key,false)){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}}
    static void Log(string text,string stack,LogType type){if((type==LogType.Exception||type==LogType.Error||type==LogType.Assert)&&!stack.Contains("UnityEditor.Search.SearchDatabase"))errors++;}
    public static void Start(){if(!Application.isBatchMode)throw new Exception("Batch copy only");EditorSceneManager.OpenScene("Assets/Scenes/Prototype.unity");SessionState.SetBool(Key,true);SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();}
    static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,Flags).GetValue(obj);
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;Debug.Log("TRAVERSAL_PASS "+message);}
    static void Tick(){
        if((DateTime.UtcNow-DateTime.Parse(SessionState.GetString(Key+"Start",DateTime.UtcNow.ToString("O")))).TotalSeconds>240){Finish(2);return;}
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        try{
            if(game==null){
                game=UnityEngine.Object.FindFirstObjectByType<GameOver>();if(game==null||game.GetComponent<PlayerFlashlights>()==null){game=null;return;}
                MatchOptions.Mode=MatchMode.SinglePlayer;MatchOptions.Role=CharacterRole.Tapsee;
                game.GetType().GetMethod("BeginGameplay",Flags).Invoke(game,null);game.enabled=false;
                agent=Field<NavMeshAgent>(game,"don");doors=UnityEngine.Object.FindFirstObjectByType<BedroomDoors>();
                var list=new List<Vector3>(Field<Vector3[]>(game,"patrol"));list.Add(MainPowerLayout.Approach);targets=list.ToArray();
                // Faster wall-clock test, with bounded simulation steps and normal capsule speed.
                Time.timeScale=4;Time.maximumDeltaTime=.05f;agent.speed=2.9f;agent.acceleration=12;agent.stoppingDistance=.2f;
                previous=agent.transform.position;segmentStart=Time.time;Check(agent.SetDestination(targets[0]),"First native traversal destination accepted");return;
            }
            if(frame==Time.frameCount)return;frame=Time.frameCount;
            agent.isStopped=false;doors.OpenForAgent(agent);doors.Tick(Time.deltaTime);
            var fractions=doors.Fractions;
            for(int i=0;i<HouseDoorLayout.Doors.Length;i++){
                var d=HouseDoorLayout.Doors[i];var inverse=Quaternion.Inverse(Quaternion.Euler(0,d.yaw,0));var origin=new Vector3(d.x,d.y,d.z);
                Vector3 a=inverse*(previous-origin),b=inverse*(agent.transform.position-origin);float dx=b.x-a.x;
                if(Mathf.Abs(dx)<.00001f)continue;float t=-a.x/dx;if(t<=0||t>1)continue;
                var crossing=Vector3.Lerp(a,b,t);
                if(Mathf.Abs(crossing.z)>d.width/2||Mathf.Abs(crossing.y)>1.1f)continue;
                Check(fractions[i]>.9f,"Agent crosses only an opened door: "+d.name);
            }
            previous=agent.transform.position;
            if(!agent.pathPending&&agent.remainingDistance<.45f&&Vector3.Distance(agent.nextPosition,targets[index])<1.3f){
                Check(true,"Agent physically reached route stop "+index+" "+targets[index]);
                index++;if(index==targets.Length){Check(errors==0,"No game errors during native traversal");Debug.Log("TRAVERSAL_COMPLETE checks="+checks+" stops="+index+" errors="+errors);Finish(0);return;}
                segmentStart=Time.time;Check(agent.SetDestination(targets[index]),"Route destination accepted "+index);
            }
            if(Time.time-segmentStart>60)throw new Exception("Traversal stuck at stop "+index+" target="+targets[index]+" actual="+agent.transform.position+" remaining="+agent.remainingDistance+" path="+agent.pathStatus);
        }catch(Exception e){Debug.LogException(e);Finish(1);}
    }
    static void Finish(int code){Time.timeScale=1;SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
