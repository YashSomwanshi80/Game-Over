using System;
using UnityEngine;

[Serializable] public sealed class RivalPacket
{
    public bool jam;public float jamUntil,jamReadyAt;
    public float radioUntil,radioReadyAt;public int spareTakenMask;
    public int doorSwingMask;
    public int version=9,kind,role,sequence;
    public float x,z,yaw,pitch;public bool sprint,jump,crouch,action,call,flashToggle;
    public bool tapFlashOn,donFlashOn;public float tapFlashCharge=1,donFlashCharge=1,tapFlashYaw,donFlashYaw,tapFlashPitch,donFlashPitch;
    public Vector3 tapsee,don;public float tapseeYaw,donYaw,eyeHeight,donEyeHeight,stamina,progress,rescue,time,donSpeed,tapseeSpeed;
    public bool hidden,bathtub,crouched,called,tapGrounded,donGrounded;public int outcome;public bool powered=true;
    public int houseLightMask=HouseLightingLayout.AllOn;public float[] roomDoors;public int bedroomLightMask=3;public int doorMask;public float westDoor,eastDoor,libraryDoor,studioDoor,entranceDoor,gardenRoomDoor,gardenExitDoor,westBalconyDoor,frontBalconyDoor;
    public int noticeId;public string noticeText;
    public bool ValidInput()=>Finite(x)&&Finite(z)&&Finite(yaw)&&Finite(pitch)&&Mathf.Abs(x)<=1.01f&&Mathf.Abs(z)<=1.01f&&Mathf.Abs(pitch)<=80&&Mathf.Abs(yaw)<100000;
    static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
}
