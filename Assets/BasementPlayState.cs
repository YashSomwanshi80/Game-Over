using System;
// Host owns reusable distraction timing and each role's one spare battery.
public sealed class BasementPlayState {
 public float RadioUntil,RadioReadyAt;public int TakenMask;
 public bool RadioPlaying(float clock)=>clock<RadioUntil;
 public bool CrankRadio(float clock){if(clock<RadioReadyAt)return false;RadioUntil=clock+8;RadioReadyAt=clock+24;return true;}
 public bool HasSpare(int role)=>(TakenMask&(1<<role))==0;
 public bool TakeSpare(int role,float charge){if(role<0||role>1||!HasSpare(role)||float.IsNaN(charge)||float.IsInfinity(charge)||charge<0||charge>=.999f)return false;TakenMask|=1<<role;return true;}
}
