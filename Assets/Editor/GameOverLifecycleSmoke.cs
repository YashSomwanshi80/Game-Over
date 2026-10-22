using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;

// Scene lifecycle integration checks; run only in a disposable batch project.
[InitializeOnLoad]
public static class GameOverLifecycleSmoke
{
    const string Key="GameOver.LifecycleSmoke";
    const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static int stage,checks,oldId,errors;
    static double since;
    static GameOverLifecycleSmoke(){if(SessionState.GetBool(Key,false)){EditorApplication.update+=Tick;Application.logMessageReceived-=Log;Application.logMessageReceived+=Log;}}
    public static void Start(){
        if(!Application.isBatchMode)throw new InvalidOperationException("Batch validation copy only.");
        EditorSceneManager.OpenScene("Assets/Scenes/Prototype.unity");
        SessionState.SetBool(Key,true);SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));
        Application.logMessageReceived+=Log;EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();
    }
    static void Log(string text,string stack,LogType type){if((type==LogType.Exception||type==LogType.Error||type==LogType.Assert)&&!stack.Contains("UnityEditor.Search.SearchDatabase"))errors++;}
    static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,Flags).GetValue(obj);
    static void Invoke(object obj,string name,params object[] args)=>obj.GetType().GetMethod(name,Flags).Invoke(obj,args);
    static void Check(bool ok,string text){if(!ok)throw new Exception(text);checks++;Debug.Log("LIFECYCLE_PASS "+text);}
    static void Route(GameOver game,FrontEndAction action){oldId=game.GetInstanceID();Invoke(game,"RequestScene",action);Invoke(game,"Update");stage++;since=EditorApplication.timeSinceStartup;}
    static void Fresh(GameOver game){
        var rules=Field<RivalRules>(game,"objective");
        Check(!rules.Called&&rules.Progress==0&&rules.Rescue==100&&rules.Outcome==0,"Reload resets solo objectives");
        var lights=UnityEngine.Object.FindFirstObjectByType<MansionLighting>();Check(lights.HouseMask==HouseLightingLayout.AllOn,"Reload resets room switches");
        var torches=game.GetComponent<PlayerFlashlights>();Check(torches.Charge(0)==1&&torches.Charge(1)==1&&!torches.On(0)&&!torches.On(1),"Reload resets both battery flashlights");
        Check(UnityEngine.Object.FindObjectsByType<GameOver>(FindObjectsSortMode.None).Length==1,"Exactly one scene bootstrap after reload");
    }
    static void DonRole(GameOver game){
        var rival=Field<RivalMatch>(game,"rival");Check(rival!=null&&rival.Role==CharacterRole.Don&&rival.Running,"Solo Don initializes a running rival match");
        var eye=Field<Camera>(game,"eye");var body=Field<CharacterController>(rival,"don");
        Check(eye.transform.parent==body.transform&&body.enabled,"First-person camera follows playable Don");
        var bot=Field<NavMeshAgent>(rival,"bot");Check(bot!=null&&bot.enabled&&bot.isOnNavMesh,"Tapsee bot is placed on native NavMesh");
        Check(body.GetComponents<CharacterController>().Length==1,"Don has one movement capsule");
        var controls=Field<RivalPacket>(rival,"local");controls.x=1;controls.call=controls.sprint=controls.flashToggle=controls.jump=controls.crouch=controls.action=true;
        Invoke(rival,"OnApplicationPause",true);
        Check(!rival.Running&&controls.x==0&&!controls.call&&!controls.sprint&&!controls.flashToggle&&!controls.jump&&!controls.crouch&&!controls.action,"Don suspension pauses offline play and clears queued controls");
        Invoke(rival,"OnApplicationPause",false);Check(!rival.Running,"Don resume requires explicit unpause");
        rival.GetType().GetField("paused",Flags).SetValue(rival,false);
    }
    static void Tick(){
        if((DateTime.UtcNow-DateTime.Parse(SessionState.GetString(Key+"Start",DateTime.UtcNow.ToString("O")))).TotalSeconds>240){Finish(2);return;}
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        var game=UnityEngine.Object.FindFirstObjectByType<GameOver>();
        if(game==null||Field<Camera>(game,"eye")==null||game.GetComponent<PlayerFlashlights>()==null)return;
        if(stage>0&&(game.GetInstanceID()==oldId||EditorApplication.timeSinceStartup-since<.3))return;
        try{
            game.enabled=false;
            if(stage==0){
                Check(!Field<bool>(game,"started"),"Normal launch shows main menu");
                MatchOptions.Mode=MatchMode.SinglePlayer;MatchOptions.Role=CharacterRole.Tapsee;Invoke(game,"BeginGameplay");
                Check(Field<bool>(game,"started")&&Field<RivalMatch>(game,"rival")==null,"Solo Tapsee uses first-person survival controller");
                game.GetType().GetField("sprintHeld",Flags).SetValue(game,true);game.GetType().GetField("jumpPressed",Flags).SetValue(game,true);
                Invoke(game,"OnApplicationPause",true);
                Check(Field<bool>(game,"paused")&&!Field<bool>(game,"sprintHeld")&&!Field<bool>(game,"jumpPressed"),"Tapsee suspension pauses and releases held controls");
                Invoke(game,"OnApplicationPause",false);Check(Field<bool>(game,"paused"),"Tapsee resume requires explicit unpause");
                game.GetType().GetField("paused",Flags).SetValue(game,false);
                var rules=Field<RivalRules>(game,"objective");rules.Progress=3;rules.Called=true;rules.Rescue=24;
                UnityEngine.Object.FindFirstObjectByType<MansionLighting>().ToggleSwitch(0);
                var torches=game.GetComponent<PlayerFlashlights>();torches.Toggle(0);torches.Step(50);
                Route(game,FrontEndAction.Retry);
            }else if(stage==1){
                Check(Field<bool>(game,"started"),"Tapsee retry skips menu and intro");Fresh(game);
                Check(!SessionFlow.ConsumeQuickRetry(),"Quick retry route is consumed exactly once");Route(game,FrontEndAction.MainMenu);
            }else if(stage==2){
                Check(!Field<bool>(game,"started")&&Field<RivalMatch>(game,"rival")==null,"Return to menu clears active solo match");Fresh(game);
                MatchOptions.Role=CharacterRole.Don;Invoke(game,"BeginGameplay");DonRole(game);Route(game,FrontEndAction.Retry);
            }else if(stage==3){
                Check(Field<bool>(game,"started"),"Don retry starts gameplay");DonRole(game);Fresh(game);Route(game,FrontEndAction.MainMenu);
            }else if(stage==4){
                Check(!Field<bool>(game,"started")&&Field<RivalMatch>(game,"rival")==null,"Don return to menu removes rival instance");
                Check(UnityEngine.Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None).Length==1,"Menu reload removes Don runtime capsule");
                Check(errors==0,"No gameplay errors across scene transitions");
                Debug.Log("LIFECYCLE_SMOKE_COMPLETE checks="+checks+" result=0. Direct routing; menu clicks, touch input and real LAN not tested.");Finish(0);
            }
        }catch(Exception e){Debug.LogException(e);Finish(1);}
    }
    static void Finish(int result){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(result);}
}
