using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class GameOverIntroSmoke
{
 const string Key="GameOver.IntroSmoke";const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 static IntroVideo intro;static VideoPlayer video;static GameOver game;static int stage,checks,errors;static double next,pausedAt;static bool advanced;
 static GameOverIntroSmoke(){if(SessionState.GetBool(Key,false)){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}}
 static void Log(string text,string stack,LogType type){if((type==LogType.Exception||type==LogType.Error||type==LogType.Assert)&&!stack.Contains("UnityEditor.Search.SearchDatabase"))errors++;}
 public static void Start(){if(!Application.isBatchMode)throw new Exception("Batch copy only");EditorSceneManager.OpenScene("Assets/Scenes/Prototype.unity");SessionState.SetBool(Key,true);SessionState.SetString(Key+"Start",DateTime.UtcNow.ToString("O"));EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();}
 static void Check(bool yes,string text){if(!yes)throw new Exception(text);checks++;Debug.Log("INTRO_NATIVE_PASS "+text);}
 static void Call(string method,bool flag)=>typeof(IntroVideo).GetMethod(method,F).Invoke(intro,new object[]{flag});
 static void Tick(){
  if((DateTime.UtcNow-DateTime.Parse(SessionState.GetString(Key+"Start",DateTime.UtcNow.ToString("O")))).TotalSeconds>180){Finish(2);return;}
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  try{
   if(game==null){game=UnityEngine.Object.FindFirstObjectByType<GameOver>();if(game==null||game.GetComponent<PlayerFlashlights>()==null){game=null;return;}game.enabled=false;intro=game.gameObject.AddComponent<IntroVideo>();video=(VideoPlayer)typeof(IntroVideo).GetField("video",F).GetValue(intro);return;}
   if(intro!=null&&intro.Failed)throw new Exception("Native intro fell back before completion");
   if(stage==0&&video.isPlaying&&video.time>1){
    Check(video.isPrepared&&video.texture!=null,"Real local MP4 decodes and supplies frames");Call("OnApplicationFocus",false);Call("OnApplicationFocus",false);Call("OnApplicationPause",true);Call("OnApplicationFocus",true);pausedAt=video.time;next=EditorApplication.timeSinceStartup+1;stage=1;
   }else if(stage==1&&EditorApplication.timeSinceStartup>next){Check(!video.isPlaying&&Math.Abs(video.time-pausedAt)<.3,"Repeated focus notifications do not resume while app is suspended");Call("OnApplicationPause",false);stage=2;
   }else if(stage==2&&video.time>pausedAt+.5){Check(video.isPlaying,"Native playback resumes after both lifecycle gates clear");intro.Finish();Check(intro.Finished,"Skip ends a prepared intro");UnityEngine.Object.Destroy(intro);next=EditorApplication.timeSinceStartup+1;stage=3;
   }else if(stage==3&&EditorApplication.timeSinceStartup>next){intro=game.gameObject.AddComponent<IntroVideo>();video=(VideoPlayer)typeof(IntroVideo).GetField("video",F).GetValue(intro);stage=4;
   }else if(stage==4){if(video.time>1)advanced=true;if(intro.Finished){Check(advanced,"A fresh introduction advances before its natural ending");Check(errors==0,"Natural media completion has no game errors");Debug.Log("INTRO_NATIVE_COMPLETE checks="+checks+" errors="+errors+" result=0. Desktop codec only; Android callbacks/media still require device acceptance.");Finish(0);}}
  }catch(Exception e){Debug.LogException(e);Finish(1);}
 }
 static void Finish(int result){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(result);}
}
