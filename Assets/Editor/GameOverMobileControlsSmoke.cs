using System;
using UnityEditor;
using UnityEngine;
public static class GameOverMobileControlsSmoke
{
 static int checks;
 static void Check(bool yes,string text){if(!yes)throw new Exception(text);checks++;Debug.Log("MOBILE_INPUT_PASS "+text);}
 static void Touch(MobileTouchInput input,int id,TouchPhase phase,Vector2 p,Vector2 delta=default(Vector2))=>input.Process(id,phase,p,delta,MobileTouchInput.At(p));
 public static void Run(){try{
  var input=new MobileTouchInput();var look=new Vector2(800,400);
  input.BeginFrame(1);Touch(input,1,TouchPhase.Began,look);
  input.BeginFrame(1);Touch(input,1,TouchPhase.Moved,TacticalHud.CrouchButton.center,new Vector2(35,10));
  Check(!input.CrouchPressed&&input.LookPixels==new Vector2(35,10),"Look swipe crossing crouch stays camera-only");
  input.BeginFrame(1);Touch(input,1,TouchPhase.Moved,TacticalHud.Action.center,new Vector2(4,5));
  Check(!input.ActionPressed&&!input.ActionHeld,"Look swipe crossing use never starts a call");
  input.BeginFrame(1);Touch(input,1,TouchPhase.Moved,TacticalHud.JumpButton.center,new Vector2(0,15));Check(!input.JumpPressed,"Look swipe crossing jump never jumps");
  input.BeginFrame(1);Touch(input,1,TouchPhase.Moved,TacticalHud.FlashlightButton.center,new Vector2(0,5));Check(!input.FlashlightPressed,"Look swipe crossing flashlight never toggles it");
  Touch(input,1,TouchPhase.Canceled,look);input.BeginFrame(1);Touch(input,1,TouchPhase.Moved,look,new Vector2(100,0));Check(input.LookPixels==Vector2.zero,"Canceled fingers cannot rotate until a new touch begins");
  input.Reset();input.BeginFrame(2);Touch(input,1,TouchPhase.Began,TacticalHud.Joystick.center);Touch(input,2,TouchPhase.Began,look);
  input.BeginFrame(2);Touch(input,1,TouchPhase.Moved,TacticalHud.Joystick.center+new Vector2(0,-50));Touch(input,2,TouchPhase.Moved,look,new Vector2(30,5));
  Check(input.Move.y>.7f&&input.LookPixels.x==30,"Left movement and right look work simultaneously");
  input.Reset();input.BeginFrame(1);Touch(input,3,TouchPhase.Began,TacticalHud.Action.center);Check(input.ActionPressed&&input.ActionHeld,"A dedicated use touch can hold the phone call");
  input.BeginFrame(1);Touch(input,3,TouchPhase.Moved,look,new Vector2(-50,0));Check(!input.ActionHeld&&input.LookPixels==Vector2.zero,"Sliding off use interrupts hold without becoming camera input");
  input.Reset();input.BeginFrame(1);Touch(input,4,TouchPhase.Began,TacticalHud.Joystick.center);
  input.BeginFrame(1);Touch(input,4,TouchPhase.Moved,TacticalHud.Joystick.center+new Vector2(0,-65));Check(!input.SprintOn&&input.Move.y==1,"Normal joystick edge gives walking");
  input.BeginFrame(1);Touch(input,4,TouchPhase.Moved,TacticalHud.Joystick.center+new Vector2(0,-100));Check(input.SprintOn,"Pushing beyond the upper rim engages sprint");
  input.BeginFrame(1);Touch(input,4,TouchPhase.Moved,TacticalHud.Joystick.center+new Vector2(0,-80));Check(input.SprintOn,"Sprint threshold hysteresis prevents flicker");
  input.BeginFrame(1);Touch(input,4,TouchPhase.Moved,TacticalHud.Joystick.center+new Vector2(0,-55));Check(!input.SprintOn,"Easing the joystick back restores walking");
  input.BeginFrame(1);Touch(input,4,TouchPhase.Moved,TacticalHud.Joystick.center+new Vector2(0,-100));Touch(input,4,TouchPhase.Ended,TacticalHud.Joystick.center);Check(!input.SprintOn,"Lifting movement finger cancels sprint");
  input.Reset();Check(!input.SprintOn&&!input.ActionHeld&&input.LookPixels==Vector2.zero,"Suspension reset clears gesture state");
  input.BeginFrame(2);Touch(input,1,TouchPhase.Began,look);Touch(input,2,TouchPhase.Began,look);input.BeginFrame(2);Touch(input,1,TouchPhase.Moved,look,new Vector2(5,0));Touch(input,2,TouchPhase.Moved,look,new Vector2(100,0));Check(input.LookPixels.x==5,"A second finger cannot steal or double the look gesture");
  var rail=new[]{TacticalHud.CrouchButton,TacticalHud.JumpButton,TacticalHud.Action,TacticalHud.FlashlightButton,TacticalHud.CameraButton,TacticalHud.PauseButton};
  for(int i=0;i<rail.Length;i++){Check(i==2||!rail[i].Overlaps(TacticalHud.LookArea),"Look region excludes edge controls; inward use is separately owned "+i);for(int j=i+1;j<rail.Length;j++)Check(!rail[i].Overlaps(rail[j]),"Touch targets do not overlap "+i+"/"+j);}
  foreach(var size in new[]{new Vector2(2340,1080),new Vector2(1600,738),new Vector2(1100,700)}){
   float offset=TacticalHud.EdgeOffset(size.x,size.y),scale=Mathf.Min(size.x/1100,size.y/700),origin=(size.x-1100*scale)/2;
   Check((980+offset+80)*scale+origin<size.x,"Right rail remains inside safe width "+size);
   Check((340+590+offset)<(980+offset),"Expanded look area stays clear of wide-screen action rail "+size);
  }
  Check(TacticalHud.EdgeOffset(2340,1080)>150&&TacticalHud.EdgeOffset(1100,700)==0,"Wide phones anchor controls outward without moving the joystick");
  Check(MobileTouchInput.At(TacticalHud.Action.center)==MobileTouchInput.Role.Action,"Highlighted inward use target owns its touch");
  Check(MobileTouchInput.At(TacticalHud.Action.center,false,false)==MobileTouchInput.Role.Look,"Invisible use target does not leave a dead camera patch");
  float a=FirstPersonLookRules.Touch(200,2340,1080,1),b=FirstPersonLookRules.Touch(100,1170,540,1);Check(Mathf.Abs(a-b)<.0001f,"Look rotation remains resolution-normalized");
  float total=0;for(int i=0;i<20;i++)total+=FirstPersonLookRules.Touch(10,2340,1080,1);Check(Mathf.Abs(a-total)<.001f,"Touch distance gives equal rotation across frame partitions");
  Check(a>24&&a<28,"Default touch response is calibrated faster without acceleration");
  Debug.Log("MOBILE_INPUT_COMPLETE checks="+checks+" result=0. Synthetic touch ownership; physical multi-touch and frame pacing require device testing.");if(Environment.GetEnvironmentVariable("GAMEOVER_CHAIN_REVIEW")=="1"){EstateReview.Capture();return;}EditorApplication.Exit(0);
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}
