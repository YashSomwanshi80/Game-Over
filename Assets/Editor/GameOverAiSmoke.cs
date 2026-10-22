using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class GameOverAiSmoke
{
    const string Key="GameOver.AiSmoke";const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static int checks,failures,errors;
    static GameOverAiSmoke(){if(SessionState.GetBool(Key,false)){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}}
    static void Log(string text,string stack,LogType type){if((type==LogType.Exception||type==LogType.Error||type==LogType.Assert)&&!stack.Contains("UnityEditor.Search.SearchDatabase"))errors++;}
    public static void Start(){if(!Application.isBatchMode)throw new Exception("Batch validation only");EditorSceneManager.OpenScene("Assets/Scenes/Prototype.unity");SessionState.SetBool(Key,true);SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();}
    static T Field<T>(object game,string name)=>(T)game.GetType().GetField(name,Flags).GetValue(game);
    static object Invoke(object game,string name,params object[] args)=>game.GetType().GetMethod(name,Flags).Invoke(game,args);
    static void Check(bool ok,string text){checks++;if(!ok)failures++;Debug.Log("AI_NATIVE_"+(ok?"PASS ":"FAIL ")+text);}
    static void Tick(){
        if((DateTime.UtcNow-DateTime.Parse(SessionState.GetString(Key+"Start",DateTime.UtcNow.ToString("O")))).TotalSeconds>180){Finish(2);return;}
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        var game=UnityEngine.Object.FindFirstObjectByType<GameOver>();if(game==null||game.GetComponent<PlayerFlashlights>()==null)return;
        try{
            MatchOptions.Mode=MatchMode.SinglePlayer;MatchOptions.Role=CharacterRole.Tapsee;Invoke(game,"BeginGameplay");game.enabled=false;
            var don=Field<NavMeshAgent>(game,"don");don.isStopped=true;
            var patrol=Field<Vector3[]>(game,"patrol");var path=new NavMeshPath();Vector3 previous=don.nextPosition;
            for(int i=0;i<patrol.Length;i++){
                NavMeshHit hit;bool sampled=NavMesh.SamplePosition(patrol[i],out hit,.7f,NavMesh.AllAreas)&&Mathf.Abs(hit.position.y-patrol[i].y)<.5f;
                Check(sampled,"Patrol waypoint "+i+" lies on its intended floor: "+patrol[i]);
                bool complete=sampled&&NavMesh.CalculatePath(previous,hit.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
                Check(complete,"Native route reaches waypoint "+i+" from previous patrol stop");if(sampled)previous=hit.position;
            }
            foreach(var level in new[]{DonDifficulty.Easy,DonDifficulty.Normal,DonDifficulty.Hard}) {
            var profile=DonDifficultyRules.For(level);game.GetType().GetField("difficulty",Flags).SetValue(game,profile);
            foreach(var point in new[]{new Vector3(-6,3.4f,3),new Vector3(5,3.4f,3),new Vector3(-7,-3.4f,3.5f),new Vector3(7,-3.4f,-3)}){
                NavMeshHit at;bool sampled=NavMesh.SamplePosition(point,out at,.7f,NavMesh.AllAreas);Check(sampled,"Search anchor on NavMesh: "+point);if(!sampled)continue;
                don.Warp(at.position);game.GetType().GetField("lastSeen",Flags).SetValue(game,at.position);
                for(int i=0;i<8;i++){Invoke(game,"ChooseSearchPoint");Check(don.hasPath&&don.path.status==NavMeshPathStatus.PathComplete&&Mathf.Abs(don.destination.y-at.position.y)<.6f&&Vector3.Distance(don.destination,at.position)<profile.SearchRadius+.8f,level+" search stays reachable on the observed storey at "+point+" attempt "+i);}
            }
            }
            var player=Field<CharacterController>(game,"player");var eye=Field<Camera>(game,"eye");
            player.enabled=false;player.transform.position=new Vector3(0,.05f,-2);player.enabled=true;eye.transform.localPosition=Vector3.up*1.55f;
            don.Warp(new Vector3(0,0,-4));don.transform.rotation=Quaternion.identity;Physics.SyncTransforms();
            Check((bool)Invoke(game,"CanSee"),"Don sees an exposed player in the clear foyer");
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="AI visibility wall fixture";wall.transform.position=new Vector3(0,1,-3);wall.transform.localScale=new Vector3(1.4f,3,.15f);Physics.SyncTransforms();
            Check(!(bool)Invoke(game,"CanSee"),"Opaque wall blocks Don's actual perception");
            wall.GetComponent<Collider>().enabled=false;UnityEngine.Object.Destroy(wall);don.transform.rotation=Quaternion.Euler(0,180,0);Physics.SyncTransforms();
            Check(!(bool)Invoke(game,"CanSee"),"Don cannot see a player behind his field of view");
            var locker=HidingLayout.Spots().Find(s=>s.name=="Garage locker");
            Check(Mathf.Abs(locker.z+.5f-6.875f)<.002f,"Garage cupboard back aligns with north wall");
            NavMeshHit garageFrom,garageTo;
            bool garageRoute=NavMesh.SamplePosition(new Vector3(10.7f,0,0),out garageFrom,.7f,NavMesh.AllAreas)&&NavMesh.SamplePosition(new Vector3(locker.ex,0,locker.ez),out garageTo,.7f,NavMesh.AllAreas)&&NavMesh.CalculatePath(garageFrom.position,garageTo.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
            Check(garageRoute,"Native path from garage entrance to hiding approach clears parked sedan");
            Check(errors==0,"No game-code errors during AI audit");Debug.Log("AI_NATIVE_COMPLETE checks="+checks+" failures="+failures+" errors="+errors);Finish(failures+errors==0?0:1);
        }catch(Exception e){Debug.LogException(e);Finish(1);}
    }
    static void Finish(int code){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
