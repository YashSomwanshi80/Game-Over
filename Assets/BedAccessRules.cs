using System;
// Metre-scale footprint of the existing bedroom beds; shared by focus and offline tests.
public static class BedAccessRules {
 public static float EdgeX(float value)=>Math.Max(-1.32f,Math.Min(1.32f,value));
 public static float EdgeZ(float value)=>Math.Max(-1.87f,Math.Min(1.87f,value));
 public static bool Eligible(float dx,float dz,float eyeHeight,float fx,float fy,float fz){
  if(eyeHeight<.45f||eyeHeight>2.05f||Math.Abs(dx)<1.3f&&Math.Abs(dz)<1.85f||fy>.45f)return false;
  float vx=EdgeX(dx)-dx,vz=EdgeZ(dz)-dz;double distance=Math.Sqrt(vx*vx+vz*vz),look=Math.Sqrt(fx*fx+fz*fz);
  return distance>.01&&distance<=1.45&&look>.001&&(vx*fx+vz*fz)/(distance*look)>=.55;
 }
}
