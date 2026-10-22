using UnityEngine;

public sealed class FirstPersonMotor : MonoBehaviour
{
    public readonly LocomotionState State=new LocomotionState();
    CharacterController controller;
    readonly Collider[] clearance=new Collider[32];
    public bool Grounded{get;private set;}
    public bool Landed{get;private set;}
    public float HorizontalSpeed{get;private set;}
    public Vector3 LastGroundedPosition{get;private set;}
    public void Configure(CharacterController body)
    {
        controller=body;controller.skinWidth=.035f;controller.stepOffset=.28f;controller.minMoveDistance=0;
        LastGroundedPosition=transform.position;
    }
    public bool CanOccupy(Vector3 feet,float height)
    {
        const float radius=.23f;
        int count=Physics.OverlapCapsuleNonAlloc(feet+Vector3.up*.265f,feet+Vector3.up*(height-.265f),radius,clearance,~(1<<8),QueryTriggerInteraction.Ignore);
        if(count==clearance.Length)return false;
        for(int i=0;i<count;i++)if(clearance[i]!=null&&clearance[i]!=controller)return false;
        return true;
    }
    public void Tick(Vector2 input,bool sprint,bool jump,bool crouch,bool horizontalAllowed,float dt)
    {
        Landed=false;bool before=controller.isGrounded;
        if(before)LastGroundedPosition=transform.position;
        State.Tick(dt,input.magnitude,sprint,jump,crouch,before,CanOccupy(transform.position,LocomotionState.StandingHeight),horizontalAllowed);
        if(!Mathf.Approximately(controller.height,State.Height)) {
            controller.height=State.Height;controller.center=Vector3.up*(State.Height/2);
        }
        Vector3 direction=horizontalAllowed?transform.TransformDirection(new Vector3(input.x,0,input.y)):Vector3.zero;
        float vertical=State.VerticalSpeed;
        Vector3 target=direction*State.Speed;
        State.MoveHorizontal(target.x,target.z,horizontalAllowed,dt);
        var horizontal=new Vector3(State.HorizontalX,0,State.HorizontalZ);
        var flags=controller.Move((horizontal+Vector3.up*vertical)*Mathf.Clamp(dt,0,.1f));
        if((flags&CollisionFlags.Above)!=0)State.HitCeiling();
        Grounded=(flags&CollisionFlags.Below)!=0;
        if(Grounded)LastGroundedPosition=transform.position;
        Landed=!before&&Grounded&&vertical< -4;
        var velocity=controller.velocity;HorizontalSpeed=new Vector2(velocity.x,velocity.z).magnitude;
    }
    public void Suspend(float dt)
    {
        State.Tick(dt,0,false,false,false,true,true,false);State.Suspend();Landed=false;HorizontalSpeed=0;
    }
    public void ResetVertical(){State.Suspend();Grounded=false;Landed=false;HorizontalSpeed=0;}
}
