using UnityEngine;
public static class CaptureFacing {
 public const float Reach=.9f,ConeDegrees=100; // total facing cone; half-angle is 50 degrees
 public static bool InReach(Vector3 don,Vector3 forward,Vector3 tapsee){var delta=tapsee-don;if(delta.sqrMagnitude>=Reach*Reach||Mathf.Abs(delta.y)>.9f)return false;delta.y=forward.y=0;return delta.sqrMagnitude>.0001f&&forward.sqrMagnitude>.0001f&&Vector3.Dot(forward.normalized,delta.normalized)>=Mathf.Cos(ConeDegrees*.5f*Mathf.Deg2Rad);}
}
