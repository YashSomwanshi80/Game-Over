using UnityEngine;
using System.Collections.Generic;

// User-recorded character Foley with procedural surface fallback. Shared cache for all actors.
public sealed class SurfaceFootsteps : MonoBehaviour
{
    enum Surface {Wood,Hard,Carpet}
    readonly List<Bounds> rugs=new List<Bounds>();
    readonly List<AudioClip> clips=new List<AudioClip>();
    readonly RaycastHit[] hits=new RaycastHit[16];
    int variant;
    readonly AudioClip[] light=new AudioClip[3],heavy=new AudioClip[3];
    public void Build()
    {
        for(int i=0;i<3;i++){light[i]=Resources.Load<AudioClip>("Presentation/Audio/TapseeStep"+i);heavy[i]=Resources.Load<AudioClip>("Presentation/Audio/DonStep"+i);}
        foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)) {
            string name=renderer.name.ToLowerInvariant();
            // Spatial batching disables originals but retains their world bounds.
            if(name.Contains("rug")||name.Contains("carpet"))rugs.Add(renderer.bounds);
        }
        for(int surface=0;surface<3;surface++)for(int take=0;take<3;take++) {
            const int rate=22050;var samples=new float[4410];var noise=new System.Random(571+surface*41+take);
            float filtered=0,frequency=surface==0?90:surface==1?150:65;
            for(int i=0;i<samples.Length;i++) {
                float t=(float)i/rate;
                float raw=(float)(noise.NextDouble()*2-1);filtered=Mathf.Lerp(filtered,raw,surface==2?.09f:.38f);
                float heel=Mathf.Exp(-t*55)*Mathf.Min(t*900,1);
                float toe=t>.075f?Mathf.Exp(-(t-.075f)*65)*Mathf.Min((t-.075f)*750,1):0;
                float resonance=Mathf.Sin(2*Mathf.PI*(frequency+take*7)*t)*Mathf.Exp(-t*38);
                float contact=surface==1?raw*.48f:filtered*(surface==2?.5f:.8f);
                samples[i]=(contact*(heel+toe*.55f)+resonance*(surface==2?.18f:.4f))*.8f;
            }
            var clip=AudioClip.Create(((Surface)surface)+" footstep / take "+take,samples.Length,1,rate,false);
            clip.SetData(samples,0);clips.Add(clip);
        }
    }
    Surface Underfoot(Vector3 feet)
    {
        Vector3 ground=feet;string floor="";float nearest=float.PositiveInfinity;
        int count=Physics.RaycastNonAlloc(feet+Vector3.up*.12f,Vector3.down,hits,1.8f,~(1<<8),QueryTriggerInteraction.Ignore);
        for(int i=0;i<count;i++) {
            if(hits[i].normal.y<.6f||hits[i].collider is CharacterController)continue;
            if(hits[i].distance<nearest){nearest=hits[i].distance;ground=hits[i].point;floor=hits[i].collider.name.ToLowerInvariant();}
        }
        foreach(var rug in rugs)
            if(ground.x>=rug.min.x&&ground.x<=rug.max.x&&ground.z>=rug.min.z&&ground.z<=rug.max.z&&Mathf.Abs(ground.y-rug.center.y)<.12f)return Surface.Carpet;
        return floor.Contains("garden")||floor.Contains("paving")||floor.Contains("garage")||floor.Contains("washroom")||floor.Contains("bathroom")||floor.Contains("basement")?Surface.Hard:Surface.Wood;
    }
    public float HearingScale(Vector3 feet) => Underfoot(feet)==Surface.Carpet?.65f:1;
    public void Play(AudioSource source,Vector3 feet,float gain=1,bool heavyBoots=false)
    {
        if(source==null||clips.Count!=9)return;
        var surface=Underfoot(feet);int take=variant++%3;
        var recorded=(heavyBoots?heavy:light)[take];
        source.PlayOneShot(recorded!=null?recorded:clips[(int)surface*3+take],gain*(surface==Surface.Carpet?.65f:1));
    }
    void OnDestroy(){foreach(var clip in clips)if(clip!=null)Destroy(clip);}
}
