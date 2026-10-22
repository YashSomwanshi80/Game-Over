using UnityEngine;
using System.Collections.Generic;

// A gesture owns its initial role until lift/cancel; crossing UI never becomes a button press.
public sealed class MobileTouchInput
{
    public enum Role {Blocked,Move,Look,Crouch,Jump,Action,Camera,Flashlight,Pause}
    readonly Dictionary<int,Role> owners=new Dictionary<int,Role>();
    public Vector2 Move,LookPixels;
    public bool SprintOn{get;private set;}
    public bool ActionHeld{get;private set;}
    public bool ActionPressed,JumpPressed,CrouchPressed,CameraPressed,FlashlightPressed,PausePressed;
    public void BeginFrame(int count){Move=LookPixels=Vector2.zero;ActionHeld=false;ActionPressed=JumpPressed=CrouchPressed=CameraPressed=FlashlightPressed=PausePressed=false;if(count==0){owners.Clear();SprintOn=false;}}
    public void Reset(){owners.Clear();SprintOn=false;BeginFrame(0);}
    public void Process(int id,TouchPhase phase,Vector2 point,Vector2 delta,Role beganRole)
    {
        if(phase==TouchPhase.Began){
            var role=beganRole;
            if((role==Role.Move||role==Role.Look)&&owners.ContainsValue(role))role=Role.Blocked;
            owners[id]=role;
            switch(role){
                case Role.Crouch:CrouchPressed=true;SprintOn=false;break;
                case Role.Jump:JumpPressed=true;break;
                case Role.Action:ActionPressed=true;break;
                case Role.Camera:CameraPressed=true;break;
                case Role.Flashlight:FlashlightPressed=true;break;
                case Role.Pause:PausePressed=true;break;
            }
        }
        if(!owners.TryGetValue(id,out var owned))return;
        if(phase==TouchPhase.Ended||phase==TouchPhase.Canceled){if(owned==Role.Move)SprintOn=false;owners.Remove(id);return;}
        if(owned==Role.Move){
            var raw=new Vector2(point.x-TacticalHud.Joystick.center.x,TacticalHud.Joystick.center.y-point.y)/65;
            Move=Vector2.ClampMagnitude(raw,1);
            SprintOn=SprintOn?raw.y>1.05f&&Mathf.Abs(raw.x)<1:raw.y>=1.4f&&Mathf.Abs(raw.x)<.8f;
        }
        if(owned==Role.Look&&phase==TouchPhase.Moved)LookPixels+=delta;
        if(owned==Role.Action&&TacticalHud.Action.Contains(point))ActionHeld=true;
    }
    public static Role At(Vector2 p,bool blocked=false,bool actionAvailable=true)
    {
        if(blocked||TacticalHud.MinimapPanel.Contains(p))return Role.Blocked;
        if(TacticalHud.PauseButton.Contains(p))return Role.Pause;
        if(TacticalHud.Joystick.Contains(p))return Role.Move;
        if(TacticalHud.CrouchButton.Contains(p))return Role.Crouch;
        if(TacticalHud.JumpButton.Contains(p))return Role.Jump;
        if(actionAvailable&&TacticalHud.Action.Contains(p))return Role.Action;
        if(TacticalHud.CameraButton.Contains(p))return Role.Camera;
        if(TacticalHud.FlashlightButton.Contains(p))return Role.Flashlight;
        return TacticalHud.LookArea.Contains(p)?Role.Look:Role.Blocked;
    }
}
