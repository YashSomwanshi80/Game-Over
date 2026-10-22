using UnityEngine;
using UnityEngine.AI;

// Single hinged room doors and the requested balcony sliders. Moving leaves are never baked into navigation.
public sealed class BedroomDoors : MonoBehaviour
{
    sealed class Door {
        public Transform root;public Transform[] hinges=new Transform[2];public BoxCollider[] leaves=new BoxCollider[2];
        public BedroomDoorState state=new BedroomDoorState();public float side,width;public bool sliding,compact;public float hinge;public AudioSource sound;
    }
    readonly Door[] doors=new Door[HouseDoorLayout.Doors.Length];readonly Collider[] overlaps=new Collider[32];AudioClip click,openingClip,closingClip;
    public int Mask {get{int mask=0;for(int i=0;i<doors.Length;i++)if(doors[i].state.Open)mask|=1<<i;return mask;}}
    public int SwingMask {get{int mask=0;for(int i=0;i<doors.Length;i++)if(doors[i].side>0)mask|=1<<i;return mask;}}
    public float[] Fractions {get{var values=new float[doors.Length];for(int i=0;i<doors.Length;i++)values[i]=doors[i].state.Fraction;return values;}}
    public float West => doors[0].state.Fraction;
    public float East => doors[1].state.Fraction;
    public float Library => doors[2].state.Fraction;
    public float Studio => doors[3].state.Fraction;
    public float Entrance => doors[4].state.Fraction;
    public float GardenRoom => doors[5].state.Fraction;
    public float GardenExit => doors[6].state.Fraction;
    public float WestBalcony => doors[7].state.Fraction;
    public float FrontBalcony => doors[8].state.Fraction;
    public string ActionLabel(int index) => doors[index].state.Open?"CLOSE DOOR":"OPEN DOOR";
    public Vector3 Prompt(int index) => doors[index].root.position+Vector3.up*1.2f;
    public void Build(PbrLibrary pbr)
    {
        click=Resources.Load<AudioClip>("Presentation/Audio/Click");openingClip=Resources.Load<AudioClip>("Presentation/Audio/DoorOpen");closingClip=Resources.Load<AudioClip>("Presentation/Audio/DoorClose");
        for(int i=0;i<doors.Length;i++) {
            var spec=HouseDoorLayout.Doors[i];float width=spec.width,height=spec.height;
            var d=new Door{side=spec.side,width=width,sliding=spec.sliding,compact=spec.compact,hinge=spec.hinge};doors[i]=d;
            d.root=new GameObject(spec.name+(d.sliding?" / sliding glass door":" / single hinged door")).transform;
            d.root.SetParent(transform,false);d.root.localPosition=new Vector3(spec.x,spec.y,spec.z);
            d.root.localRotation=Quaternion.Euler(0,spec.yaw,0);
            d.sound=d.root.gameObject.AddComponent<AudioSource>();d.sound.playOnAwake=false;d.sound.spatialBlend=1;d.sound.minDistance=1;d.sound.maxDistance=8;d.sound.dopplerLevel=0;
            if(d.sliding){BuildSlider(d,height,pbr);continue;}
            foreach(float edge in new[]{-1f,1f}) {
                var jamb=VisualMesh.Rounded("Single door timber jamb",new Vector3(0,height/2,edge*(width/2+.025f)),new Vector3(.17f,height,.045f),.007f,pbr.Walnut,d.root);
                var target=jamb.AddComponent<BoxCollider>();target.size=new Vector3(.17f,height,.045f);
            }
            d.hinges=new Transform[1];d.leaves=new BoxCollider[1];
            for(int leaf=0;leaf<1;leaf++) {
                float sign=d.hinge;float hingeOffset=width/2-.035f,leafWidth=width-.07f,leafHeight=height-.06f;
                var hinge=new GameObject("Door hinge "+leaf).transform;hinge.SetParent(d.root,false);hinge.localPosition=new Vector3(0,0,-sign*hingeOffset);d.hinges[leaf]=hinge;
                var body=VisualMesh.Rounded("Walnut door leaf",new Vector3(0,height/2,sign*leafWidth/2),new Vector3(.065f,leafHeight,leafWidth),.012f,pbr.Walnut,hinge);
                var box=body.AddComponent<BoxCollider>();box.size=new Vector3(.065f,leafHeight,leafWidth);d.leaves[leaf]=box;
                foreach(float face in new[]{-1f,1f}) {
                    foreach(float y in new[]{-height*.23f,height*.23f})VisualMesh.Rounded("Recessed door panel",new Vector3(face*.038f,y,0),new Vector3(.012f,leafHeight*.37f,leafWidth-.18f),.004f,pbr.Walnut,body.transform);
                    float handleZ=sign*(leafWidth/2-.15f);
                    VisualMesh.Rounded("Satin brass handle backplate",new Vector3(face*.039f,-.12f,handleZ),new Vector3(.012f,.18f,.055f),.006f,pbr.Brass,body.transform);
                    VisualMesh.Rounded("Lever spindle",new Vector3(face*.061f,-.09f,handleZ),new Vector3(.04f,.025f,.025f),.006f,pbr.Brass,body.transform);
                    VisualMesh.Rounded("Brass lever handle",new Vector3(face*.086f,-.09f,handleZ-sign*.052f),new Vector3(.025f,.025f,.13f),.008f,pbr.Brass,body.transform);
                }
                foreach(float hingeY in new[]{.24f,height/2,height-.24f})
                    VisualMesh.Lathe("Brass hinge barrel",new Vector3(-.04f,hingeY-.045f,0),new[]{new Vector2(.015f,0),new Vector2(.015f,.09f)},pbr.Brass,hinge,12);
                // Combine only this leaf's decorative meshes. Its collider and moving hinge remain independent.
                hinge.gameObject.AddComponent<StaticVisualBatch>().Build(hinge.GetComponentsInChildren<Renderer>());
            }
        }
    }
    public int Focus(Vector3 origin,Vector3 forward,Collider observer=null)
    {
        RaycastHit hit;if(!InteractionRaycast.First(origin,forward,2.4f,observer,out hit))return -1;
        for(int i=0;i<doors.Length;i++)if(hit.transform.IsChildOf(doors[i].root))return i;
        return -1;
    }
    public int Focus(Camera eye) => Focus(eye.transform.position,eye.transform.forward,eye.GetComponentInParent<CharacterController>());
    public void Toggle(int index){var d=doors[index];d.state.Toggle();PlayDoor(d);}
    public void Toggle(int index,Vector3 actorPosition){var d=doors[index];if(!d.sliding&&!d.state.Open)d.side=DoorSwingRules.AwaySide(d.root.InverseTransformPoint(actorPosition).x,d.state.Fraction,d.side);Toggle(index);}
    void PlayDoor(Door d){var clip=d.state.Open?openingClip:closingClip;if(clip==null)clip=click;if(clip!=null)d.sound.PlayOneShot(clip,.6f);}
    public void ForceOpen(int index,Vector3? actorPosition=null){var d=doors[index];if(!d.sliding&&actorPosition.HasValue)d.side=DoorSwingRules.AwaySide(d.root.InverseTransformPoint(actorPosition.Value).x,d.state.Fraction,d.side);d.state.Apply(true,1);Pose(d);PlayDoor(d);Physics.SyncTransforms();}
    Quaternion Rotation(Door d,int leaf,float fraction) => d.sliding?Quaternion.identity:Quaternion.Euler(0,d.side*d.hinge*88*fraction,0);
    bool Clear(Door d,float fraction)
    {
        for(int leaf=0;leaf<d.leaves.Length;leaf++) {
            var box=d.leaves[leaf];Quaternion rotation=d.root.rotation*Rotation(d,leaf,fraction);
            Vector3 center=d.sliding?d.root.TransformPoint(Slide(d,leaf,fraction)+box.transform.localPosition):d.hinges[leaf].position+rotation*box.transform.localPosition;
            int count=Physics.OverlapBoxNonAlloc(center,box.size*.5f,overlaps,rotation,~(1<<8),QueryTriggerInteraction.Ignore);
            if(count==overlaps.Length)return false;
            for(int n=0;n<count;n++)if(overlaps[n]!=null&&!overlaps[n].transform.IsChildOf(transform)) {
                if(!d.sliding||overlaps[n].GetComponentInParent<CharacterController>()!=null||overlaps[n].GetComponentInParent<NavMeshAgent>()!=null)return false;
            }
        }
        return true;
    }
    Vector3 Slide(Door d,int leaf,float fraction)
    {
        if(d.compact) {
            float z=-.975f+leaf*.65f;
            if(leaf==1)z-=.65f*fraction;if(leaf==2)z+=.65f*fraction;
            return new Vector3(leaf%2==0?-.035f:.035f,0,z);
        }
        return new Vector3(leaf==0?-.035f:.035f,0,(leaf==0?-1:1)*(d.width/4+(d.width/2+.02f)*fraction));
    }
    void Pose(Door d){for(int i=0;i<d.leaves.Length;i++)if(d.sliding)d.hinges[i].localPosition=Slide(d,i,d.state.Fraction);else d.hinges[i].localRotation=Rotation(d,i,d.state.Fraction);}
    void BuildSlider(Door d,float height,PbrLibrary pbr)
    {
        var glass=pbr.ResourceMaterial("Presentation/ArchitecturalGlass","Balcony sliding safety glass");
        glass.color=new Color(.16f,.25f,.28f,.13f);
        float panel=d.compact?.63f:d.width/2-.02f;
        if(d.compact){d.hinges=new Transform[4];d.leaves=new BoxCollider[4];}
        for(int leaf=0;leaf<d.leaves.Length;leaf++) {
            var carrier=new GameObject("Sliding glass panel "+leaf).transform;carrier.SetParent(d.root,false);d.hinges[leaf]=carrier;
            var body=VisualMesh.Rounded("Clear balcony safety glass",new Vector3(0,height/2,0),new Vector3(.025f,height-.12f,panel-.08f),.004f,glass,carrier);
            var box=body.AddComponent<BoxCollider>();box.size=new Vector3(.05f,height-.06f,panel);d.leaves[leaf]=box;
            foreach(float side in new[]{-1f,1f}) {
                VisualMesh.Rounded("Sliding frame upright",new Vector3(0,height/2,side*(panel/2-.025f)),new Vector3(.06f,height,.05f),.006f,pbr.Metal,carrier);
                VisualMesh.Rounded("Sliding frame rail",new Vector3(0,side<0?.035f:height-.035f,0),new Vector3(.06f,.07f,panel),.006f,pbr.Metal,carrier);
                VisualMesh.Rounded("Sliding door pull",new Vector3(side*.07f,1.1f,(leaf==0?1:-1)*(panel/2-.14f)),new Vector3(.035f,.32f,.035f),.008f,pbr.Brass,carrier);
            }
            carrier.gameObject.AddComponent<StaticVisualBatch>().Build(carrier.GetComponentsInChildren<Renderer>());
        }
        // The jamb control stays reachable when panels have parked beside the opening.
        var control=VisualMesh.Rounded("Balcony slide open-close control",new Vector3(.23f,1.15f,d.width/2-.10f),new Vector3(.10f,.20f,.10f),.01f,pbr.Brass,d.root);
        control.AddComponent<BoxCollider>();
        VisualMesh.Rounded("Balcony upper sliding track",new Vector3(0,height+.025f,0),new Vector3(.14f,.05f,(d.compact?d.width:d.width*2)),.005f,pbr.Metal,d.root);
        Pose(d);
    }
    public void Tick(float dt,bool checkCollision=true)
    {
        Physics.SyncTransforms();
        float remaining=Mathf.Clamp(dt,0,.1f);
        while(remaining>0) {
            float step=Mathf.Min(remaining,.016f);remaining-=step;
            foreach(var d in doors){float next=d.state.Next(step);d.state.Advance(step,!checkCollision||Mathf.Approximately(next,d.state.Fraction)||Clear(d,next));Pose(d);}
        }
        Physics.SyncTransforms();
    }
    public void ApplySnapshot(int mask,float west,float east,float library,float studio,float entrance,float gardenRoom,float gardenExit,float westBalcony,float frontBalcony,float[] roomDoors=null,int swingMask=-1)
    {
        int priorMask=Mask;
        if(swingMask>=0)for(int i=0;i<doors.Length;i++)if(!doors[i].sliding)doors[i].side=(swingMask&(1<<i))!=0?1:-1;
        doors[0].state.Apply((mask&1)!=0,west);doors[1].state.Apply((mask&2)!=0,east);
        doors[2].state.Apply((mask&4)!=0,library);doors[3].state.Apply((mask&8)!=0,studio);
        doors[4].state.Apply((mask&16)!=0,entrance);doors[5].state.Apply((mask&32)!=0,gardenRoom);doors[6].state.Apply((mask&64)!=0,gardenExit);
        doors[7].state.Apply((mask&128)!=0,westBalcony);doors[8].state.Apply((mask&256)!=0,frontBalcony);
        if(roomDoors!=null&&roomDoors.Length==doors.Length)for(int i=0;i<doors.Length;i++)doors[i].state.Apply((mask&(1<<i))!=0,roomDoors[i]);
        for(int i=0;i<doors.Length;i++)if(((priorMask^mask)&(1<<i))!=0)PlayDoor(doors[i]);
        foreach(var d in doors)Pose(d);Physics.SyncTransforms();
    }
    public void OpenForAgent(NavMeshAgent agent)
    {
        if(agent==null||!agent.enabled||!agent.isOnNavMesh||!agent.hasPath)return;
        foreach(var d in doors) {
            Vector3 delta=agent.transform.position-d.root.position;
            if(Mathf.Abs(delta.y)>1.1f||new Vector2(delta.x,delta.z).magnitude>2.4f)continue;
            bool crosses=false;Vector3 previous=d.root.InverseTransformPoint(agent.transform.position);
            foreach(var worldCorner in agent.path.corners) {
                Vector3 corner=d.root.InverseTransformPoint(worldCorner);
                float dx=corner.x-previous.x;
                if(Mathf.Abs(dx)>.0001f) {
                    float t=-previous.x/dx;
                    Vector3 crossing=Vector3.LerpUnclamped(previous,corner,t);
                    if(t>=0&&t<=1&&Mathf.Abs(crossing.z)<=d.width/2+.1f&&Mathf.Abs(crossing.y)<1.1f){crosses=true;break;}
                }
                previous=corner;
            }
            if(!crosses)continue;
            if(!d.state.Open){if(!d.sliding)d.side=DoorSwingRules.AwaySide(d.root.InverseTransformPoint(agent.transform.position).x,d.state.Fraction,d.side);d.state.SetOpen(true);PlayDoor(d);}
            if(new Vector2(delta.x,delta.z).magnitude<1.4f&&d.state.Fraction<.95f)agent.isStopped=true;
        }
    }
}
