using UnityEngine;
// Bedroom beds have access along both sides and the foot, not one small fixed anchor.
public static class HidingAccess {
 public static bool IsBedroomBed(HidingLayout.Spot s)=>s.bed&&s.name.EndsWith(" bed");
 public static Vector3 Prompt(HidingLayout.Spot s,Vector3 eye){if(!IsBedroomBed(s))return new Vector3(s.px,s.py,s.pz);return new Vector3(s.x+BedAccessRules.EdgeX(eye.x-s.x),s.y+.65f,s.z+BedAccessRules.EdgeZ(eye.z-s.z));}
 public static bool BedVisible(HidingLayout.Spot s,Vector3 eye,Vector3 forward,Collider actor){
  if(!IsBedroomBed(s)||!BedAccessRules.Eligible(eye.x-s.x,eye.z-s.z,eye.y-s.y,forward.x,forward.y,forward.z))return false;
  Vector3 point=Prompt(s,eye),delta=point-eye;
  RaycastHit hit;if(!InteractionRaycast.First(eye,delta,delta.magnitude,actor,out hit))return true;
  string n=hit.collider.name;bool bed=n.StartsWith("Bed frame")||n=="Mattress"||n=="Pillow"||n=="Bed blanket"||n=="Bed headboard";
  var center=hit.collider.bounds.center;return bed&&Mathf.Abs(center.x-s.x)<1.5f&&Mathf.Abs(center.z-s.z)<2.1f;
 }
}
