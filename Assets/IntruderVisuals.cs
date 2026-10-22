using UnityEngine;
using UnityEngine.AI;

// A prefab with an Animator can replace this fallback without touching the NavMeshAgent.
// Import a rigged model in Unity and save it as Resources/Characters/Don.prefab.
// Optional parameters: Speed/Alert (float), Crouched (bool). Root motion stays disabled.
public sealed class IntruderVisuals : MonoBehaviour
{
    NavMeshAgent agent;Animator animator;
    Transform torso,head,leftArm,rightArm,leftForearm,rightForearm,leftThigh,rightThigh,leftShin,rightShin,leftTail,rightTail;
    float phase,speed,alert,targetAlert;bool active;
    bool hasSpeed,hasAlert,hasCrouch,humanCrouched;
    PbrLibrary pbr;Material coat;
    public bool ImportedModel{get;private set;}
    float externalSpeed=-1;
    public void SetHumanMotion(float value,bool crouched=false){externalSpeed=value;humanCrouched=crouched;}
    public void Build(NavMeshAgent actor,PbrLibrary library)
    {
        agent=actor;pbr=library;
        var capsule=actor.GetComponent<Renderer>();if(capsule!=null) capsule.enabled=false;
        transform.SetParent(actor.transform,false);transform.localPosition=Vector3.down;
        var prefab=Resources.Load<GameObject>("Characters/Don");
        if(prefab!=null) {
            ImportedModel=true;
            var model=Instantiate(prefab,transform);model.name="Don / imported rig";
            CharacterVisibility.Configure(model);
            animator=model.GetComponentInChildren<Animator>();if(animator!=null) {animator.applyRootMotion=false;hasSpeed=HasParameter("Speed");hasAlert=HasParameter("Alert");}
            if(animator!=null)foreach(var parameter in animator.parameters)if(parameter.name=="Crouched"&&parameter.type==AnimatorControllerParameterType.Bool)hasCrouch=true;
            foreach(var c in model.GetComponentsInChildren<Collider>()) c.enabled=false;
        } else BuildTailoredFigure();
        var beam=new GameObject("Intruder chest torch").AddComponent<Light>();beam.transform.SetParent(transform,false);
        beam.transform.localPosition=new Vector3(.12f,1.4f,.23f);beam.transform.localRotation=Quaternion.Euler(14,0,0);
        beam.type=LightType.Spot;beam.range=6;beam.spotAngle=58;beam.innerSpotAngle=26;
        beam.color=new Color(.47f,.61f,.69f);beam.intensity=.75f;
        beam.shadows=LightShadows.Hard;beam.shadowBias=.03f;beam.shadowNormalBias=.12f;
        foreach(var renderer in GetComponentsInChildren<Renderer>())renderer.gameObject.layer=10;
    }
    void BuildTailoredFigure()
    {
        coat=new Material(pbr.CharacterCloth);coat.name="Don / wool trench cloth";coat.color=new Color(.065f,.072f,.083f);coat.SetFloat("_Glossiness",.17f);
        torso=VisualMesh.Joint("Spine / chest",new Vector3(0,1.03f,0),transform);
        Profile("Tailored chest",torso,Vector3.zero,new[]{new Vector2(.19f,0),new Vector2(.215f,.17f),new Vector2(.29f,.48f),new Vector2(.24f,.56f),new Vector2(.12f,.60f)},coat,.64f);
        Profile("Turtleneck",torso,new Vector3(0,.49f,0),new[]{new Vector2(.12f,0),new Vector2(.105f,.14f),new Vector2(.08f,.18f)},pbr.Hair,.83f);
        foreach(float side in new[]{-1f,1f}) {
            var lapel=R("Folded trench lapel",torso,new Vector3(side*.105f,.37f,.168f),new Vector3(.11f,.34f,.026f),pbr.Hair,.012f);
            lapel.localRotation=Quaternion.Euler(-8,0,side*-21);
            R("Collar stand",torso,new Vector3(side*.11f,.565f,.07f),new Vector3(.13f,.12f,.10f),coat,.02f).localRotation=Quaternion.Euler(12,0,side*22);
        }
        for(int i=0;i<4;i++) R("Coat horn button",torso,new Vector3(.035f,.13f+i*.09f,.157f),new Vector3(.023f,.023f,.013f),pbr.Brass,.01f);
        R("Coat waist belt",torso,new Vector3(0,.06f,0),new Vector3(.41f,.043f,.29f),pbr.Hair,.02f);
        R("Belt buckle",torso,new Vector3(0,.06f,.154f),new Vector3(.055f,.045f,.018f),pbr.Brass,.009f);
        leftTail=Tail(-1);rightTail=Tail(1);
        head=VisualMesh.Joint("Neck / head",new Vector3(0,1.67f,0),transform);
        HumanAnatomy.Head("Contoured jaw, cheek and nose geometry",Vector3.zero,pbr.Skin,head);
        CharacterCraft.FaceDetails(head,Vector3.zero,pbr);
        Profile("Swept hair cap",head,new Vector3(0,.20f,-.018f),new[]{new Vector2(.12f,0),new Vector2(.136f,.05f),new Vector2(.12f,.11f),new Vector2(.05f,.15f),new Vector2(0,.15f)},pbr.Hair,.9f);
        foreach(float side in new[]{-1f,1f}) {
            R("Ear",head,new Vector3(side*.125f,.13f,-.005f),new Vector3(.035f,.078f,.041f),pbr.Skin,.015f);
            R("Sunglasses metal rim",head,new Vector3(side*.068f,.185f,.113f),new Vector3(.125f,.061f,.025f),pbr.Metal,.021f);
            R("Sunglasses smoked lens",head,new Vector3(side*.068f,.185f,.129f),new Vector3(.108f,.046f,.012f),pbr.Glass,.016f);
            R("Glasses temple",head,new Vector3(side*.128f,.186f,.04f),new Vector3(.014f,.013f,.16f),pbr.Metal,.005f);
        }
        R("Sunglasses bridge",head,new Vector3(0,.19f,.128f),new Vector3(.035f,.013f,.02f),pbr.Metal,.005f);
        leftArm=Arm(-1,out leftForearm);rightArm=Arm(1,out rightForearm);
        leftThigh=Leg(-1,out leftShin);rightThigh=Leg(1,out rightShin);
        foreach(float side in new[]{-1f,1f}) {
            R("Coat pocket opening",torso,new Vector3(side*.18f,.16f,.145f),new Vector3(.105f,.014f,.017f),pbr.Leather,.005f).localRotation=Quaternion.Euler(0,0,side*12);
            R("Tailored shoulder seam",torso,new Vector3(side*.23f,.49f,.015f),new Vector3(.012f,.018f,.23f),pbr.CharacterCloth,.004f);
            for(int i=0;i<3;i++) R("Sleeve cuff button",side<0?leftForearm:rightForearm,new Vector3(side*.054f,-.24f+i*.025f,.005f),new Vector3(.012f,.016f,.016f),pbr.Metal,.005f);
        }
        // Merge only meshes directly attached to the same joint. Elbows, knees,
        // head and coat tails keep their independent transforms and animation.
        foreach(var joint in GetComponentsInChildren<Transform>()) {
            var attached=new System.Collections.Generic.List<Renderer>();
            foreach(Transform child in joint) {var renderer=child.GetComponent<Renderer>();if(renderer!=null) attached.Add(renderer);}
            if(attached.Count>1) joint.gameObject.AddComponent<StaticVisualBatch>().Build(attached);
        }
    }
    Transform Tail(float side)
    {
        var t=VisualMesh.Joint("Split coat hem",new Vector3(side*.115f,1.08f,-.025f),transform);
        Profile("Flared tailored coat skirt",t,new Vector3(0,-.53f,0),new[]{new Vector2(.145f,0),new Vector2(.139f,.13f),new Vector2(.12f,.40f),new Vector2(.11f,.54f)},coat,1.15f);
        R("Coat pocket welt",t,new Vector3(side*.08f,-.12f,.135f),new Vector3(.1f,.025f,.02f),pbr.Hair,.007f);
        return t;
    }
    Transform Arm(float side,out Transform forearm)
    {
        var upper=VisualMesh.Joint("Shoulder",new Vector3(side*.29f,1.5f,0),transform);
        Profile("Shaped upper sleeve",upper,new Vector3(0,-.32f,0),new[]{new Vector2(.065f,0),new Vector2(.079f,.12f),new Vector2(.09f,.29f),new Vector2(.06f,.34f)},coat,.92f);
        forearm=VisualMesh.Joint("Elbow",new Vector3(0,-.32f,0),upper);
        Profile("Tapered sleeve cuff",forearm,new Vector3(0,-.29f,0),new[]{new Vector2(.048f,0),new Vector2(.058f,.20f),new Vector2(.069f,.30f)},coat,.94f);
        R("Shirt cuff",forearm,new Vector3(0,-.295f,0),new Vector3(.095f,.035f,.10f),pbr.Hair,.012f);
        CharacterCraft.Hand(forearm,new Vector3(0,-.30f,.012f),side,pbr);
        return upper;
    }
    Transform Leg(float side,out Transform shin)
    {
        var thigh=VisualMesh.Joint("Hip",new Vector3(side*.12f,1.02f,0),transform);
        Profile("Tailored trouser thigh",thigh,new Vector3(0,-.44f,0),new[]{new Vector2(.075f,0),new Vector2(.095f,.30f),new Vector2(.09f,.44f)},pbr.Hair,.9f);
        shin=VisualMesh.Joint("Knee",new Vector3(0,-.44f,0),thigh);
        Profile("Tapered trouser shin",shin,new Vector3(0,-.43f,0),new[]{new Vector2(.06f,0),new Vector2(.072f,.2f),new Vector2(.075f,.43f)},pbr.Hair,.92f);
        CharacterCraft.Shoe(shin,new Vector3(0,-.49f,.065f),pbr,true);
        return thigh;
    }
    void Profile(string name,Transform parent,Vector3 p,Vector2[] profile,Material material,float depth)
    {VisualMesh.Lathe(name,p,profile,material,parent,32,depth);}
    Transform R(string name,Transform parent,Vector3 p,Vector3 size,Material m,float radius)
    {return VisualMesh.Rounded(name,p,size,radius,m,parent).transform;}
    public void SetState(bool chase,bool playing){targetAlert=chase?1:0;active=playing;}
    bool HasParameter(string name)
    {foreach(var p in animator.parameters) if(p.name==name&&p.type==AnimatorControllerParameterType.Float) return true;return false;}
    void LateUpdate()
    {
        if(agent==null) return;
        float dt=Time.deltaTime;float target=externalSpeed>=0?(active?externalSpeed:0):active&&agent.isOnNavMesh&&!agent.isStopped?agent.velocity.magnitude:0;
        speed=Mathf.Lerp(speed,target,1-Mathf.Exp(-dt*8));alert=Mathf.Lerp(alert,targetAlert,1-Mathf.Exp(-dt*5));
        if(animator!=null) {
            animator.speed=active?1:0;
            if(hasSpeed) animator.SetFloat("Speed",speed);
            if(hasAlert) animator.SetFloat("Alert",alert);
            if(hasCrouch) animator.SetBool("Crouched",humanCrouched);
            return;
        }
        if(torso==null||!active) return;
        phase+=dt*speed*(Mathf.PI*2)/Mathf.Lerp(1.7f,1.95f,alert);
        float blend=Mathf.Clamp01(speed/.8f),swing=Mathf.Sin(phase)*blend*Mathf.Lerp(22,34,alert);
        torso.localRotation=Quaternion.Euler(alert*5,Mathf.Sin(phase)*blend*3,0);
        torso.localPosition=new Vector3(0,1.03f+Mathf.Cos(phase*2)*.012f*blend+Mathf.Sin(Time.time*1.5f)*.003f,0);
        leftThigh.localRotation=Quaternion.Euler(swing,0,0);rightThigh.localRotation=Quaternion.Euler(-swing,0,0);
        leftShin.localRotation=Quaternion.Euler(Mathf.Max(0,-Mathf.Sin(phase))*blend*46,0,0);
        rightShin.localRotation=Quaternion.Euler(Mathf.Max(0,Mathf.Sin(phase))*blend*46,0,0);
        leftArm.localRotation=Quaternion.Euler(-swing*.7f-alert*12,0,4);rightArm.localRotation=Quaternion.Euler(swing*.7f-alert*12,0,-4);
        leftForearm.localRotation=rightForearm.localRotation=Quaternion.Euler(-8-alert*27,0,0);
        leftTail.localRotation=Quaternion.Euler(swing*.2f,0,2);rightTail.localRotation=Quaternion.Euler(-swing*.2f,0,-2);
        head.localRotation=Quaternion.Euler(-alert*3,Mathf.Sin(Time.time*.8f)*(1-blend)*5,0);
    }
    void OnDestroy(){if(coat!=null) Destroy(coat);}
}

