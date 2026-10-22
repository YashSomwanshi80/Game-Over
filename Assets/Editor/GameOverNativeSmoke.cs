using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;

// Runs on a disposable validation copy. Does not save the playable scene.
[InitializeOnLoad]
public static class GameOverNativeSmoke
{
    const string Key="GameOver.NativeGameplaySmoke";
    static int checks;
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static GameOverNativeSmoke(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
    public static void Start()
    {
        if(!Application.isBatchMode)throw new InvalidOperationException("Use a separate batch validation project.");
        EditorSceneManager.OpenScene("Assets/Scenes/Prototype.unity");
        SessionState.SetBool(Key,true);SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();
    }
    static T Field<T>(object instance,string name)=>(T)instance.GetType().GetField(name,Flags).GetValue(instance);
    static void Invoke(object instance,string method)=>instance.GetType().GetMethod(method,Flags).Invoke(instance,null);
    static void Check(bool ok,string text){if(!ok)throw new Exception(text);checks++;Debug.Log("NATIVE_GAMEPLAY_PASS "+text);}
    static void Place(CharacterController body,FirstPersonMotor motor,Vector3 at)
    {
        body.enabled=false;body.transform.position=at;body.transform.rotation=Quaternion.identity;body.enabled=true;
        motor.ResetVertical();Physics.SyncTransforms();
        for(int i=0;i<10;i++)motor.Tick(Vector2.zero,false,false,false,true,.02f);
    }
    static void Tick()
    {
        if((DateTime.UtcNow-DateTime.Parse(SessionState.GetString(Key+"Start",DateTime.UtcNow.ToString("O")))).TotalSeconds>180){Finish(2);return;}
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        var game=UnityEngine.Object.FindFirstObjectByType<GameOver>();
        if(game==null||Field<Camera>(game,"eye")==null||UnityEngine.Object.FindFirstObjectByType<PlayerFlashlights>()==null)return;
        try {
            if(!Field<bool>(game,"started"))Invoke(game,"BeginGameplay");game.enabled=false;
            var player=Field<CharacterController>(game,"player");var motor=player.GetComponent<FirstPersonMotor>();var eye=Field<Camera>(game,"eye");
            var don=Field<NavMeshAgent>(game,"don");don.enabled=false;don.transform.position=new Vector3(1000,0,1000);
            var observer=GameObject.CreatePrimitive(PrimitiveType.Cube);observer.name="Sightline observer fixture";observer.transform.position=new Vector3(100,50,100);
            var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.name="Sightline wall fixture";blocker.transform.position=new Vector3(100,50,102);
            Physics.SyncTransforms();RaycastHit rayHit;var rayStart=new Vector3(100,50,98);
            Check(Physics.Raycast(rayStart,Vector3.forward,out rayHit,5)&&rayHit.collider==observer.GetComponent<Collider>(),"Sightline fixture reproduces own-body obstruction");
            Check(InteractionRaycast.First(rayStart,Vector3.forward,5,observer.GetComponent<Collider>(),out rayHit)&&rayHit.collider==blocker.GetComponent<Collider>(),"Ignoring own body preserves wall occlusion");
            blocker.GetComponent<Collider>().enabled=false;Physics.SyncTransforms();
            Check(InteractionRaycast.Clear(rayStart,new Vector3(100,50,103),observer.GetComponent<Collider>()),"Clear sightline ignores only the observer body");
            observer.GetComponent<Collider>().enabled=false;UnityEngine.Object.Destroy(observer);UnityEngine.Object.Destroy(blocker);
            Place(player,motor,new Vector3(0,.05f,-5.5f));var start=player.transform.position;
            for(int i=0;i<25;i++)motor.Tick(Vector2.up,false,false,false,true,.02f);
            Check(player.transform.position.z>start.z+.4f,"CharacterController walks on the actual foyer floor");
            Check(Mathf.Abs(player.transform.position.y)<.2f,"Player remains supported above the basement");
            motor.Tick(Vector2.zero,false,false,true,true,.02f);Check(player.height<1.4f,"Native capsule crouches");
            motor.Tick(Vector2.zero,false,false,true,true,.02f);Check(player.height>1.6f,"Native capsule stands with clear headroom");
            Place(player,motor,new Vector3(0,.05f,-5.5f));
            for(int i=0;i<45;i++)motor.Tick(Vector2.down,false,false,false,true,.02f);
            Check(player.transform.position.z> -7,"Closed entrance leaf physically blocks passage");
            var doors=UnityEngine.Object.FindFirstObjectByType<BedroomDoors>();
            Place(player,motor,new Vector3(.9f,.05f,-5.5f));doors.Toggle(4);
            for(int i=0;i<80;i++)doors.Tick(.016f);
            Check(doors.Entrance>.99f,"Main entrance opens without a collider obstruction");
            Place(player,motor,new Vector3(0,.05f,-5.5f));
            for(int i=0;i<60;i++)motor.Tick(Vector2.down,false,false,false,true,.02f);
            Check(player.transform.position.z< -7.4f,"Open entrance permits native passage onto the porch");
            foreach(var spot in HidingLayout.Spots()) {
                Place(player,motor,new Vector3(spot.ex,spot.ey,spot.ez));
                var prompt=new Vector3(spot.px,spot.py,spot.pz);eye.transform.localPosition=Vector3.up*1.55f;
                eye.transform.rotation=Quaternion.LookRotation(prompt-eye.transform.position);
                Invoke(game,"Interact");
                if(!Field<bool>(game,"hidden")||player.enabled) {
                    RaycastHit obstacle;string hit=Physics.Raycast(eye.transform.position,eye.transform.forward,out obstacle,2.5f)?obstacle.collider.name:"none";
                    throw new Exception("Hide entry failed: "+spot.name+"; context="+game.GetType().GetMethod("CurrentContext",Flags).Invoke(game,null)+"; grounded="+motor.Grounded+"; notice="+Field<string>(game,"notice")+"; at="+player.transform.position+"; eye="+eye.transform.position+"; firstHit="+hit);
                }
                Check(true,"Enter hide: "+spot.name);
                Check(Vector3.Distance(player.transform.position,new Vector3(spot.x,spot.y,spot.z))<.02f,"Hide viewpoint anchor: "+spot.name);
                Invoke(game,"Interact");
                Check(!Field<bool>(game,"hidden")&&player.enabled,"Exit hide: "+spot.name);
            }
            Place(player,motor,new Vector3(7,.05f,2.8f));eye.transform.localPosition=Vector3.up*1.55f;
            eye.transform.rotation=Quaternion.LookRotation(new Vector3(7,1.2f,4)-eye.transform.position);
            Check((int)game.GetType().GetMethod("CurrentContext",Flags).Invoke(game,null)==TacticalHud.Phone,"Living-room phone has unobstructed native interaction focus");
            var call=game.GetType().GetMethod("AdvancePoliceCall",Flags);var resolve=game.GetType().GetMethod("AdvanceOutcome",Flags);
            var objective=Field<RivalRules>(game,"objective");
            for(int i=0;i<20;i++)call.Invoke(game,new object[]{true,.1f});
            Check(!objective.Called&&objective.Progress>1.99f&&objective.Progress<2.01f,"Phone requires a sustained hold");
            call.Invoke(game,new object[]{false,.1f});Check(objective.Progress==0,"Releasing the call resets partial progress");
            call.Invoke(game,new object[]{true,.1f});call.Invoke(game,new object[]{true,.1f});
            eye.transform.rotation=Quaternion.LookRotation(Vector3.back);call.Invoke(game,new object[]{true,.1f});
            Check(objective.Progress==0,"Looking away interrupts a held call");
            eye.transform.rotation=Quaternion.LookRotation(new Vector3(7,1.2f,4)-eye.transform.position);
            game.GetType().GetField("paused",Flags).SetValue(game,true);call.Invoke(game,new object[]{true,5f});
            Check(objective.Progress==0,"Paused phone does not advance");
            game.GetType().GetField("paused",Flags).SetValue(game,false);
            call.Invoke(game,new object[]{true,5f});Check(objective.Progress<=.1001f&&!objective.Called,"A long frame cannot instantly complete a call");
            objective.Progress=0;for(int i=0;i<39;i++)call.Invoke(game,new object[]{true,.1f});
            Check(!objective.Called,"Call is incomplete before four seconds");call.Invoke(game,new object[]{true,.1f});
            Check(objective.Called&&objective.Progress==RivalRules.CallSeconds&&objective.Rescue==100,"Four-second call starts the full rescue window");
            game.GetType().GetField("paused",Flags).SetValue(game,true);resolve.Invoke(game,new object[]{false,10f});
            Check(objective.Rescue==100,"Pause freezes the rescue countdown");game.GetType().GetField("paused",Flags).SetValue(game,false);
            objective.Rescue=.05f;resolve.Invoke(game,new object[]{true,.1f});Check(objective.Outcome==2,"Capture wins a simultaneous capture/rescue frame");
            resolve.Invoke(game,new object[]{false,.1f});Check(objective.Outcome==2,"Capture outcome remains terminal");
            objective.Outcome=0;objective.Rescue=.05f;resolve.Invoke(game,new object[]{false,.1f});
            Check(objective.Outcome==1&&objective.Rescue==0,"Surviving the countdown produces rescue");
            resolve.Invoke(game,new object[]{true,.1f});Check(objective.Outcome==1,"Rescue outcome remains terminal");
            Debug.Log("NATIVE_GAMEPLAY_SMOKE_COMPLETE checks="+checks+" result=0. Deterministic system calls; real input, scene retry and LAN remain unverified.");Finish(0);
        }catch(Exception e){Debug.LogException(e);Finish(1);}
    }
    static void Finish(int result){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;EditorApplication.Exit(result);}
}
