using UnityEngine;
// One rendered feed shared by local HUDs; switching never changes authoritative match state.
public sealed class HouseCctv : MonoBehaviour {
 public static int Selected {get;private set;}
 public static readonly Vector3[] Positions={new Vector3(9.5f,2.75f,6.5f),new Vector3(.9f,6.15f,1.5f),new Vector3(-9.65f,-.65f,6.5f),new Vector3(4.7f,-.65f,1.5f),new Vector3(-3,5.4f,-8.1f),new Vector3(16.5f,3,-7.6f),new Vector3(8.8f,5.4f,7.6f)};
 public static readonly Vector3[] Targets={new Vector3(4.5f,.8f,-1),new Vector3(0,4.2f,-2),new Vector3(-7.1f,-2.4f,2.5f),new Vector3(1.5f,-2.4f,-1.2f),new Vector3(0,1,-16.7f),new Vector3(13.5f,.7f,-14),new Vector3(0,.7f,14)};
 static readonly string[] Names={"LIVING ROOM","FIRST FLOOR / STAIRS","BASEMENT / OFFICE","BASEMENT / EAST HALL","EXTERIOR / MAIN GATE","EXTERIOR / DRIVEWAY","EXTERIOR / BACK GARDEN"};
 public static int FloorOf(int i)=>Positions[i].y<-.3f?0:Positions[i].y<3.3f?1:2; // 0 basement, 1 ground, 2 first floor
 public static string Label=>"CAM "+(Selected+1).ToString("00")+" / "+Names[Selected];
 public static void Select(int direction){Selected=(Selected+direction%Names.Length+Names.Length)%Names.Length;}
 public static void ApplySelected(Camera camera){camera.cullingMask&=~(1<<11);camera.transform.position=Positions[Selected];camera.transform.LookAt(Targets[Selected]);}
 public void Build(Camera camera){Selected=0;ApplySelected(camera);var pbr=FindFirstObjectByType<PbrLibrary>();if(pbr==null)return;for(int i=1;i<Positions.Length;i++){
  var mount=new GameObject("CCTV mount / "+Names[i]);mount.transform.SetParent(transform,false);mount.transform.position=Positions[i];mount.transform.rotation=Quaternion.LookRotation(Targets[i]-Positions[i]);
  VisualMesh.Rounded("Security camera housing",Vector3.zero,new Vector3(.2f,.14f,.3f),.015f,pbr.Brass,mount.transform).transform.localPosition=Vector3.zero;
  VisualMesh.Rounded("Security camera lens",Vector3.zero,new Vector3(.1f,.09f,.02f),.005f,pbr.Hair,mount.transform).transform.localPosition=new Vector3(0,0,.16f);
  foreach(var r in mount.GetComponentsInChildren<Renderer>())r.gameObject.layer=11;
 }}
}
