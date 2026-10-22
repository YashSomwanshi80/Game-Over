using System;

// Pure movement rules; the Unity motor supplies grounded/clearance facts and collision results.
public sealed class LocomotionState
{
    public const float WalkSpeed=2.7f,SprintSpeed=4.4f,CrouchSpeed=1.3f;
    public const float StandingHeight=1.7f,CrouchingHeight=1.05f,Gravity=20,JumpHeight=.9f;
    public bool Crouched{get;private set;}
    public bool Sprinting{get;private set;}
    public bool Jumped{get;private set;}
    public bool StandBlocked{get;private set;}
    public bool Exhausted{get;private set;}
    public float Stamina{get;private set;}=1;
    public float VerticalSpeed{get;private set;}
    public float Speed=>Crouched?CrouchSpeed:Sprinting?SprintSpeed:WalkSpeed;
    public float Height=>Crouched?CrouchingHeight:StandingHeight;
    public float EyeHeight=>Crouched?.90f:1.55f;
    public float HorizontalX{get;private set;}
    public float HorizontalZ{get;private set;}
    float grace,buffer,restDelay;
    // World-space velocity avoids rotating existing momentum instantly with mouse look.
    public void MoveHorizontal(float targetX,float targetZ,bool allowed,float dt)
    {
        if(!allowed){HorizontalX=HorizontalZ=0;return;}
        dt=Math.Max(0,Math.Min(dt,.1f));
        float dx=targetX-HorizontalX,dz=targetZ-HorizontalZ;
        float distance=(float)Math.Sqrt(dx*dx+dz*dz);
        float current=HorizontalX*HorizontalX+HorizontalZ*HorizontalZ;
        float target=targetX*targetX+targetZ*targetZ;
        // Faster braking than acceleration keeps stealth positioning responsive.
        float step=(target<current?32:20)*dt;
        if(distance<=step){HorizontalX=targetX;HorizontalZ=targetZ;}
        else if(distance>0){HorizontalX+=dx/distance*step;HorizontalZ+=dz/distance*step;}
    }
    public void Tick(float dt,float movement,bool sprint,bool jump,bool toggleCrouch,bool grounded,bool standClear,bool allowMovement)
    {
        dt=Math.Max(0,Math.Min(dt,.1f));Jumped=StandBlocked=false;
        if(toggleCrouch&&allowMovement) {
            if(!Crouched)Crouched=true;
            else if(standClear)Crouched=false;
            else StandBlocked=true;
        }
        grace=grounded?.10f:Math.Max(0,grace-dt);
        buffer=jump&&allowMovement?.12f:Math.Max(0,buffer-dt);
        if(grounded&&VerticalSpeed<0)VerticalSpeed=-2;
        if(buffer>0&&grace>0&&!Crouched&&allowMovement) {
            VerticalSpeed=(float)Math.Sqrt(2*Gravity*JumpHeight);Jumped=true;buffer=grace=0;
        }
        Sprinting=allowMovement&&sprint&&!Crouched&&movement>.1f&&!Exhausted&&Stamina>0;
        if(Sprinting){Stamina=Math.Max(0,Stamina-dt*.2f);restDelay=1.1f;if(Stamina<=0){Exhausted=true;Sprinting=false;}}
        else {
            restDelay=Math.Max(0,restDelay-dt);
            if(restDelay<=0)Stamina=Math.Min(1,Stamina+dt*.16f);
            if(Exhausted&&Stamina>=.25f)Exhausted=false;
        }
        VerticalSpeed=Math.Max(-25,VerticalSpeed-Gravity*dt);
    }
    public void HitCeiling(){if(VerticalSpeed>0)VerticalSpeed=0;}
    public void Suspend(){HorizontalX=HorizontalZ=0;Sprinting=Jumped=false;VerticalSpeed=0;grace=buffer=0;}
}
