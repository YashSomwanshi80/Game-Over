using UnityEngine;
// Existing room geometry remains in place. These are small, bounded stealth interactions.
public sealed class BasementPlay : MonoBehaviour {
 public readonly BasementPlayState State=new BasementPlayState();
 public static readonly Vector3 RadioPoint=new Vector3(-4.25f,-2.5f,-.85f);
 public static readonly Vector3 DrawerPoint=new Vector3(7.15f,-2.25f,-5.65f);
 AudioSource radio;AudioClip radioClip;Transform[] cells=new Transform[2];Transform drawer;
 public void Build(PbrLibrary pbr){
  foreach(var part in BasementPlayLayout.Parts())VisualMesh.Rounded(part.name+" / finish",new Vector3(part.x,part.y,part.z),new Vector3(part.w,part.h,part.d),.012f,part.material=="fabric"?pbr.Linen:HidingDecor.Finish(pbr,part.material),transform);
  for(int i=0;i<6;i++)VisualMesh.Rounded("Radio speaker grille",RadioPoint+new Vector3(-.1f,-.12f+i*.045f,-.157f),new Vector3(.19f,.012f,.012f),.003f,pbr.Rubber,transform);
  foreach(float x in new[]{.07f,.17f})VisualMesh.Rounded("Radio tuning knob",RadioPoint+new Vector3(x,-.08f,-.17f),new Vector3(.055f,.055f,.025f),.02f,pbr.Brass,transform);
  VisualMesh.Rounded("Radio hand crank",RadioPoint+new Vector3(.31f,0,0),new Vector3(.16f,.035f,.035f),.008f,pbr.Brass,transform);
  VisualMesh.Rounded("Radio crank grip",RadioPoint+new Vector3(.38f,.06f,0),new Vector3(.035f,.15f,.035f),.01f,pbr.Rubber,transform);
  drawer=VisualMesh.Rounded("Supply drawer / sliding front",DrawerPoint+new Vector3(0,-.06f,.25f),new Vector3(.6f,.2f,.045f),.012f,pbr.Metal,transform).transform;
  VisualMesh.Rounded("Drawer pull",new Vector3(0,0,.04f),new Vector3(.22f,.04f,.055f),.008f,pbr.Brass,drawer);
  for(int i=0;i<2;i++)cells[i]=VisualMesh.Rounded("Spare flashlight battery",DrawerPoint+new Vector3(-.14f+i*.28f,.11f,0),new Vector3(.09f,.065f,.23f),.025f,pbr.Brass,transform).transform;
  foreach(float x in new[]{8.42f,9.58f})foreach(float z in new[]{3.39f,4.11f})VisualMesh.Rounded("Trolley rubber castor",new Vector3(x,-3.36f,z),new Vector3(.12f,.08f,.12f),.035f,pbr.Rubber,transform);
  foreach(float z in new[]{3.45f,3.7f,3.95f})CoutureMesh.Cushion("Folded clean linen",new Vector3(9,-2.17f,z),new Vector3(1.05f,.12f,.24f),.035f,pbr.Linen,transform);
  radio=new GameObject("Hand-cranked radio / spatial diversion").AddComponent<AudioSource>();radio.transform.SetParent(transform,false);radio.transform.position=RadioPoint;radio.playOnAwake=false;radio.loop=true;radio.spatialBlend=1;radio.minDistance=2;radio.maxDistance=18;radio.dopplerLevel=0;radio.volume=.55f;
  const int rate=22050;var samples=new float[rate];var rng=new System.Random(912);float filtered=0;
  for(int i=0;i<samples.Length;i++){float t=(float)i/rate;filtered=Mathf.Lerp(filtered,(float)(rng.NextDouble()*2-1),.25f);float burst=Mathf.Sin(Mathf.PI*t);samples[i]=burst*(filtered*.35f+Mathf.Sin(2*Mathf.PI*740*t)*.13f*(.5f+.5f*Mathf.Sin(t*28)));}
  radioClip=AudioClip.Create("Original hand-crank radio crackle",samples.Length,1,rate,false);radioClip.SetData(samples,0);radio.clip=radioClip;
 }
 public int Focus(Vector3 origin,Vector3 forward,Collider actor){RaycastHit hit;if(!InteractionRaycast.First(origin,forward,2.5f,actor,out hit))return -1;if(hit.collider.name=="Basement play / radio")return 0;if(hit.collider.name=="Basement play / supply drawer")return 1;return -1;}
 public string Label(int index,int role,float clock,float charge){if(index==0)return clock<State.RadioReadyAt?"RADIO / "+Mathf.CeilToInt(State.RadioReadyAt-clock)+"s":"CRANK RADIO";return !State.HasSpare(role)?"SPARE TAKEN":charge>=.999f?"BATTERY FULL":"TAKE SPARE";}
 public Vector3 Prompt(int index)=>index==0?RadioPoint:DrawerPoint;
 public bool Use(int index,int role,float clock,PlayerFlashlights lights,out string message){
  if(index==0){bool ok=State.CrankRadio(clock);message=ok?"Radio winding down. Its noise can give away this room.":"The radio needs time before it can be wound again.";return ok;}
  if(State.TakeSpare(role,lights.Charge(role))){float before=lights.Charge(role);lights.Recharge(role,.25f);message="Spare battery fitted: +"+Mathf.RoundToInt((lights.Charge(role)-before)*100)+"% charge.";}else message=!State.HasSpare(role)?"You already took your spare battery.":"Your flashlight battery is full. Save the spare for later.";return false;
 }
 public void Present(float clock,bool active){bool sound=active&&State.RadioPlaying(clock);if(sound&&!radio.isPlaying)radio.Play();else if(!sound&&radio.isPlaying)radio.Stop();for(int i=0;i<2;i++)cells[i].gameObject.SetActive((State.TakenMask&(1<<i))==0);drawer.localPosition=new Vector3(DrawerPoint.x,DrawerPoint.y-.06f,DrawerPoint.z+.25f+(State.TakenMask!=0?.15f:0));}
 void OnApplicationFocus(bool focused){if(!focused&&radio!=null)radio.Stop();}
 void OnApplicationPause(bool paused){if(paused&&radio!=null)radio.Stop();}
 void OnDestroy(){if(radioClip!=null)Destroy(radioClip);}
}
