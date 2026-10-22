using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameOver : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetLaunchRoute(){SessionFlow.SetNext(FrontEndAction.MainMenu);}
    CharacterController player; Camera eye; NavMeshAgent don; NavMeshData nav;
    FirstPersonMotor motor;
    readonly List<HidingLayout.Spot> hidingSpots=HidingLayout.Spots();
    HidingLayout.Spot activeHide;
    bool sprintHeld,jumpPressed,crouchPressed;
    Camera securityCamera; RenderTexture securityFeed;
    AudioSource donSteps,playerSteps; AudioClip footstep;
    SurfaceFootsteps surfaceSteps;
    float nextStep, nextPlayerStep, nextCameraFrame;
    readonly SecurityState security = new SecurityState();
    PbrLibrary pbr;
    readonly TacticalHud hud=new TacticalHud();
    readonly MainMenu mainMenu=new MainMenu();
    readonly QuickGuide guide=new QuickGuide();
    bool introRequested;
    FrontEndAction pendingScene;
    readonly List<GameObject> geometry=new List<GameObject>();
    MansionVisuals visuals;
    BedroomDoors bedroomDoors;MansionLighting bedroomLights;PlayerFlashlights flashlights;
    ThrillerSoundscape soundscape;
    MenuStorm menuStorm;
    EstateMenuCamera estateMenuCamera;
    RivalMatch rival;
    readonly Vector3 fuseBox = MainPowerLayout.Panel;
    float nextSabotage=35;
    bool SignalLost => !security.Powered;
    Vector3 start = new Vector3(-7, 0.1f, -1), lastSeen;
    readonly Vector3 phone = new Vector3(7, 1, 4);
    readonly Vector3[] patrol = { new Vector3(7,0,2.8f),new Vector3(0,0,-11),new Vector3(-13,.05f,-2),new Vector3(6,0,9), new Vector3(13,0,-3), new Vector3(7,0,-5), new Vector3(-7,0,-5), new Vector3(-7,0,4), new Vector3(0,3.4f,6.1f), new Vector3(-6,3.4f,3), new Vector3(5,3.4f,3), new Vector3(-7,-3.4f,3.5f), new Vector3(7,-3.4f,-3) };
    float yaw, pitch, suspicion, searchUntil, nextPatrol, elapsed;
    readonly RivalRules objective=new RivalRules();
    DonDifficultyProfile difficulty=DonDifficultyRules.For(DonDifficulty.Normal);
    float callProgress=>objective.Progress;
    float rescue=>objective.Rescue;
    bool called=>objective.Called;
    bool won=>objective.Outcome==1;
    bool lost=>objective.Outcome==2;
    int waypoint; bool hidden, started, paused, cameras;
    string state = "Patrol"; Vector3 hideReturn; Vector2 movement, look;
    float hideReturnYaw, hideReturnPitch;
    string notice=""; float noticeUntil;
    readonly Vector3 phonePrompt=new Vector3(7,1.2f,4);
    readonly Vector3 powerPrompt=MainPowerLayout.Prompt;
    readonly Vector3 entrancePrompt=new Vector3(.65f,1.2f,-6.82f);
    readonly ComicPresentation story=new ComicPresentation();
    IntroVideo introVideo;
    BasementPlay basementPlay;
    readonly MobileTouchInput touchInput=new MobileTouchInput();
    Material wall, floor, wood, black;
    readonly List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();

    void Start()
    {
        Lock(false);
        GamePreferences.Apply();
        pbr=gameObject.AddComponent<PbrLibrary>();pbr.Build();
        wall=pbr.Plaster;floor=pbr.Floor;wood=pbr.Walnut;black=pbr.Metal;
        Box("Ground floor west",new Vector3(-4.3f,-.2f,0),new Vector3(11.4f,.4f,14),floor);
        Box("Ground floor east",new Vector3(6.9f,-.2f,0),new Vector3(6.2f,.4f,14),floor);
        Box("Ground floor entrance approach",new Vector3(2.6f,-.2f,-2),new Vector3(2.4f,.4f,10),floor);
        Box("Ground floor north edge",new Vector3(2.6f,-.2f,6.85f),new Vector3(2.4f,.4f,.3f),floor);
        foreach(var part in BasementLayout.Create()) {
            Material material=part.material=="wood"?wood:part.material=="floor"?floor:part.material=="screen"?black:part.material=="fabric"?pbr.Fabric:part.material=="linen"?pbr.Linen:wall;
            Box(part.name,new Vector3(part.x,part.y,part.z),new Vector3(part.w,part.h,part.d),material);
        }
        Box("North wall west",new Vector3(-7.775f,1.5f,7),new Vector3(4.45f,3,.25f),wall);
        Box("North wall middle",new Vector3(1.25f,1.5f,7),new Vector3(9.4f,3,.25f),wall);
        Box("North wall east",new Vector3(9.025f,1.5f,7),new Vector3(1.95f,3,.25f),wall);
        Box("North window west sill wall",new Vector3(-4.5f,.5375f,7),new Vector3(2.1f,1.075f,.25f),wall);
        Box("North window west header wall",new Vector3(-4.5f,2.7625f,7),new Vector3(2.1f,.475f,.25f),wall);
        Box("North window east sill wall",new Vector3(7,.5375f,7),new Vector3(2.1f,1.075f,.25f),wall);
        Box("North window east header wall",new Vector3(7,2.7625f,7),new Vector3(2.1f,.475f,.25f),wall);
        Box("North window west glass collision",new Vector3(-4.5f,1.8f,7),new Vector3(2.1f,1.45f,.05f),pbr.Glass);
        Box("North window east glass collision",new Vector3(7,1.8f,7),new Vector3(2.1f,1.45f,.05f),pbr.Glass);
        Box("South wall west corner",new Vector3(-7.4f,1.5f,-7),new Vector3(5.2f,3,.25f),wall);
        Box("South wall west entrance",new Vector3(-1.75f,1.5f,-7),new Vector3(1.3f,3,.25f),wall);
        Box("South wall east entrance",new Vector3(2.95f,1.5f,-7),new Vector3(3.7f,3,.25f),wall);
        Box("South wall east corner",new Vector3(8.6f,1.5f,-7),new Vector3(2.8f,3,.25f),wall);
        foreach(float x in new[]{-3.6f,6f}) {
            Box("Front window sill wall",new Vector3(x,.45f,-7),new Vector3(2.4f,.9f,.25f),wall);
            Box("Front window header wall",new Vector3(x,2.7f,-7),new Vector3(2.4f,.6f,.25f),wall);
            Box("Front window glass collision",new Vector3(x,1.65f,-7),new Vector3(2.4f,1.5f,.025f),pbr.Glass);
        }
        Box("Main entrance lintel",new Vector3(0,2.75f,-7),new Vector3(2.2f,.5f,.25f),wall);
        // The working entrance leaves are built after the static NavMesh.
        Box("West wall south",new Vector3(-10,1.5f,-4.6f),new Vector3(.25f,3,4.8f),wall);
        Box("West wall north",new Vector3(-10,1.5f,3.6f),new Vector3(.25f,3,6.8f),wall);
        Box("Garden room doorway lintel",new Vector3(-10,2.75f,-1),new Vector3(.25f,.5f,2.4f),wall);
        // A two-metre doorway connects the attached garage to the living room.
        Box("East wall north",new Vector3(10,1.5f,4),new Vector3(.25f,3,6),wall);
        Box("East wall south",new Vector3(10,1.5f,-4),new Vector3(.25f,3,6),wall);
        Box("Garage doorway lintel",new Vector3(10,2.7f,0),new Vector3(.25f,.6f,2),wall);
        Box("Garage floor",new Vector3(13,-.2f,0),new Vector3(6,.4f,14),floor);
        Box("Garage outer wall",new Vector3(16,1.5f,0),new Vector3(.25f,3,14),wall);
        Box("Garage north wall",new Vector3(13,1.5f,7),new Vector3(6,3,.25f),wall);
        Box("Garage shutter",new Vector3(13,1.5f,-7),new Vector3(6,3,.25f),black);
        Box("Parked car collision",new Vector3(13.45f,.72f,.5f),new Vector3(2.4f,1.44f,4.7f),black);
        // Store room occupies the south-west corner, with an open doorway into the kitchen.
        Box("Store east partition",new Vector3(-6,1.5f,-5.1f),new Vector3(.2f,3,3.8f),wall);
        Box("Store north partition",new Vector3(-8.8f,1.5f,-3.2f),new Vector3(2.4f,3,.2f),wall);
        Box("Store doorway lintel",new Vector3(-6.8f,2.7f,-3.2f),new Vector3(1.6f,.6f,.2f),wall);
        Box("Store shelving",new Vector3(-9.4f,1,-5.2f),new Vector3(.8f,2,2.8f),wood);
        // Wide doorway connects kitchen/dining and living-room halves.
        BuildStairsAndWashroom();
        Box("Kitchen counter",new Vector3(-8.8f,.5f,3.5f),new Vector3(1.5f,1,5),wood);
        Box("Dining table",new Vector3(-4,.39f,0),new Vector3(1.24f,.78f,2.01f),wood);
        Box("Living sofa",new Vector3(7,.72f,-3),new Vector3(3.52f,1.44f,1.20f),wood);
        Box("Coffee table",new Vector3(6,.325f,0),new Vector3(2,.65f,1),wood);
        Box("Lounge chair east collision",new Vector3(8.80000f,0.52839f,-1.40000f),new Vector3(1.02000f,1.05678f,0.85152f),wood);
        Box("Lounge chair west collision",new Vector3(4.25000f,0.52839f,-1.10000f),new Vector3(1.02000f,1.05678f,0.85152f),wood);
        Box("Living room planter collision",new Vector3(9.20000f,0.70431f,5.90000f),new Vector3(0.62068f,1.40861f,0.67000f),wood);
        Box("Telephone desk",new Vector3(7,.45f,4),new Vector3(2,.9f,1),wood);
        Box("Police telephone",phone,new Vector3(.5f,.2f,.35f),Mat(new Color(.2f,.8f,.65f)));
        // Kitchen cupboard uses the shared wall-aligned hiding layout.
        foreach(var part in BasementPlayLayout.Parts())Box(part.name,new Vector3(part.x,part.y,part.z),new Vector3(part.w,part.h,part.d),HidingDecor.Finish(pbr,part.material));
        foreach(var part in BasementPlayLayout.Exclusions())sources.Add(new NavMeshBuildSource {shape=NavMeshBuildSourceShape.Box,transform=Matrix4x4.TRS(new Vector3(part.x,part.y,part.z),Quaternion.identity,Vector3.one),size=new Vector3(part.w,part.h,part.d),area=1});
        foreach(var part in HidingLayout.Create())
            Box(part.name,new Vector3(part.x,part.y,part.z),new Vector3(part.w,part.h,part.d),HidingDecor.Finish(pbr,part.material));
        foreach(var part in HidingLayout.NavigationBlockers())
            sources.Add(new NavMeshBuildSource {shape=NavMeshBuildSourceShape.Box,
                transform=Matrix4x4.TRS(new Vector3(part.x,part.y,part.z),Quaternion.identity,Vector3.one),size=new Vector3(part.w,part.h,part.d),area=1});
        // Extra collision above the balcony glass prevents jumping into the non-playable garden.
        foreach(var part in FirstFloorLayout.Create())if(part.name.Contains("balcony")&&part.name.Contains("railing"))
            Box("Balcony safety barrier "+part.name,new Vector3(part.x,part.y+part.h/2+1.25f,part.z),new Vector3(part.w,2.5f,part.d),black);
        Box("Security fuse box",fuseBox,new Vector3(.8f,1,.3f),pbr.Metal);
        Box("Passage motion sensor",new Vector3(1.15f,2.45f,.85f),new Vector3(.10f,.20f,.22f),pbr.Linen);
        foreach(var part in EstateRenovation.Plan.parts)if(part.collision) {
            Box("Estate / "+part.name,EstateRenovation.V(part.p),EstateRenovation.V(part.s),EstateRenovation.Finish(pbr,part.material));
            geometry[geometry.Count-1].layer=part.layer;
        }
        // The front slider's two outer panels stay in place. Route AI through the clear center.
        foreach(float x in new[]{-.975f,.975f})
            sources.Add(new NavMeshBuildSource {shape=NavMeshBuildSourceShape.Box,
                transform=Matrix4x4.TRS(new Vector3(x,4.6f,-7.23f),Quaternion.identity,Vector3.one),
                size=new Vector3(.63f,2.4f,.14f),area=1});
        foreach(var part in HouseDoorLayout.FixedParts())
            Box(part.name,new Vector3(part.x,part.y,part.z),new Vector3(part.w,part.h,part.d),wall);
        var settings = NavMesh.GetSettingsByIndex(0); settings.agentRadius = .35f;
        settings.agentClimb=.3f; settings.agentSlope=45; settings.overrideVoxelSize=true; settings.voxelSize=.06f;
        nav = NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(new Vector3(2,1.5f,0),new Vector3(52,13,40)),Vector3.zero,Quaternion.identity);
        if(nav != null) NavMesh.AddNavMeshData(nav);
        var p = new GameObject("Tapsee / first-person controller"); p.transform.position = start;
        player = p.AddComponent<CharacterController>(); player.height = 1.7f; player.radius = .25f; player.center = new Vector3(0,.85f,0);
        motor=p.AddComponent<FirstPersonMotor>();motor.Configure(player);
        eye = new GameObject("First person camera").AddComponent<Camera>(); eye.tag="MainCamera"; eye.transform.SetParent(p.transform); eye.transform.localPosition=new Vector3(0,1.55f,0); eye.nearClipPlane=.06f; eye.fieldOfView=72; eye.gameObject.AddComponent<AudioListener>();
        var d = GameObject.CreatePrimitive(PrimitiveType.Capsule); d.name="Don - navigation root"; d.transform.position=new Vector3(6,1,3); d.GetComponent<Renderer>().sharedMaterial=black;
        Destroy(d.GetComponent<Collider>()); don=d.AddComponent<NavMeshAgent>(); don.height=2; don.radius=.35f; don.baseOffset=1; don.speed=1.65f; don.angularSpeed=220;
        don.Warp(new Vector3(6,0,3)); lastSeen=start;
        visuals=new GameObject("Mansion visual pass").AddComponent<MansionVisuals>(); visuals.Build(don,eye,geometry,pbr);
        bedroomDoors=new GameObject("Existing mansion / bedroom doors").AddComponent<BedroomDoors>();bedroomDoors.Build(pbr);bedroomLights=visuals.GetComponent<MansionLighting>();
        flashlights=gameObject.AddComponent<PlayerFlashlights>();flashlights.Build(eye,pbr);
        basementPlay=gameObject.AddComponent<BasementPlay>();basementPlay.Build(pbr);
        CreateSurveillance();
        CreateFootsteps();
        surfaceSteps=gameObject.AddComponent<SurfaceFootsteps>();surfaceSteps.Build();
        soundscape=gameObject.AddComponent<ThrillerSoundscape>();soundscape.Build();
        menuStorm=gameObject.AddComponent<MenuStorm>();menuStorm.Build();menuStorm.SetVisible(true);
        estateMenuCamera=gameObject.AddComponent<EstateMenuCamera>();estateMenuCamera.Configure(eye);
        if(SessionFlow.ConsumeQuickRetry())BeginGameplay();
    }
    void BeginGameplay()
    {
        difficulty=DonDifficultyRules.For(GamePreferences.Difficulty);
        if(estateMenuCamera!=null)estateMenuCamera.EnterGame();
        if(menuStorm!=null)menuStorm.SetVisible(false);
        introRequested=true;started=true;paused=false;flashlights.SetActive(true);don.SetDestination(patrol[0]);Lock(true);
        guide.Begin(player.transform.position,eye.transform.forward);
        Notify("Find the living-room telephone. Look at it and hold the call action.");
        if(MatchOptions.Mode!=MatchMode.SinglePlayer||MatchOptions.Role==CharacterRole.Don) {
            rival=gameObject.AddComponent<RivalMatch>();rival.Build(player,don,eye,visuals,hud,securityFeed,footstep);
            rival.Exit=()=>RequestScene(FrontEndAction.MainMenu);rival.Retry=()=>RequestScene(FrontEndAction.Retry);
        }
    }
    void RequestScene(FrontEndAction action)
    {
        if(action==FrontEndAction.Retry||action==FrontEndAction.MainMenu)pendingScene=action;
    }
    void BuildStairsAndWashroom()
    {
        // Seventeen 20 cm risers reach the 3.4 m first-floor landing.
        for(int i=0;i<17;i++) {
            float height=(i+1)*.2f;
            Box("Central stair "+i,new Vector3(0,height/2,.65f+i*.3f),new Vector3(2.4f,height,.3f),wood);
        }
        Box("Stairwell west wall",new Vector3(-1.3f,3.3f,3.05f),new Vector3(.2f,6.6f,5.1f),wall);
        Box("Stairwell east wall",new Vector3(1.3f,3.3f,3.05f),new Vector3(.2f,6.6f,5.1f),wall);
        Box("First floor landing",new Vector3(0,3.3f,6.2f),new Vector3(4.6f,.2f,1.2f),floor);
        Box("Landing back wall",new Vector3(0,4.8f,6.9f),new Vector3(4.8f,2.8f,.2f),wall);
        var linen=pbr.Linen;
        var upholstery=pbr.Fabric;
        foreach(var part in FirstFloorLayout.Create()) {
            Material material=part.material=="wood"?wood:part.material=="floor"?floor:part.material=="linen"?linen:part.material=="fabric"?upholstery:wall;
            Box(part.name,new Vector3(part.x,part.y,part.z),new Vector3(part.w,part.h,part.d),material);
        }
        Box("Landing west guard",new Vector3(-1.85f,3.95f,5.6f),new Vector3(.9f,1.1f,.15f),wood);
        // Studio approach is continuous floor: no projecting landing guard here.
        Box("Stairwell roof",new Vector3(0,6.3f,3.7f),new Vector3(4.8f,.2f,6.4f),wall);
        Box("Washroom east wall",new Vector3(3.8f,1.5f,5),new Vector3(.2f,3,4),wall);
        Box("Washroom rear west wall",new Vector3(1.3f,1.5f,6.3f),new Vector3(.2f,3,1.4f),wall);
        Box("Basement entrance right wall",new Vector3(3.2f,1.5f,3),new Vector3(1.2f,3,.2f),wall);
        Box("Basement entrance lintel",new Vector3(2,2.7f,3),new Vector3(1.2f,.6f,.2f),wall);
    }
    void CreateSurveillance()
    {
        securityFeed=new RenderTexture(640,360,16) { name="Living room CCTV" };
        securityFeed.Create();
        securityCamera=new GameObject("Living room security camera").AddComponent<Camera>();
        securityCamera.transform.position=new Vector3(9.5f,2.75f,6.5f);
        securityCamera.transform.LookAt(new Vector3(4.5f,.8f,-1));
        securityCamera.fieldOfView=85; securityCamera.nearClipPlane=.1f; securityCamera.farClipPlane=22;
        securityCamera.targetTexture=securityFeed; securityCamera.enabled=false;
        securityCamera.cullingMask=~(1<<8);
        securityCamera.clearFlags=CameraClearFlags.SolidColor; securityCamera.backgroundColor=Color.black;
        securityCamera.gameObject.AddComponent<ThrillerRendering>().Configure(securityCamera,false);
        gameObject.AddComponent<HouseCctv>().Build(securityCamera);
    }
    void CreateFootsteps()
    {
        // Short synthesized shoe thud; no downloaded audio assets required.
        const int rate=22050;
        var samples=new float[4410]; var noise=new System.Random(41);
        for(int i=0;i<samples.Length;i++) {
            float t=(float)i/rate;
            samples[i]=(Mathf.Sin(2*Mathf.PI*85*t)*.65f+(float)(noise.NextDouble()*2-1)*.35f)*Mathf.Exp(-t*35)*Mathf.Min(t*400,1);
        }
        footstep=AudioClip.Create("Don shoe step",samples.Length,1,rate,false); footstep.SetData(samples,0);
        donSteps=don.gameObject.AddComponent<AudioSource>(); donSteps.playOnAwake=false;
        donSteps.spatialBlend=1; donSteps.rolloffMode=AudioRolloffMode.Linear;
        donSteps.minDistance=1; donSteps.maxDistance=12; donSteps.dopplerLevel=0;
        playerSteps=player.gameObject.AddComponent<AudioSource>();playerSteps.playOnAwake=false;playerSteps.spatialBlend=0;playerSteps.volume=.16f;
    }
    void UpdateSurveillanceAndSound()
    {
        if(cameras&&!SignalLost&&elapsed>=nextCameraFrame) {
            nextCameraFrame=elapsed+.125f; HouseCctv.ApplySelected(securityCamera);securityCamera.Render();
        }
        if(don.velocity.sqrMagnitude>.15f&&elapsed>=nextStep) {
            nextStep=elapsed+(state=="Chase"?.33f:.55f);
            Vector3 source=don.transform.position+Vector3.up*.65f;
            RaycastHit hit;
            bool muffled=Physics.Linecast(source,eye.transform.position,out hit)&&hit.collider!=player;
            donSteps.pitch=state=="Chase"?1.1f:1;
            surfaceSteps.Play(donSteps,don.transform.position,muffled?.22f:.65f,true);
        }
    }
    void UpdatePlayerNoise()
    {
        if(hidden)return;
        if(motor.State.Jumped){playerSteps.volume=.16f;surfaceSteps.Play(playerSteps,player.transform.position,.35f);HearPlayer(5*surfaceSteps.HearingScale(player.transform.position));}
        if(motor.Landed){playerSteps.volume=.22f;surfaceSteps.Play(playerSteps,player.transform.position,.9f);nextPlayerStep=elapsed+.25f;HearPlayer(6*surfaceSteps.HearingScale(player.transform.position));}
        if(!cameras&&motor.Grounded&&motor.HorizontalSpeed>.3f&&movement.sqrMagnitude>.1f&&elapsed>=nextPlayerStep) {
            nextPlayerStep=elapsed+(motor.State.Sprinting?.32f:motor.State.Crouched?.72f:.53f);
            playerSteps.volume=motor.State.Sprinting?.26f:motor.State.Crouched?.055f:.16f;
            playerSteps.pitch=motor.State.Sprinting?1.05f:.9f;surfaceSteps.Play(playerSteps,player.transform.position);
            HearPlayer((motor.State.Sprinting?8:motor.State.Crouched?1.4f:3.5f)*surfaceSteps.HearingScale(player.transform.position));
        }
    }
    void HearPlayer(float range)
    {
        range*=difficulty.Hearing;
        if(hidden||state=="Chase")return;
        NavMeshHit floor;
        if(!NavMesh.SamplePosition(don.transform.position,out floor,2,NavMesh.AllAreas)||Mathf.Abs(floor.position.y-motor.LastGroundedPosition.y)>1.7f)return;
        Vector3 source=motor.LastGroundedPosition+Vector3.up*.7f;
        if(!InteractionRaycast.Clear(source,don.transform.position+Vector3.up*.65f,player,don.GetComponent<Collider>()))range*=.45f;
        if(Vector3.Distance(source,don.transform.position+Vector3.up*.65f)>range)return;
        lastSeen=motor.LastGroundedPosition;searchUntil=elapsed+difficulty.Memory;state="Investigate";
    }
    void HearDistraction(Vector3 point,float range)
    {
        if(state=="Chase")return;
        NavMeshHit floor;if(!NavMesh.SamplePosition(don.transform.position,out floor,2,NavMesh.AllAreas)||Mathf.Abs(floor.position.y+1-point.y)>1.6f)return;
        Vector3 ear=don.transform.position+Vector3.up*.65f;
        if(!InteractionRaycast.Clear(point,ear,don.GetComponent<Collider>()))range*=.55f;
        if(Vector3.Distance(point,ear)>range*difficulty.Hearing)return;
        NavMeshHit heard;if(!NavMesh.SamplePosition(new Vector3(point.x,-3.4f,point.z-1),out heard,1,NavMesh.AllAreas))return;
        lastSeen=heard.position;searchUntil=elapsed+difficulty.Memory;state="Investigate";
    }
    bool CanCatchPlayer()
    {
        // The solo agent origin is one metre above its feet; rival controllers use feet origins.
        Vector3 feet=don.transform.position-Vector3.up*don.baseOffset;
        return CaptureFacing.InReach(feet,don.transform.forward,player.transform.position)&&CanSee();
    }
    Material Mat(Color c) { return pbr.Plain(c); }
    void Box(string name,Vector3 pos,Vector3 scale,Material mat)
    {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cube); o.name=name; o.transform.position=pos; o.transform.localScale=scale; o.GetComponent<Renderer>().sharedMaterial=mat; geometry.Add(o);
        sources.Add(new NavMeshBuildSource { shape=NavMeshBuildSourceShape.Box, transform=Matrix4x4.TRS(pos,Quaternion.identity,Vector3.one), size=scale, area=0 });
    }
    void Update()
    {
        if(pendingScene!=FrontEndAction.None) {
            var destination=pendingScene;pendingScene=FrontEndAction.None;
            GamePreferences.Save();SessionFlow.SetNext(destination);Lock(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);return;
        }
        if(rival!=null){rival.Tick();return;}
        if(visuals!=null) visuals.SetState(suspicion,state=="Chase",started&&!paused&&!won&&!lost,hidden||cameras?0:motor.HorizontalSpeed);
        if(Input.GetKeyDown(KeyCode.Escape)&&started&&!won&&!lost) { paused=!paused;if(!paused)GamePreferences.Save();Lock(!paused); }
        if(don != null && don.isOnNavMesh) don.isStopped=!started||paused||won||lost;
        if(!started||paused||won||lost) { if(donSteps!=null&&donSteps.isPlaying) donSteps.Stop(); return; }
        elapsed+=Time.deltaTime;
        security.Tick(Time.deltaTime,Mathf.Abs(don.transform.position.y)<2&&Mathf.Abs(don.transform.position.x)<1.25f&&Mathf.Abs(don.transform.position.z)<2.5f);
        visuals.SetPower(security.Powered);
        ReadInput();
        if(paused) {if(don.isOnNavMesh) don.isStopped=true;return;}
        if(!cameras) {
            yaw+=look.x; pitch=Mathf.Clamp(pitch-look.y,hidden&&activeHide.LowView?(activeHide.tub?-45:-20):-75,hidden&&activeHide.LowView?(activeHide.tub?45:20):75);
            if(hidden&&activeHide.LowView)yaw=activeHide.yaw+Mathf.Clamp(Mathf.DeltaAngle(activeHide.yaw,yaw),-65,65); player.transform.rotation=Quaternion.Euler(0,yaw,0); eye.transform.localRotation=Quaternion.Euler(pitch,0,0);
        }
        if(Input.GetKeyDown(KeyCode.C))cameras=!cameras;
        if(hidden)motor.Suspend(Time.deltaTime);
        else motor.Tick(movement,sprintHeld&&!cameras,jumpPressed&&!cameras,crouchPressed&&!cameras,!cameras,Time.deltaTime);
        if(motor.State.StandBlocked)Notify("Not enough headroom to stand here.");
        if(Input.GetKeyDown(KeyCode.E))Interact();
        flashlights.Step(Time.deltaTime);
        UpdatePlayerNoise();
        bedroomDoors.Tick(Time.deltaTime);
        bool visible = !hidden && CanSee();
        suspicion=DonDetection.Advance(suspicion,visible,motor.State.Crouched,Time.deltaTime,difficulty.Response);
        if(visible) {
            lastSeen=player.transform.position;searchUntil=elapsed+difficulty.Memory;
            if(DonDetection.Pursue(suspicion,state=="Chase"))state="Chase";
            else if(suspicion>=DonDetection.InvestigateThreshold)state="Investigate";
        }
        else if(state=="Chase") state="Investigate";
        if(state=="Patrol"&&security.Powered&&elapsed>=nextSabotage)state="Sabotage";
        if(state=="Sabotage") {
            don.speed=1.65f;don.SetDestination(MainPowerLayout.Approach);
            Vector3 offset=don.transform.position-MainPowerLayout.Approach;offset.y=0;
            if(offset.magnitude<.65f&&Mathf.Abs(don.transform.position.y-fuseBox.y)<1.5f) {
                don.transform.rotation=Quaternion.Euler(0,180,0);security.SwitchOff();visuals.SetPower(false);
                nextSabotage=elapsed+45;state="Patrol";don.SetDestination(patrol[waypoint]);
                Notify("Don switched off the main power. The breaker is in the basement utility room.");
            }
        } else if(state=="Chase"||state=="Investigate") {
            don.speed=state=="Chase"?2.9f:1.9f; don.SetDestination(lastSeen);
            if(!visible&&Vector3.Distance(don.transform.position,lastSeen)<1.8f) state="Search";
        } else if(state=="Search") {
            don.speed=1.2f; if(elapsed>nextPatrol) {nextPatrol=elapsed+difficulty.SearchInterval;ChooseSearchPoint();}
        } else { don.speed=1.65f; if(!don.pathPending&&don.remainingDistance<.5f) { waypoint=(waypoint+1)%patrol.Length; don.SetDestination(patrol[waypoint]); } }
        if(!visible&&elapsed>searchUntil&&state!="Patrol"&&state!="Sabotage") { state="Patrol"; don.SetDestination(patrol[waypoint]); }
        bedroomDoors.OpenForAgent(don);
        AdvanceOutcome(!hidden&&visible&&CanCatchPlayer(),Time.deltaTime);
        if(!lost&&!won) UpdatePoliceCall();
        if(!won&&!lost) UpdateSurveillanceAndSound();
    }
    void Notify(string message) { notice=message; noticeUntil=elapsed+4; }
    void LateUpdate()
    {
        if(rival==null&&basementPlay!=null)basementPlay.Present(elapsed,started&&!paused&&!won&&!lost);
        if(menuStorm!=null)menuStorm.SetVisible(!started&&!introRequested);
        if(rival!=null) {
            if(soundscape!=null)soundscape.Tick(new ThrillerSoundscape.State{started=true,paused=!rival.Running&&!rival.Finished,won=rival.Rescued,lost=rival.Captured,called=rival.Called,hidden=rival.Hidden,cameras=rival.UsingCctv,powered=rival.Powered,jammed=rival.Jammed,danger=rival.Danger,callProgress=rival.Progress,rescue=rival.RescueRemaining,actors=true,sensorTapseeHidden=rival.SensorTapseeHidden,tapsee=rival.SensorTapsee,don=rival.SensorDon,time=rival.Elapsed,position=rival.Position},Time.deltaTime);
            if(securityCamera!=null){securityCamera.enabled=false;if(rival.Running&&rival.UsingCctv&&rival.SurveillanceOnline&&Time.unscaledTime>=nextCameraFrame){HouseCctv.ApplySelected(securityCamera);securityCamera.Render();nextCameraFrame=Time.unscaledTime+.15f;}}
            return;
        }
        visuals.SetEyeHeight(hidden?activeHide.eye:motor.State.EyeHeight);
        guide.Tick(player.transform.position,eye.transform.forward,CurrentContext(),called,started&&!paused&&!won&&!lost,
            !hidden&&!cameras&&suspicion<.1f,Time.deltaTime);
        if(visuals!=null)visuals.SetInteraction(!called&&callProgress>0,hidden,cameras,security.Powered,started&&!paused&&!won&&!lost&&!(hidden&&activeHide.LowView));
        if(soundscape!=null)soundscape.Tick(new ThrillerSoundscape.State {
            started=started,paused=paused,won=won,lost=lost,hidden=hidden,called=called,
            powered=security.Powered,cameras=cameras,motion=security.MotionAlert,
            danger=suspicion,callProgress=callProgress,rescue=rescue,actors=true,sensorTapseeHidden=hidden,tapsee=player.transform.position,don=don.transform.position,time=elapsed,position=player.transform.position
        },Time.deltaTime);
    }
    bool CallHeld()
    {
        if(Input.GetKey(KeyCode.F)) return true;
        if(Application.isMobilePlatform||Input.touchCount>0)return touchInput.ActionHeld;
        return Cursor.lockState!=CursorLockMode.Locked&&Input.GetMouseButton(0)&&TacticalHud.Action.Contains(hud.Position(Input.mousePosition));
    }
    void UpdatePoliceCall(){AdvancePoliceCall(CallHeld(),Time.deltaTime);}
    void AdvancePoliceCall(bool held,float dt)
    {
        if(!started||paused||won||lost||called)return;
        bool valid=!hidden&&!cameras&&CurrentContext()==TacticalHud.Phone&&held;
        bool interrupted=!valid&&callProgress>.1f;
        objective.Call(valid,dt,elapsed);
        if(interrupted)Notify("Call interrupted. Keep looking at the phone and hold the call action.");
        if(called){cameras=false;Notify("Call connected. Survive for "+Mathf.CeilToInt(rescue)+" seconds until help arrives.");}
    }
    void AdvanceOutcome(bool captured,float dt)
    {
        if(!started||paused||won||lost)return;
        objective.Resolve(captured,dt);objective.Limit(elapsed);
        if(won||lost)Lock(false);
    }
    void OnDestroy()
    {
        GamePreferences.Save();
        mainMenu.Dispose();
        hud.Dispose();
        if(nav != null) { NavMesh.RemoveAllNavMeshData(); Destroy(nav); }
        if(securityCamera!=null) securityCamera.targetTexture=null;
        if(securityFeed!=null) { securityFeed.Release(); Destroy(securityFeed); }
        if(footstep!=null) Destroy(footstep);
    }
    void ChooseSearchPoint()
    {
        if(!don.isOnNavMesh)return;
        var path=new NavMeshPath();
        for(int attempt=0;attempt<6;attempt++) {
            Vector2 offset=Random.insideUnitCircle*difficulty.SearchRadius;
            Vector3 candidate=lastSeen+new Vector3(offset.x,0,offset.y);
            NavMeshHit hit;
            // A broad 3D sample can snap through the floor into a different storey.
            if(!NavMesh.SamplePosition(candidate,out hit,.7f,NavMesh.AllAreas))continue;
            if(Mathf.Abs(hit.position.y-lastSeen.y)>.6f)continue;
            if(!don.CalculatePath(hit.position,path)||path.status!=NavMeshPathStatus.PathComplete)continue;
            don.SetPath(path);return;
        }
        // No valid local point: retain the last destination until the search expires.
    }
    bool CanSee()
    {
        Vector3 delta=eye.transform.position-(don.transform.position+Vector3.up*.65f);
        float range=motor.State.Crouched&&state!="Chase"?6.5f:9;
        if(delta.magnitude>range||Vector3.Angle(don.transform.forward,delta)>65) return false;
        return InteractionRaycast.Clear(don.transform.position+Vector3.up*.65f,eye.transform.position,don.GetComponent<Collider>(),player);
    }
    bool Near(Vector3 point) { return Vector3.Distance(player.transform.position+Vector3.up,point)<2.1f; }
    bool Focused(Vector3 point)
    {
        Vector3 delta=point-eye.transform.position;
        if(Vector3.Dot(eye.transform.forward,delta.normalized)<.84f) return false;
        // Prompt anchors sit just outside the object's collision surface. Any blocker
        // before that anchor hides the prompt, including a wall close to the object.
        return InteractionRaycast.Clear(eye.transform.position,point,player);
    }
    int CurrentContext()
    {
        if(cameras) return TacticalHud.None;
        if(hidden) return TacticalHud.Leave;
        if(bedroomLights!=null&&bedroomLights.FocusSwitch(eye)>=0)return TacticalHud.Door;
        if(bedroomDoors!=null&&bedroomDoors.Focus(eye)>=0)return TacticalHud.Door;
        if(basementPlay!=null&&basementPlay.Focus(eye.transform.position,eye.transform.forward,player)>=0)return TacticalHud.Item;
        if(!called&&Near(phonePrompt)&&Focused(phonePrompt)) return TacticalHud.Phone;
        if(!security.Powered&&Near(fuseBox)&&Focused(powerPrompt)) return TacticalHud.Power;
        if(FindHidingSpot()!=null)return TacticalHud.Hide;
        return TacticalHud.None;
    }
    HidingLayout.Spot FindHidingSpot()
    {
        HidingLayout.Spot best=null;float distance=2.1f;
        foreach(var spot in hidingSpots) {
            Vector3 prompt=HidingAccess.Prompt(spot,eye.transform.position);
            float d=Vector3.Distance(player.transform.position+Vector3.up,prompt);
            if(d<distance&&(HidingAccess.IsBedroomBed(spot)?HidingAccess.BedVisible(spot,eye.transform.position,eye.transform.forward,player):Focused(prompt))){best=spot;distance=d;}
        }
        return best;
    }
    void Interact()
    {
        if(!hidden&&!cameras&&bedroomLights!=null) {
            int light=bedroomLights.FocusSwitch(eye);
            if(light>=0){bedroomLights.ToggleSwitch(light);Notify(bedroomLights.SwitchFeedback(light));HearPlayer(2);return;}
        }
        if(!hidden&&!cameras&&bedroomDoors!=null) {
            int door=bedroomDoors.Focus(eye);
            if(door>=0){bedroomDoors.Toggle(door,player.transform.position);HearPlayer(4);return;}
        }
        if(!hidden&&!cameras&&basementPlay!=null){int item=basementPlay.Focus(eye.transform.position,eye.transform.forward,player);if(item>=0){string message;bool noise=basementPlay.Use(item,0,elapsed,flashlights,out message);Notify(message);if(noise)HearDistraction(BasementPlay.RadioPoint,14);return;}}
        if(hidden) {
            Vector3 exit=hideReturn;
            if(!motor.CanOccupy(exit,motor.State.Height))exit=new Vector3(activeHide.ex,activeHide.ey,activeHide.ez);
            if(!motor.CanOccupy(exit,motor.State.Height)){Notify("The exit is blocked. Wait before leaving.");return;}
            hidden=false;cameras=false;player.transform.position=exit;yaw=hideReturnYaw;pitch=hideReturnPitch;
            player.transform.rotation=Quaternion.Euler(0,yaw,0);eye.transform.localRotation=Quaternion.Euler(pitch,0,0);
            player.enabled=true;motor.ResetVertical();visuals.SetEyeHeight(motor.State.EyeHeight,true);
            Notify("You left "+activeHide.name+". Stay out of sight.");activeHide=null;return;
        }
        if(CurrentContext()==TacticalHud.Power) {
            security.Restore();nextCameraFrame=0;
            HearPlayer(7); // The breaker click must reach Don through the existing hearing checks.
            Notify("Security power restored. Don may investigate the noise.");return;
        }
        if(CurrentContext()==TacticalHud.Hide) {
            if(!motor.Grounded){Notify("Land before entering a hiding spot.");return;}
            if(CanSee()){Notify("Don can see you! Break line of sight before hiding.");return;}
            activeHide=FindHidingSpot();if(activeHide==null)return;
            hideReturn=player.transform.position;hideReturnYaw=yaw;hideReturnPitch=pitch;
            player.enabled=false;motor.ResetVertical();player.transform.position=new Vector3(activeHide.x,activeHide.y,activeHide.z);
            yaw=activeHide.yaw;pitch=activeHide.tub?-20:0;player.transform.rotation=Quaternion.Euler(0,yaw,0);eye.transform.localRotation=Quaternion.Euler(pitch,0,0);
            hidden=true;cameras=false;visuals.SetEyeHeight(activeHide.eye,true);
            Notify(activeHide.tub?"Hidden below the bath rim. Peek through the curtain gap; use the action to leave.":"Hidden in "+activeHide.name+". Look around; use the action to leave.");
        }
    }
    void ToggleFlashlight(){if(paused)return;flashlights.Toggle(0);if(!Application.isMobilePlatform)Notify(flashlights.Feedback(0));}
    void ReadInput()
    {
        if(Input.GetKeyDown(KeyCode.L))ToggleFlashlight();
        sprintHeld=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift);
        jumpPressed=Input.GetKeyDown(KeyCode.Space);
        crouchPressed=Input.GetKeyDown(KeyCode.LeftControl)||Input.GetKeyDown(KeyCode.RightControl);
        movement=Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),1);
        look=Cursor.lockState==CursorLockMode.Locked?GamePreferences.MouseLook(new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"))):Vector2.zero;
        touchInput.BeginFrame(Input.touchCount);
        foreach(var t in Input.touches){var p=hud.Position(t.position);touchInput.Process(t.fingerId,t.phase,p,t.deltaPosition,MobileTouchInput.At(p,hud.IsGuideControl(p),t.phase!=TouchPhase.Began||!TacticalHud.Action.Contains(p)||CurrentContext()!=TacticalHud.None));}
        if(Application.isMobilePlatform||Input.touchCount>0){
            movement=touchInput.Move;look=GamePreferences.TouchLook(touchInput.LookPixels);
            sprintHeld=touchInput.SprintOn;crouchPressed|=touchInput.CrouchPressed;jumpPressed|=touchInput.JumpPressed;
            if(touchInput.FlashlightPressed)ToggleFlashlight();
            if(touchInput.CameraPressed)cameras=!cameras;
            if(touchInput.ActionPressed&&CurrentContext()!=TacticalHud.Phone)Interact();
            if(touchInput.PausePressed){paused=true;Lock(false);touchInput.Reset();}
        }
        if(Input.touchCount==0&&!Application.isMobilePlatform) {


            if(Cursor.lockState!=CursorLockMode.Locked&&Input.GetMouseButtonDown(0)) {
                Vector2 pos=hud.Position(Input.mousePosition);
                if(TacticalHud.FlashlightButton.Contains(pos))ToggleFlashlight();
                else if(TacticalHud.CrouchButton.Contains(pos))crouchPressed=true;
                else if(TacticalHud.JumpButton.Contains(pos))jumpPressed=true;
                else if(TacticalHud.CameraButton.Contains(pos)) cameras=!cameras;
                else if(TacticalHud.Action.Contains(pos)&&CurrentContext()!=TacticalHud.Phone) Interact();
                else if(TacticalHud.PauseButton.Contains(pos)) {paused=true;Lock(false);}
            }
        }
    }
    void Lock(bool value) { Cursor.lockState=value&&!Application.isMobilePlatform?CursorLockMode.Locked:CursorLockMode.None; Cursor.visible=!value; }
    void PauseLocalControls()
    {
        if(!started||won||lost)return;
        paused=true;touchInput.Reset();
        sprintHeld=jumpPressed=crouchPressed=false;movement=look=Vector2.zero;Lock(false);
    }
    void OnApplicationFocus(bool focus){if(!focus)PauseLocalControls();}
    void OnApplicationPause(bool pause){if(pause){PauseLocalControls();GamePreferences.Save();}}
    void OnGUI()
    {
        if(pendingScene!=FrontEndAction.None) {
            GUI.matrix=Matrix4x4.identity;GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.blackTexture);return;
        }
        float sx=Screen.width/1100f, sy=Screen.height/700f; GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(sx,sy,1));
        if(!started) {
            if(!introRequested) {
                if(mainMenu.Draw(hud,visuals.GlassBackground,menuStorm)) {
                    if(menuStorm!=null)menuStorm.SetVisible(false);
                    introRequested=true;GamePreferences.Save();
                    if(MatchOptions.Mode!=MatchMode.SinglePlayer)BeginGameplay();
                    else introVideo=gameObject.AddComponent<IntroVideo>();
                }
                return;
            }
            if(introVideo!=null&&!introVideo.Failed&&!introVideo.Finished) {
                introVideo.Draw(); return;
            }
            if((introVideo!=null&&introVideo.Finished)||story.DrawIntro()) {
                BeginGameplay();
            }
            return;
        }
        if(rival!=null){rival.Draw(visuals.GlassBackground);return;}
        if(won||lost) {
            flashlights.SetActive(false);
            RequestScene(hud.DrawEnding(won,called,elapsed,visuals.GlassBackground));
            return;
        }
        if(paused) {
            var action=hud.DrawPause(visuals.GlassBackground);
            if(action==FrontEndAction.Resume){GamePreferences.Save();paused=false;Lock(true);}
            else RequestScene(action);
            return;
        }
        Vector3 location=player.transform.position;
        bool upstairs=location.y>3.1f;
        string upperRoom=location.x<-10||location.z<-7?"BALCONY":location.x>-4.2f&&location.x<-1.4f&&location.z>2.5f&&location.z<5.5f?"WASHROOM 1":location.x>7.4f&&location.z>2.5f?"WASHROOM 2":Mathf.Abs(location.x)<2.4f&&location.z>5.5f?"LANDING":location.x<0?"BEDROOM 1":"BEDROOM 2";
        bool basement=location.y< -1;
        string lowerRoom=location.x>1.4f&&location.x<3.8f&&location.z>3?"BASEMENT STAIRS":location.x< -5?(location.z>0?"OFFICE / GAMING":"GYM"):location.x< -1.4f?"GAME ROOM":location.x<1.4f?"SAFE ROOM":location.x<5?(location.z>2?"STORAGE":location.z< -2?"UTILITY":"PASSAGE"):(location.z>2?"LAUNDRY":"WORKSHOP");
        string room=basement?lowerRoom:upstairs?upperRoom:Mathf.Abs(location.x)<1.4f&&location.z>.4f?"CENTRAL STAIRS":location.x>1.4f&&location.x<3.8f&&location.z>2.7f?"BASEMENT STAIRS":location.x>10?"GARAGE":location.x<-6&&location.z<-3.2f?"STORE ROOM":location.x<0?"KITCHEN / DINING":"LIVING ROOM";
        int context=CurrentContext();
        int focusedLight=context==TacticalHud.Door&&bedroomLights!=null?bedroomLights.FocusSwitch(eye):-1;
        int focusedDoor=context==TacticalHud.Door&&focusedLight<0?bedroomDoors.Focus(eye):-1;
        var spot=hidden?activeHide:FindHidingSpot();
        int focusedItem=context==TacticalHud.Item?basementPlay.Focus(eye.transform.position,eye.transform.forward,player):-1;
        if(hud.Draw(new TacticalHud.State {
            sprintArmed=touchInput.SprintOn,called=called,hidden=hidden,powered=security.Powered,motion=security.MotionAlert,
            cameras=cameras,rescue=rescue,progress=callProgress,danger=suspicion,time=elapsed,
            context=context,floor=basement?"BASEMENT":upstairs?"FIRST FLOOR":"GROUND FLOOR",room=EstateRenovation.Room(location,room),
            prompt=focusedItem>=0?basementPlay.Prompt(focusedItem):focusedLight>=0?bedroomLights.SwitchPrompt(focusedLight):focusedDoor>=0?bedroomDoors.Prompt(focusedDoor):context==TacticalHud.Phone?phonePrompt:context==TacticalHud.Power?powerPrompt:context==TacticalHud.Entrance?entrancePrompt:spot!=null?HidingAccess.Prompt(spot,eye.transform.position):Vector3.zero,
            view=eye,feed=securityFeed,glass=visuals.GlassBackground,move=movement,
            notice=elapsed<noticeUntil?notice:null,
            hideAction=focusedItem>=0?basementPlay.Label(focusedItem,0,elapsed,flashlights.Charge(0)):focusedLight>=0?bedroomLights.SwitchLabel(focusedLight):focusedDoor>=0?bedroomDoors.ActionLabel(focusedDoor):spot!=null?spot.action:"HIDE",hideName=spot!=null?spot.name:"hiding spot",
            crouched=motor.State.Crouched,sprinting=motor.State.Sprinting,grounded=motor.Grounded,stamina=motor.State.Stamina,
            guideVisible=guide.Enabled&&!hidden&&!cameras&&suspicion<.1f,guideStep=guide.Step,
            guideTitle=guide.Title,guideCopy=guide.Copy(Application.isMobilePlatform||Input.touchCount>0),
            phoneDistance=Vector3.Distance(eye.transform.position,phonePrompt),
            phoneBearing=Vector3.SignedAngle(Vector3.ProjectOnPlane(eye.transform.forward,Vector3.up),Vector3.ProjectOnPlane(phone-location,Vector3.up),Vector3.up)
        }))guide.Dismiss();
        if(!cameras)hud.DrawMinimap(location,eye.transform.eulerAngles.y,phonePrompt,called,true);
        flashlights.DrawHud(0);
    }
}
