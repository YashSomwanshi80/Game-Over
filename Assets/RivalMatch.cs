using UnityEngine;
using UnityEngine.AI;
using System;

// Host simulates both controllers, objectives, hiding, captures and security.
public sealed class RivalMatch : MonoBehaviour
{
    CharacterController tapsee,don;FirstPersonMotor tapMotor,donMotor;Camera eye;
    NavMeshAgent bot;MansionVisuals visuals;TacticalHud hud;Texture feed;
    LanWifiDiscovery wifiDiscovery;LanPeer peer;bool host,online,ready,hello,paused,focused=true,cameras;
    bool priorBackground;
    public CharacterRole Role{get;private set;}
    public bool Finished=>outcome!=0;
    public bool Rescued=>outcome==1;
    public bool Captured=>outcome==2;
    public bool Called=>called;
    public bool Hidden=>Role==CharacterRole.Tapsee&&hidden;
    public bool Powered=>security.Powered;
    public float Progress=>progress;
    public float Elapsed=>time;
    public float Danger=>Role==CharacterRole.Tapsee&&!hidden&&DonSeesTapsee()?.7f:0;
    public Vector3 Position=>Body(Role).transform.position;
    public bool SensorTapseeHidden=>hidden;
    public Vector3 SensorTapsee=>tapsee.transform.position;public Vector3 SensorDon=>don.transform.position;public float RescueRemaining=>rescue;
    public bool Running=>ready&&!paused&&outcome==0;
    public bool UsingCctv=>cameras&&Role==CharacterRole.Tapsee;
    public Action Exit,Retry;
    readonly SecurityState security=new SecurityState();
    RivalPacket local=new RivalPacket(),remote=new RivalPacket(),target;
    readonly RivalRules rules=new RivalRules();
    readonly JammerState jammer=new JammerState();
    public bool Jammed=>jammer.Active(time);public bool SurveillanceOnline=>security.Powered&&!Jammed;
    public string JammerLabel=>Jammed?"JAM "+Mathf.CeilToInt(jammer.Until-time)+"s":time<jammer.ReadyAt?"JAM "+Mathf.CeilToInt(jammer.ReadyAt-time)+"s":"JAM READY";
    float progress{get=>rules.Progress;set=>rules.Progress=value;}
    float rescue{get=>rules.Rescue;set=>rules.Rescue=value;}
    bool called{get=>rules.Called;set=>rules.Called=value;}
    int outcome{get=>rules.Outcome;set=>rules.Outcome=value;}
    float yaw,pitch,sendAt,time,remoteAt,botChoose;
    int sequence,lastRemoteSequence=-1;
    readonly MobileTouchInput touchInput=new MobileTouchInput();
    bool hidden,presentedHidden,snapEyeNext=true;HidingLayout.Spot hiding;Vector3 returnPosition;float returnYaw;
    GameObject survivorVisual;Renderer[] donRenderers;bool[] donEnabled;
    Transform leftArm,rightArm,leftLeg,rightLeg,leftElbow,rightElbow,leftKnee,rightKnee;float gait;
    Animator survivorAnimator;bool rigSpeed,rigCrouch;
    BedroomDoors bedroomDoors;MansionLighting bedroomLights;PlayerFlashlights flashlights;
    BasementPlay basementPlay;SurfaceFootsteps surfaceSteps;
    AudioSource tapSteps,donSteps;AudioClip stepClip;float tapStepAt,donStepAt;
    string connectionIssue;
    string notice="";float noticeUntil;
    string remoteNotice;int remoteNoticeId,lastNoticeId;float remoteNoticeUntil;
    Vector3 phone=new Vector3(7,1.2f,4);readonly Vector3 fuse=MainPowerLayout.Prompt;
    MultiplayerOpeningAtmosphere opening;bool breached,powerReturned;
    bool OpeningBlackout=>online&&MultiplayerOpeningRules.Blackout(time);
    bool PhoneReady=>!online||MultiplayerOpeningRules.CanCall(time,security.Powered);
    GUIStyle label,heading,mobileNotice;bool settings;
    public void Build(CharacterController survivor,NavMeshAgent intruder,Camera camera,MansionVisuals art,TacticalHud controls,Texture cctv,AudioClip footstep)
    {
        tapsee=survivor;tapMotor=survivor.GetComponent<FirstPersonMotor>();eye=camera;visuals=art;hud=controls;feed=cctv;
        basementPlay=GetComponent<BasementPlay>();
        stepClip=footstep;surfaceSteps=FindFirstObjectByType<SurfaceFootsteps>();bedroomDoors=FindFirstObjectByType<BedroomDoors>();bedroomLights=FindFirstObjectByType<MansionLighting>();
        online=MatchOptions.Mode!=MatchMode.SinglePlayer;host=MatchOptions.Mode!=MatchMode.Join;Role=MatchOptions.Role;
        priorBackground=Application.runInBackground;if(online)Application.runInBackground=true;
        intruder.enabled=false;intruder.transform.position=new Vector3(6,.1f,3);
        var visual=intruder.GetComponentInChildren<IntruderVisuals>();if(visual!=null){visual.transform.localPosition=Vector3.zero;visual.transform.localScale=Vector3.one*(visual.ImportedModel?1.7f/1.8f:.89f);}
        don=intruder.gameObject.AddComponent<CharacterController>();don.height=1.7f;don.radius=.25f;don.center=Vector3.up*.85f;
        donMotor=intruder.gameObject.AddComponent<FirstPersonMotor>();donMotor.Configure(don);
        donRenderers=intruder.GetComponentsInChildren<Renderer>();donEnabled=new bool[donRenderers.Length];
        for(int i=0;i<donRenderers.Length;i++)donEnabled[i]=donRenderers[i].enabled;
        BuildSurvivor();
        tapSteps=Steps(tapsee.gameObject);donSteps=Steps(don.gameObject);
        if(online){wifiDiscovery=new LanWifiDiscovery();peer=new LanPeer();peer.Message=Receive;if(host)peer.HostRoom(MatchOptions.Port);else peer.JoinRoom(MatchOptions.RoomCode,MatchOptions.Port);}
        else {ready=true;tapsee.enabled=false;bot=tapsee.gameObject.AddComponent<NavMeshAgent>();bot.radius=.25f;bot.height=1.7f;bot.speed=2.7f;bot.baseOffset=0;bot.updatePosition=false;bot.acceleration=12;bot.angularSpeed=360;bot.stoppingDistance=.25f;bot.Warp(new Vector3(-7,0,-1));tapsee.transform.position=bot.nextPosition;tapsee.enabled=true;}
        flashlights=GetComponent<PlayerFlashlights>();
        if(online) {
            phone=MultiplayerOpeningAtmosphere.Phone;security.SwitchOff();
            tapsee.enabled=false;tapsee.transform.position=MultiplayerOpeningAtmosphere.TapseeSpawn;tapsee.transform.rotation=Quaternion.Euler(0,180,0);tapsee.enabled=host;tapMotor.ResetVertical();
            don.enabled=false;don.transform.position=MultiplayerOpeningAtmosphere.DonOutside;don.transform.rotation=Quaternion.identity;don.enabled=host;donMotor.ResetVertical();
            FindFirstObjectByType<MansionEnvironment>().MoveTelephoneToBasement();
            opening=gameObject.AddComponent<MultiplayerOpeningAtmosphere>();opening.Build(eye,FindFirstObjectByType<PbrLibrary>());
            var storm=FindFirstObjectByType<GameplayStorm>();if(storm!=null)storm.HorrorInterior=true;
            visuals.SetPower(false);
        }
        AttachCamera();
        if(online)SetCursor(false);
    }
    CharacterController Body(CharacterRole role)=>role==CharacterRole.Tapsee?tapsee:don;
    FirstPersonMotor Motor(CharacterRole role)=>role==CharacterRole.Tapsee?tapMotor:donMotor;
    void AttachCamera()
    {
        snapEyeNext=true;
        flashlights?.ConfigureLocal((int)Role);
        eye.transform.SetParent(Body(Role).transform,false);eye.transform.localPosition=Vector3.up*1.55f;
        yaw=Body(Role).transform.eulerAngles.y;pitch=0;
        for(int i=0;i<donRenderers.Length;i++)if(donRenderers[i]!=null)donRenderers[i].enabled=Role!=CharacterRole.Don&&donEnabled[i];
        survivorVisual.SetActive(Role==CharacterRole.Don&&!hidden);
    }
    void BuildSurvivor()
    {
        survivorVisual=new GameObject("Tapsee / remote survivor figure");survivorVisual.transform.SetParent(tapsee.transform,false);
        var library=FindFirstObjectByType<PbrLibrary>();
        var prefab=Resources.Load<GameObject>("Characters/Tapsee");
        if(prefab!=null) {
            var model=Instantiate(prefab,survivorVisual.transform);model.name="Tapsee / imported production rig";
            // Current source mesh sole minimum is -0.01172 m in the imported Y-up frame.
            model.transform.localPosition+=Vector3.up*.011723f;
            foreach(var collider in model.GetComponentsInChildren<Collider>())collider.enabled=false;
            foreach(var renderer in model.GetComponentsInChildren<Renderer>())renderer.gameObject.layer=10;
            CharacterVisibility.Configure(model);
            survivorAnimator=model.GetComponentInChildren<Animator>();
            if(survivorAnimator!=null){survivorAnimator.applyRootMotion=false;foreach(var parameter in survivorAnimator.parameters){if(parameter.name=="Speed"&&parameter.type==AnimatorControllerParameterType.Float)rigSpeed=true;if(parameter.name=="Crouched"&&parameter.type==AnimatorControllerParameterType.Bool)rigCrouch=true;}}
            return;
        }
        VisualMesh.Lathe("Hoodie torso",new Vector3(0,.74f,0),new[]{new Vector2(.17f,0),new Vector2(.2f,.35f),new Vector2(.23f,.65f),new Vector2(.12f,.72f)},library.CharacterCloth,survivorVisual.transform,24,.7f);
        HumanAnatomy.Hood(new Vector3(0,1.34f,.03f),library.CharacterCloth,survivorVisual.transform);
        HumanAnatomy.Head("Tapsee / shaped head",new Vector3(0,1.34f,.06f),library.Skin,survivorVisual.transform);
        CharacterCraft.FaceDetails(survivorVisual.transform,new Vector3(0,1.34f,.06f),library);
        foreach(float side in new[]{-1f,1f}) {
            VisualMesh.Rounded("Tapsee glasses frame",new Vector3(side*.058f,1.525f,.174f),new Vector3(.105f,.051f,.018f),.018f,library.Hair,survivorVisual.transform);
            VisualMesh.Rounded("Tapsee eye white",new Vector3(side*.05f,1.524f,.161f),new Vector3(.039f,.018f,.013f),.006f,library.EyeWhite,survivorVisual.transform);
            VisualMesh.Rounded("Tapsee pupil",new Vector3(side*.05f,1.524f,.171f),new Vector3(.012f,.013f,.006f),.003f,library.Hair,survivorVisual.transform);
            VisualMesh.Rounded("Tapsee glasses lens",new Vector3(side*.058f,1.525f,.186f),new Vector3(.087f,.036f,.009f),.012f,library.ClearLens,survivorVisual.transform);
        }
        VisualMesh.Rounded("Glasses bridge",new Vector3(0,1.529f,.188f),new Vector3(.035f,.009f,.01f),.003f,library.Metal,survivorVisual.transform);
        foreach(float side in new[]{-1f,1f})VisualMesh.Rounded("Hood drawstring",new Vector3(side*.06f,1.29f,.158f),new Vector3(.008f,.23f,.008f),.003f,library.Linen,survivorVisual.transform);
        VisualMesh.Rounded("Hoodie kangaroo pocket",new Vector3(0,.9f,.137f),new Vector3(.28f,.17f,.019f),.025f,library.CharacterCloth,survivorVisual.transform);
        VisualMesh.Rounded("Ribbed hoodie waistband",new Vector3(0,.78f,0),new Vector3(.35f,.055f,.25f),.02f,library.CharacterCloth,survivorVisual.transform);
        foreach(float side in new[]{-1f,1f})VisualMesh.Rounded("Pocket opening stitch",new Vector3(side*.12f,.93f,.153f),new Vector3(.008f,.12f,.006f),.002f,library.Hair,survivorVisual.transform).transform.localRotation=Quaternion.Euler(0,0,side*-19);
        foreach(float side in new[]{-1f,1f}) {
            var arm=VisualMesh.Joint("Hoodie shoulder",new Vector3(side*.27f,1.36f,0),survivorVisual.transform);
            var leg=VisualMesh.Joint("Hip",new Vector3(side*.11f,.76f,0),survivorVisual.transform);
            if(side<0){leftArm=arm;leftLeg=leg;}else{rightArm=arm;rightLeg=leg;}
            var elbow=VisualMesh.Joint("Survivor elbow",new Vector3(0,-.28f,0),arm);
            var knee=VisualMesh.Joint("Survivor knee",new Vector3(0,-.36f,0),leg);
            if(side<0){leftElbow=elbow;leftKnee=knee;}else{rightElbow=elbow;rightKnee=knee;}
            VisualMesh.Lathe("Upper hoodie sleeve",new Vector3(0,-.28f,0),new[]{new Vector2(.065f,0),new Vector2(.079f,.13f),new Vector2(.065f,.29f)},library.CharacterCloth,arm,20,.9f);
            VisualMesh.Lathe("Forearm hoodie sleeve",new Vector3(0,-.25f,0),new[]{new Vector2(.049f,0),new Vector2(.059f,.1f),new Vector2(.065f,.26f)},library.CharacterCloth,elbow,20,.9f);
            CharacterCraft.Hand(elbow,new Vector3(0,-.26f,0),side,library);
            VisualMesh.Lathe("Shaped denim thigh",new Vector3(0,-.36f,0),new[]{new Vector2(.063f,0),new Vector2(.087f,.18f),new Vector2(.08f,.37f)},library.CharacterCloth,leg,20,.9f);
            VisualMesh.Lathe("Shaped denim calf",new Vector3(0,-.33f,0),new[]{new Vector2(.046f,0),new Vector2(.065f,.2f),new Vector2(.063f,.34f)},library.CharacterCloth,knee,20,.9f);
            CharacterCraft.Shoe(knee,new Vector3(0,-.34f,.055f),library,false);
        }
        foreach(var renderer in survivorVisual.GetComponentsInChildren<Renderer>())renderer.gameObject.layer=10;
        foreach(var joint in survivorVisual.GetComponentsInChildren<Transform>()) {
            var attached=new System.Collections.Generic.List<Renderer>();foreach(Transform child in joint){var renderer=child.GetComponent<Renderer>();if(renderer!=null)attached.Add(renderer);}
            if(attached.Count>1)joint.gameObject.AddComponent<StaticVisualBatch>().Build(attached);
        }
    }
    bool roleConflict;CharacterRole availableRole;
    public bool CharacterTaken=>roleConflict;
    public void AcceptAvailableRole(){if(host||!roleConflict)return;Role=availableRole;MatchOptions.Role=Role;roleConflict=false;AttachCamera();peer.Send(JsonUtility.ToJson(new RivalPacket{kind=2,role=(int)Role}));}
    void Receive(string json)
    {
        RivalPacket packet;
        try{packet=JsonUtility.FromJson<RivalPacket>(json);}catch{connectionIssue="Invalid network message.";peer.Dispose();return;}
        if(packet==null||packet.version!=9){connectionIssue="Different game versions. Use the same build.";peer.Dispose();return;}
        if(host) {
            if(packet.kind==2){if(packet.role!=(int)MatchOptions.Opposite(Role)){peer.Send(JsonUtility.ToJson(new RivalPacket{kind=5,role=(int)MatchOptions.Opposite(Role)}));return;}if(!ready)SetCursor(true);ready=true;remoteAt=Time.unscaledTime;return;}
            if(!ready||packet.kind!=3||packet.sequence<=lastRemoteSequence||!packet.ValidInput())return;
            lastRemoteSequence=packet.sequence;remoteAt=Time.unscaledTime;
            packet.jam|=remote.jam;packet.flashToggle|=remote.flashToggle;packet.jump|=remote.jump;packet.crouch|=remote.crouch;packet.action|=remote.action;remote=packet;
        } else {
            if(packet.kind==1||packet.kind==5){if(packet.role<0||packet.role>1){connectionIssue="Invalid character reservation.";peer.Dispose();return;}availableRole=(CharacterRole)packet.role;if(Role!=availableRole){roleConflict=true;SetCursor(false);return;}roleConflict=false;AttachCamera();peer.Send(JsonUtility.ToJson(new RivalPacket{kind=2,role=(int)Role}));return;}
            if(packet.kind!=4)return;if(!ready)SetCursor(true);
            if(Role==CharacterRole.Tapsee&&packet.hidden!=hidden) {
                yaw=packet.tapseeYaw;if(packet.hidden)pitch=packet.bathtub?-20:0;
                // Hiding is a pose transition, not walking through the enclosure.
                tapsee.enabled=false;tapsee.transform.position=packet.tapsee;
            }
            if(packet.noticeId>lastNoticeId&&!string.IsNullOrEmpty(packet.noticeText)) {
                lastNoticeId=packet.noticeId;Notify(packet.noticeText);
            }
            flashlights.Apply(0,packet.tapFlashCharge,packet.tapFlashOn,packet.tapFlashYaw,packet.tapFlashPitch);
            flashlights.Apply(1,packet.donFlashCharge,packet.donFlashOn,packet.donFlashYaw,packet.donFlashPitch);
            bedroomLights?.ApplyHouseMask(packet.houseLightMask);
            bedroomDoors?.ApplySnapshot(packet.doorMask,packet.westDoor,packet.eastDoor,packet.libraryDoor,packet.studioDoor,packet.entranceDoor,packet.gardenRoomDoor,packet.gardenExitDoor,packet.westBalconyDoor,packet.frontBalconyDoor,packet.roomDoors,packet.doorSwingMask);
            // The forced-entry transition is a teleport, not an interpolated walk through the door.
            if(online&&time<60&&packet.time>=60){don.enabled=false;don.transform.position=packet.don;}
            basementPlay.State.RadioUntil=packet.radioUntil;basementPlay.State.RadioReadyAt=packet.radioReadyAt;basementPlay.State.TakenMask=packet.spareTakenMask;
            jammer.Until=packet.jamUntil;jammer.ReadyAt=packet.jamReadyAt;target=packet;ready=true;called=packet.called;progress=packet.progress;rescue=packet.rescue;time=packet.time;outcome=packet.outcome;hidden=packet.hidden;
            if(packet.powered&&!security.Powered)security.Restore();else if(!packet.powered&&security.Powered)security.SwitchOff();
        }
    }
    public void Tick()
    {
        basementPlay?.Present(time,Running&&focused);
        if(survivorAnimator!=null)survivorAnimator.speed=Running?1:0;
        float dt=Mathf.Min(Time.deltaTime,.1f);
        if(peer!=null){peer.Poll(Time.unscaledDeltaTime);if((peer.Connected||peer.Error!=null)&&wifiDiscovery!=null){wifiDiscovery.Dispose();wifiDiscovery=null;}if(host&&peer.Connected&&!hello){hello=true;peer.Send(JsonUtility.ToJson(new RivalPacket{kind=1,role=(int)MatchOptions.Opposite(Role)}));}}
        if(peer!=null&&(!peer.Connected||peer.Error!=null)){basementPlay?.Present(time,false);if(opening!=null)opening.Present(time,false,false);if(ready){ready=false;SetCursor(false);visuals.SetRivalState(Role==CharacterRole.Don,0,0,false,false,hidden,eye.transform.localPosition.y);}return;}
        if(!ready)return;
        if(settings&&Input.GetKeyDown(KeyCode.Escape)){settings=false;return;}
        if(Input.GetKeyDown(KeyCode.Escape)){paused=!paused;SetCursor(!paused);}
        if(Finished){if(opening!=null)opening.Present(time,false,false);flashlights.SetActive(false);SetCursor(false);if(bot!=null)bot.isStopped=true;visuals.SetRivalState(Role==CharacterRole.Don,0,0,false,false,hidden,eye.transform.localPosition.y);if(online&&Time.unscaledTime>=sendAt){if(host)SendSnapshot();else peer.Send(JsonUtility.ToJson(new RivalPacket{kind=3,sequence=++sequence}));sendAt=Time.unscaledTime+.25f;}return;}
        ReadControls();
        if(!host){
            if(Time.unscaledTime>=sendAt){local.kind=3;local.sequence=++sequence;peer.Send(JsonUtility.ToJson(local));local.jam=local.flashToggle=local.jump=local.crouch=local.action=false;sendAt=Time.unscaledTime+.04f;}
            ApplySnapshot(dt);Present();return;
        }
        if(!online&&paused){if(bot!=null)bot.isStopped=true;visuals.SetRivalState(Role==CharacterRole.Don,0,0,false,false,hidden,eye.transform.localPosition.y);return;}
        flashlights.Step(dt);
        time+=dt;
        TickOpening();
        RivalPacket tapInput=Role==CharacterRole.Tapsee?local:remote,donInput=Role==CharacterRole.Don?local:remote;
        if(online&&Time.unscaledTime-remoteAt>.35f){remote.x=remote.z=0;remote.call=remote.sprint=false;}
        Simulate(CharacterRole.Don,donInput,dt);
        if(bot!=null)BotTick(dt);else Simulate(CharacterRole.Tapsee,tapInput,dt);
        bedroomDoors?.OpenForAgent(bot);bedroomDoors?.Tick(dt);
        MoveSurvivorBot(dt);
        remote.jam=remote.flashToggle=remote.jump=remote.crouch=remote.action=false;
        local.jam=local.flashToggle=local.jump=local.crouch=local.action=false;
        security.Tick(dt,Mathf.Abs(don.transform.position.y)<2&&Mathf.Abs(don.transform.position.x)<1.25f&&Mathf.Abs(don.transform.position.z)<2.5f);
        rules.Resolve((!online||breached)&&!hidden&&CanCaptureTapsee(),dt);rules.Limit(time);
        if(online&&(outcome!=0||Time.unscaledTime>=sendAt)){SendSnapshot();sendAt=Time.unscaledTime+.05f;}
        Present();
    }
    void TickOpening()
    {
        if(!online)return;
        if(!breached&&MultiplayerOpeningRules.Breached(time)) {
            breached=true;don.enabled=false;don.transform.position=MultiplayerOpeningAtmosphere.DonEntry;don.enabled=true;donMotor.ResetVertical();
            bedroomDoors?.ForceOpen(4,MultiplayerOpeningAtmosphere.DonOutside);NotifyActor(CharacterRole.Tapsee,"The front door has given way. Don is inside.");NotifyActor(CharacterRole.Don,"You are inside. Hunt Tapsee.");
        }
        if(!powerReturned&&!OpeningBlackout) {
            powerReturned=true;security.Restore();NotifyActor(CharacterRole.Tapsee,"Power restored. The basement-office phone is live.");NotifyActor(CharacterRole.Don,"Power restored. The basement phone is live.");
        }
    }
    void Simulate(CharacterRole role,RivalPacket input,float dt)
    {
        var body=Body(role);var motor=Motor(role);
        if(role==CharacterRole.Tapsee)rules.MaintainCall(input.action,input.call,!hidden&&PhoneReady&&input.call&&NearView(body,input,phone));
        flashlights.Aim((int)role,input.yaw,input.pitch);
        if(input.flashToggle){flashlights.Toggle((int)role);NotifyActor(role,flashlights.Feedback((int)role));}
        if(online&&role==CharacterRole.Don&&!breached){motor.Suspend(dt);return;}
        if(role==CharacterRole.Tapsee&&hidden){motor.Suspend(dt);float angle=hiding.LowView?hiding.yaw+Mathf.Clamp(Mathf.DeltaAngle(hiding.yaw,input.yaw),-65,65):input.yaw;body.transform.rotation=Quaternion.Euler(0,angle,0);if(input.action)LeaveHide();return;}
        if(role==CharacterRole.Don&&input.jam){if(jammer.TryActivate(time)){NotifyActor(CharacterRole.Don,"Jammer active for 15 seconds.");NotifyActor(CharacterRole.Tapsee,"CCTV and sensors jammed for 15 seconds.");}else NotifyActor(CharacterRole.Don,"Jammer cooldown: "+Mathf.CeilToInt(jammer.ReadyAt-time)+"s");}
        body.transform.rotation=Quaternion.Euler(0,input.yaw,0);
        motor.Tick(Vector2.ClampMagnitude(new Vector2(input.x,input.z),1),input.sprint,input.jump,input.crouch,true,dt);
        if(motor.State.StandBlocked)NotifyActor(role,"Not enough headroom to stand here.");
        if(input.action&&bedroomLights!=null) {
            Vector3 from=body.transform.position+Vector3.up*motor.State.EyeHeight;
            int light=bedroomLights.FocusSwitch(from,Quaternion.Euler(input.pitch,input.yaw,0)*Vector3.forward,body);
            if(light>=0){bedroomLights.ToggleSwitch(light);NotifyActor(role,bedroomLights.SwitchFeedback(light));return;}
        }
        if(input.action&&bedroomDoors!=null) {
            Vector3 from=body.transform.position+Vector3.up*motor.State.EyeHeight;
            int door=bedroomDoors.Focus(from,Quaternion.Euler(input.pitch,input.yaw,0)*Vector3.forward,body);
            if(door>=0){if(online&&door==4&&!breached){NotifyActor(role,"The entrance is being forced from outside.");return;}bedroomDoors.Toggle(door,body.transform.position);return;}
        }
        if(input.action&&basementPlay!=null){int item=basementPlay.Focus(body.transform.position+Vector3.up*motor.State.EyeHeight,Quaternion.Euler(input.pitch,input.yaw,0)*Vector3.forward,body);if(item>=0){string message;basementPlay.Use(item,(int)role,time,flashlights,out message);NotifyActor(role,message);return;}}
        if(role==CharacterRole.Don){if(input.action){if(security.Powered&&NearView(body,input,fuse)){security.SwitchOff();NotifyActor(role,"Main power switched off.");}else SearchHide(input);}return;}
        if(input.action) {
            if(!security.Powered&&NearView(body,input,fuse)){if(OpeningBlackout)NotifyActor(role,"Storm outage. Power returns in "+MultiplayerOpeningRules.Remaining(time,120)+" seconds.");else {security.Restore();NotifyActor(role,"Security restored.");}}
            else if(NearView(body,input,new Vector3(.65f,1.2f,-6.82f)))NotifyActor(role,called?"Door locked. Survive until police arrive.":"Door locked. Call the police using the living-room telephone.");
            else foreach(var spot in HidingLayout.Spots())if(NearHide(body,input,spot)) {
                if(!motor.Grounded){NotifyActor(role,"Land before entering a hiding spot.");break;}
                if(DonSeesTapsee()){NotifyActor(role,"Don can see you. Break line of sight before hiding.");break;}
                hiding=spot;returnPosition=body.transform.position;returnYaw=input.yaw;body.enabled=false;body.transform.position=new Vector3(spot.x,spot.y,spot.z);body.transform.rotation=Quaternion.Euler(0,spot.yaw,0);if(Role==CharacterRole.Tapsee){yaw=spot.yaw;pitch=spot.tub?-20:0;}hidden=true;motor.ResetVertical();NotifyActor(role,spot.tub?"Hidden below the bath rim. Peek through the curtain gap; use the action to leave.":"Hidden in "+spot.name+". Use the action to leave.");break;
            }
        }
        bool valid=PhoneReady&&!hidden&&!called&&input.call&&NearView(body,input,phone);
        bool wasCalled=called;rules.Call(valid,dt,time);if(!wasCalled&&called){NotifyActor(CharacterRole.Tapsee,"Police called. Survive for "+Mathf.CeilToInt(rescue)+" seconds.");NotifyActor(CharacterRole.Don,"Tapsee called the police. Catch her before they arrive.");}
    }
    bool CanCaptureTapsee()
    {
        return CaptureFacing.InReach(don.transform.position,don.transform.forward,tapsee.transform.position)
            &&ClearView(don.transform.position+Vector3.up*1.5f,tapsee.transform.position+Vector3.up*1.1f);
    }
    bool ClearView(Vector3 from,Vector3 to)
    {
        return InteractionRaycast.Clear(from,to,tapsee,don);
    }
    bool NearView(CharacterController body,RivalPacket input,Vector3 point)
    {
        Vector3 from=body.transform.position+Vector3.up*Motor(body==tapsee?CharacterRole.Tapsee:CharacterRole.Don).State.EyeHeight;
        return Vector3.Distance(body.transform.position+Vector3.up,point)<2.1f&&Vector3.Dot(Quaternion.Euler(input.pitch,input.yaw,0)*Vector3.forward,(point-from).normalized)>.84f&&ClearView(from,point);
    }
    bool NearHide(CharacterController body,RivalPacket input,HidingLayout.Spot spot){if(!HidingAccess.IsBedroomBed(spot))return NearView(body,input,new Vector3(spot.px,spot.py,spot.pz));return HidingAccess.BedVisible(spot,body.transform.position+Vector3.up*Motor(body==tapsee?CharacterRole.Tapsee:CharacterRole.Don).State.EyeHeight,Quaternion.Euler(input.pitch,input.yaw,0)*Vector3.forward,body);}
    bool LocalNearHide(HidingLayout.Spot spot)=>HidingAccess.IsBedroomBed(spot)?HidingAccess.BedVisible(spot,eye.transform.position,eye.transform.forward,Body(Role)):LocalNear(new Vector3(spot.px,spot.py,spot.pz));
    bool DonSeesTapsee()
    {
        Vector3 delta=tapsee.transform.position-don.transform.position;
        return delta.magnitude<9&&Vector3.Angle(don.transform.forward,delta)<65&&ClearView(don.transform.position+Vector3.up*1.5f,tapsee.transform.position+Vector3.up*1.1f);
    }
    void SearchHide(RivalPacket input)
    {
        foreach(var spot in HidingLayout.Spots())if(NearHide(don,input,spot)) {
            if(hidden&&hiding==null)return;
            if(hidden&&hiding.name==spot.name){outcome=2;return;}NotifyActor(CharacterRole.Don,"No one in this hiding spot.");return;
        }
    }
    void LeaveHide()
    {
        Vector3 exit=returnPosition;if(!tapMotor.CanOccupy(exit,tapMotor.State.Height))exit=new Vector3(hiding.ex,hiding.ey,hiding.ez);
        if(!tapMotor.CanOccupy(exit,tapMotor.State.Height)){NotifyActor(CharacterRole.Tapsee,"The exit is blocked. Wait before leaving.");return;}
        hidden=false;tapsee.transform.position=exit;tapsee.transform.rotation=Quaternion.Euler(0,returnYaw,0);tapsee.enabled=true;tapMotor.ResetVertical();hiding=null;NotifyActor(CharacterRole.Tapsee,"You left the hiding spot. Stay out of sight.");
    }
    Vector3 botGoal,botThreat;float botCommitUntil,botThreatUntil;bool botFleeing;
    void MoveSurvivorBot(float dt)
    {
        if(bot==null||!bot.enabled||!bot.isOnNavMesh||!tapsee.enabled)return;
        Vector3 velocity=bot.isStopped?Vector3.zero:bot.desiredVelocity;
        tapsee.Move((velocity+Vector3.down*2)*dt);
        bot.nextPosition=tapsee.transform.position;
    }
    void BotTick(float dt)
    {
        if(!bot.isOnNavMesh)return;
        Vector3 here=tapsee.transform.position;
        float distance=Vector3.Distance(here,don.transform.position);
        bool sees=distance<8&&Mathf.Abs(here.y-don.transform.position.y)<2&&ClearView(here+Vector3.up*1.5f,don.transform.position+Vector3.up);
        if(sees){botThreat=don.transform.position;botThreatUntil=time+3;}
        bool threatened=time<botThreatUntil;
        var objective=security.Powered?new Vector3(7,0,2.8f):MainPowerLayout.Approach;
        if(!called&&!threatened&&Vector3.Distance(here,objective)<.75f){
            bot.isStopped=true;
            if(!security.Powered){security.Restore();visuals.SetPower(true);}else rules.Call(true,dt,time);
            return;
        }
        rules.Call(false,dt);bot.isStopped=false;
        // Keep an escape destination through turns instead of orbiting the pursuer.
        bool arrived=!bot.pathPending&&bot.hasPath&&bot.remainingDistance<.6f;
        if(time<botCommitUntil&&bot.hasPath&&!arrived&&threatened==botFleeing)return;
        if(time<botChoose)return;botChoose=time+.5f;
        botFleeing=threatened;bot.speed=threatened?3.5f:2.7f;
        if(!called&&!threatened){var route=new NavMeshPath();if(bot.CalculatePath(objective,route)&&route.status==NavMeshPathStatus.PathComplete){bot.SetPath(route);botGoal=objective;botCommitUntil=time+3;return;}}
        Vector3 best=here;float score=float.NegativeInfinity;NavMeshPath bestPath=null;
        foreach(var point in new[]{new Vector3(-7,0,3),new Vector3(7,0,-5),new Vector3(13,0,-3),new Vector3(-6,3.4f,3),new Vector3(5,3.4f,3),new Vector3(-7,-3.4f,3),new Vector3(7,-3.4f,-3),new Vector3(-13,.05f,-2),new Vector3(0,0,-11),new Vector3(6,0,9)}){
            NavMeshHit hit;if(!NavMesh.SamplePosition(point,out hit,.65f,bot.areaMask)||Mathf.Abs(hit.position.y-point.y)>.4f||Vector3.Distance(here,hit.position)<2)continue;
            var path=new NavMeshPath();if(!bot.CalculatePath(hit.position,path)||path.status!=NavMeshPathStatus.PathComplete)continue;
            float length=0,risk=100;Vector3 prior=here;
            foreach(var corner in path.corners){length+=Vector3.Distance(prior,corner);Vector3 line=corner-prior;float t=line.sqrMagnitude>.001f?Mathf.Clamp01(Vector3.Dot(botThreat-prior,line)/line.sqrMagnitude):0;risk=Mathf.Min(risk,Vector3.Distance(prior+line*t,botThreat));prior=corner;}
            float value=threatened?Mathf.Min(risk,6)*4+Vector3.Distance(hit.position,botThreat)-length*.35f:length*.15f;
            if(value>score){score=value;best=hit.position;bestPath=path;}
        }
        if(bestPath!=null){bot.SetPath(bestPath);botGoal=best;botCommitUntil=time+4;}
        else {bot.ResetPath();bot.isStopped=true;}
    }
    void SendSnapshot()
    {
        peer.Send(JsonUtility.ToJson(new RivalPacket{kind=4,jamUntil=jammer.Until,jamReadyAt=jammer.ReadyAt,doorSwingMask=bedroomDoors!=null?bedroomDoors.SwingMask:0,radioUntil=basementPlay.State.RadioUntil,radioReadyAt=basementPlay.State.RadioReadyAt,spareTakenMask=basementPlay.State.TakenMask,tapFlashOn=flashlights.On(0),donFlashOn=flashlights.On(1),tapFlashCharge=flashlights.Charge(0),donFlashCharge=flashlights.Charge(1),tapFlashYaw=flashlights.Yaw(0),donFlashYaw=flashlights.Yaw(1),tapFlashPitch=flashlights.Pitch(0),donFlashPitch=flashlights.Pitch(1),houseLightMask=bedroomLights!=null?bedroomLights.HouseMask:HouseLightingLayout.AllOn,roomDoors=bedroomDoors!=null?bedroomDoors.Fractions:null,bedroomLightMask=bedroomLights!=null?bedroomLights.BedroomMask:3,doorMask=bedroomDoors!=null?bedroomDoors.Mask:0,westDoor=bedroomDoors!=null?bedroomDoors.West:0,eastDoor=bedroomDoors!=null?bedroomDoors.East:0,libraryDoor=bedroomDoors!=null?bedroomDoors.Library:0,studioDoor=bedroomDoors!=null?bedroomDoors.Studio:0,entranceDoor=bedroomDoors!=null?bedroomDoors.Entrance:0,gardenRoomDoor=bedroomDoors!=null?bedroomDoors.GardenRoom:0,gardenExitDoor=bedroomDoors!=null?bedroomDoors.GardenExit:0,westBalconyDoor=bedroomDoors!=null?bedroomDoors.WestBalcony:0,frontBalconyDoor=bedroomDoors!=null?bedroomDoors.FrontBalcony:0,noticeId=remoteNoticeId,noticeText=Time.unscaledTime<remoteNoticeUntil?remoteNotice:null,tapsee=tapsee.transform.position,don=don.transform.position,tapseeYaw=tapsee.transform.eulerAngles.y,donYaw=don.transform.eulerAngles.y,
            eyeHeight=hidden?hiding.eye:tapMotor.State.EyeHeight,donEyeHeight=donMotor.State.EyeHeight,yaw=hidden?hiding.yaw:0,action=hidden&&hiding.LowView,bathtub=hidden&&hiding.tub,stamina=Motor(MatchOptions.Opposite(Role)).State.Stamina,crouched=tapMotor.State.Crouched,
            hidden=hidden,called=called,progress=progress,rescue=rescue,time=time,outcome=outcome,powered=security.Powered,donSpeed=donMotor.HorizontalSpeed,tapseeSpeed=tapMotor.HorizontalSpeed,tapGrounded=tapMotor.Grounded,donGrounded=donMotor.Grounded}));
    }
    void ApplySnapshot(float dt)
    {
        if(target==null)return;
        tapsee.enabled=false;don.enabled=false;
        tapsee.transform.position=Vector3.Lerp(tapsee.transform.position,target.tapsee,1-Mathf.Exp(-dt*22));don.transform.position=Vector3.Lerp(don.transform.position,target.don,1-Mathf.Exp(-dt*22));
        tapsee.transform.rotation=Quaternion.Euler(0,Role==CharacterRole.Tapsee?yaw:target.tapseeYaw,0);don.transform.rotation=Quaternion.Euler(0,Role==CharacterRole.Don?yaw:target.donYaw,0);
    }
    void Present()
    {
        float height=Role==CharacterRole.Tapsee?(hidden?(host?hiding.eye:target.eyeHeight):host?tapMotor.State.EyeHeight:target.eyeHeight):host?donMotor.State.EyeHeight:target.donEyeHeight;
        // Presence owns eye height: smooth crouch, but snap teleport-like hide transitions.
        eye.transform.localRotation=Quaternion.Euler(pitch,0,0);
        bool crouched=host?tapMotor.State.Crouched:target.crouched;
        survivorVisual.SetActive(Role==CharacterRole.Don&&!hidden);survivorVisual.transform.localScale=new Vector3(1,crouched&&!rigCrouch?.62f:1,1);
        float tapSpeed=host?(bot!=null?bot.velocity.magnitude:tapMotor.HorizontalSpeed):target.tapseeSpeed;
        float donSpeed=host?donMotor.HorizontalSpeed:target.donSpeed;
        if(survivorAnimator!=null){survivorAnimator.speed=Running?1:0;if(rigSpeed)survivorAnimator.SetFloat("Speed",tapSpeed);if(rigCrouch)survivorAnimator.SetBool("Crouched",crouched);}
        if(leftLeg!=null){gait+=Time.deltaTime*tapSpeed*3.6f;float swing=Mathf.Sin(gait)*Mathf.Clamp01(tapSpeed)*24;
        leftLeg.localRotation=Quaternion.Euler(swing,0,0);rightLeg.localRotation=Quaternion.Euler(-swing,0,0);
        leftArm.localRotation=Quaternion.Euler(-swing*.7f,0,0);rightArm.localRotation=Quaternion.Euler(swing*.7f,0,0);
        float stride=Mathf.Clamp01(tapSpeed/.8f);
        leftKnee.localRotation=Quaternion.Euler(Mathf.Max(0,-Mathf.Sin(gait))*stride*43,0,0);rightKnee.localRotation=Quaternion.Euler(Mathf.Max(0,Mathf.Sin(gait))*stride*43,0,0);
        leftElbow.localRotation=rightElbow.localRotation=Quaternion.Euler(-8-Mathf.Clamp01((tapSpeed-2.7f)/1.7f)*35,0,0);
        }
        flashlights.SetActive(ready&&!Finished);
        flashlights.Pose(0,tapsee.transform.position+Vector3.up*(hidden?(host?hiding.eye:target.eyeHeight):(host?tapMotor.State.EyeHeight:target.eyeHeight)),tapsee);
        flashlights.Pose(1,don.transform.position+Vector3.up*(host?donMotor.State.EyeHeight:target.donEyeHeight),don);
        bool donCrouched=host?donMotor.State.Crouched:target.donEyeHeight<1.15f;
        visuals.SetPower(security.Powered);visuals.SetRivalState(Role==CharacterRole.Don,Role==CharacterRole.Tapsee?tapSpeed:donSpeed,donSpeed,Running&&!paused,cameras,hidden,height,hidden&&(host?hiding.LowView:target.action),Role==CharacterRole.Tapsee&&progress>0&&!called,donCrouched,snapEyeNext||hidden!=presentedHidden);
        snapEyeNext=false;presentedHidden=hidden;
        if(opening!=null){opening.SetCallPoint(phone);opening.Present(time,progress>0&&!called,ready&&!Finished&&focused&&!paused);}
        if(Running){Step(tapSteps,hidden||!(host?bot!=null||tapMotor.Grounded:target.tapGrounded)?0:tapSpeed,ref tapStepAt);Step(donSteps,(host?donMotor.Grounded:target.donGrounded)?donSpeed:0,ref donStepAt);}
    }
    AudioSource Steps(GameObject actor)
    {
        var source=actor.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=1;source.rolloffMode=AudioRolloffMode.Linear;source.minDistance=1;source.maxDistance=16;source.dopplerLevel=0;return source;
    }
    void Step(AudioSource source,float speed,ref float next)
    {
        if(speed<.3f||Time.time<next||stepClip==null)return;
        next=Time.time+(speed>3?.32f:speed<1.6f?.72f:.53f);source.volume=speed>3?.42f:speed<1.6f?.08f:.25f;if(surfaceSteps!=null)surfaceSteps.Play(source,source.transform.position,1,source==donSteps);else source.PlayOneShot(stepClip);
    }
    void ReadControls()
    {
        bool allowed=focused&&!paused;
        bool flash=allowed&&Input.GetKeyDown(KeyCode.L);
        bool jam=allowed&&Role==CharacterRole.Don&&(Input.GetKeyDown(KeyCode.J)||Input.GetKeyDown(KeyCode.C));
        Vector2 move=allowed?Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),1):Vector2.zero;
        Vector2 look=allowed&&Cursor.lockState==CursorLockMode.Locked?GamePreferences.MouseLook(new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"))):Vector2.zero;
        bool action=allowed&&Input.GetKeyDown(KeyCode.E),jump=allowed&&Input.GetKeyDown(KeyCode.Space),crouch=allowed&&(Input.GetKeyDown(KeyCode.LeftControl)||Input.GetKeyDown(KeyCode.RightControl));
        bool call=allowed&&Input.GetKey(KeyCode.F),sprint=allowed&&(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift));
        if(allowed&&Input.GetKeyDown(KeyCode.C)&&Role==CharacterRole.Tapsee)cameras=!cameras;
        touchInput.BeginFrame(Input.touchCount);
        foreach(var t in Input.touches){var p=hud.Position(t.position);touchInput.Process(t.fingerId,t.phase,p,t.deltaPosition,MobileTouchInput.At(p,!allowed,t.phase!=TouchPhase.Began||!TacticalHud.Action.Contains(p)||(!cameras&&ActionHint().Length>0)));}
        if(Application.isMobilePlatform||Input.touchCount>0){
            move=touchInput.Move;look=GamePreferences.TouchLook(touchInput.LookPixels);sprint=touchInput.SprintOn;
            flash|=touchInput.FlashlightPressed;action|=touchInput.ActionPressed;jump|=touchInput.JumpPressed;crouch|=touchInput.CrouchPressed;
            call=touchInput.ActionHeld;
            if(touchInput.CameraPressed){if(Role==CharacterRole.Tapsee)cameras=!cameras;else jam=true;}
            if(touchInput.PausePressed){paused=true;SetCursor(false);touchInput.Reset();}
        }
        if(!allowed||paused){move=look=Vector2.zero;flash=call=sprint=action=jump=crouch=jam=false;}
        yaw=Mathf.Repeat(yaw+look.x,360);
        bool inTub=hidden&&Role==CharacterRole.Tapsee&&(host?hiding.tub:target!=null&&target.bathtub);
        float hidePitch=inTub?45:20;
        pitch=Mathf.Clamp(pitch-look.y,hidden&&Role==CharacterRole.Tapsee?-hidePitch:-75,hidden&&Role==CharacterRole.Tapsee?hidePitch:75);
        if(hidden&&Role==CharacterRole.Tapsee&&(host?hiding.LowView:target.action)){float facing=host?hiding.yaw:target.yaw;yaw=facing+Mathf.Clamp(Mathf.DeltaAngle(facing,yaw),-65,65);}
        local.x=cameras?0:move.x;local.z=cameras?0:move.y;local.yaw=yaw;local.pitch=pitch;local.sprint=sprint&&!cameras;local.call=call&&!cameras;
        local.jam|=jam;local.flashToggle|=flash;local.jump|=jump&&!cameras;local.crouch|=crouch&&!cameras;local.action|=action&&!cameras;
    }
    void NotifyActor(CharacterRole recipient,string text)
    {
        if(recipient==Role){Notify(text);return;}
        if(!online||!host)return;
        remoteNotice=text;remoteNoticeId++;remoteNoticeUntil=Time.unscaledTime+4;
    }
    void Notify(string text){notice=text;noticeUntil=Time.unscaledTime+4;}
    void SetCursor(bool locked){Cursor.lockState=locked&&!Application.isMobilePlatform?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!locked;}
    public void ReplayMultiplayer(){if(!online)return;MainMenu.QueueMultiplayerReplay();Exit?.Invoke();}
    public bool CanReplayMultiplayer=>online&&(Finished||connectionIssue!=null||(peer!=null&&peer.Error!=null));
    void Styles(){if(label!=null)return;label=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};heading=new GUIStyle(label){fontSize=30,fontStyle=FontStyle.Bold};mobileNotice=new GUIStyle(label){fontSize=22,fontStyle=FontStyle.Bold};}
    readonly EndingReveal endingReveal=new EndingReveal();
    public void Draw(Texture glass)
    {
        hud.SetMatrix();Styles();
        if(settings){if(hud.DrawSettings(glass)){GamePreferences.Save();settings=false;}return;}
        if(Finished){
            var endingAction=endingReveal.Draw(Rescued,Rescued==(Role==CharacterRole.Tapsee),called,time,online);
            if(endingAction==FrontEndAction.Retry){if(online)ReplayMultiplayer();else Retry?.Invoke();}
            else if(endingAction==FrontEndAction.MainMenu)Exit?.Invoke();
            return;
        }
        if(!ready||Finished||paused) {
            GUI.Box(new Rect(260,150,580,470),"");
            string status=roleConflict?"CHARACTER ALREADY TAKEN":!ready?(connectionIssue??(peer!=null&&peer.Error!=null?peer.Error:peer!=null&&peer.Connected?"Connected. Waiting for match confirmation...":host?"Waiting for the second player...":"Finding room...")):Finished?((outcome==1)==(Role==CharacterRole.Tapsee)?"YOU WIN":"YOU LOSE"):online?"MATCH CONTINUES":"PAUSED";
            GUI.Label(new Rect(290,180,520,110),status,heading);
            GUI.Label(new Rect(290,300,520,80),"You play as "+Role+(online?(host?"\nROOM CODE: "+(peer!=null?peer.RoomCode:""):"\nROOM CODE: "+MatchOptions.RoomCode):"\nSingle-player against AI Tapsee"),label);
            if(roleConflict){GUI.Label(new Rect(290,385,520,40),"The host is playing "+MatchOptions.Opposite(availableRole)+".",label);if(GUI.Button(new Rect(290,440,520,50),"PLAY AS "+availableRole.ToString().ToUpperInvariant())){MenuClickSound.Play();AcceptAvailableRole();}}
            if(!roleConflict&&paused&&!Finished&&GUI.Button(new Rect(290,390,520,45),"RESUME")){paused=false;SetCursor(true);}
            if(CanReplayMultiplayer){GUI.Label(new Rect(290,390,520,48),"Play again: host a new room and share its new code.",label);if(GUI.Button(new Rect(290,450,520,45),"PLAY AGAIN")){MenuClickSound.Play();ReplayMultiplayer();}}
            if(!online&&Finished&&GUI.Button(new Rect(290,390,520,45),"RETRY"))Retry?.Invoke();
            if(paused&&!Finished&&!CanReplayMultiplayer&&GUI.Button(new Rect(290,450,520,45),"SETTINGS"))settings=true;
            if(GUI.Button(new Rect(290,530,520,45),"MAIN MENU"))Exit?.Invoke();return;
        }
        if(Application.isMobilePlatform){
            hud.DrawRivalMobile((called?"Police / "+Mathf.CeilToInt(rescue)+"s":OpeningObjective())+"  |  "+MatchClock.Label(time),Jammed?"SIGNAL JAMMED / "+Mathf.CeilToInt(jammer.Until-time)+"s":!security.Powered?(OpeningBlackout?"POWER / "+MultiplayerOpeningRules.Remaining(time,120)+"s":"POWER OFF"):"",touchInput.Move,touchInput.SprintOn,host?Motor(Role).State.Crouched:target.crouched,host?Motor(Role).Grounded:(Role==CharacterRole.Tapsee?target.tapGrounded:target.donGrounded),!Hidden&&!cameras,Role==CharacterRole.Tapsee,cameras,Mathf.CeilToInt(flashlights.Charge((int)Role)*100),flashlights.On((int)Role),Role==CharacterRole.Don?JammerLabel:null);
        }else {
        GUI.Box(new Rect(222,24,550,115),"");
        GUI.Label(new Rect(238,32,520,30),Role==CharacterRole.Tapsee?"TAPSEE  /  SURVIVE":"DON  /  HUNT TAPSEE",heading);
        GUI.Label(new Rect(238,76,520,54),called?"Police arrive in "+Mathf.CeilToInt(rescue)+" seconds":Role==CharacterRole.Tapsee?(online?OpeningObjective():"Reach the telephone. Hold F / CALL for "+RivalRules.CallSeconds+" seconds."):"Catch Tapsee. E searches hiding spots or cuts power at the basement utility breaker.",label);
        if(Role==CharacterRole.Tapsee&&!called)GUI.Label(new Rect(238,145,380,40),(online?"PHONE / BASEMENT OFFICE / ":"TELEPHONE / GROUND FLOOR / ")+Mathf.CeilToInt(Vector3.Distance(Position,phone))+" m direct",label);
        }
        if(!Application.isMobilePlatform&&Role==CharacterRole.Don&&GUI.Button(TacticalHud.CameraButton,JammerLabel))local.jam=true;
        if(!Application.isMobilePlatform)GUI.Label(new Rect(620,145,260,35),"MATCH / "+MatchClock.Label(time),heading);
        if(!Application.isMobilePlatform)GUI.Label(new Rect(30,615,900,65),"WASD move / Mouse look / CTRL crouch / SHIFT sprint / SPACE jump\nE action / search / F call (Tapsee) / L flashlight / Esc menu"+(hidden&&Role==CharacterRole.Tapsee?"  /  HIDDEN":""),label);
        string action=ActionHint();if(action.Length>0&&!Application.isMobilePlatform)GUI.Label(new Rect(330,445,500,60),action,label);
        if(Time.unscaledTime<noticeUntil)GUI.Label(new Rect(300,Application.isMobilePlatform?596:180,590,74),notice,Application.isMobilePlatform?mobileNotice:label);
        if(!Application.isMobilePlatform&&GUI.Button(TacticalHud.PauseButton,"II")){paused=true;SetCursor(false);}
        if(cameras&&Role==CharacterRole.Tapsee)hud.DrawSurveillance(feed,SurveillanceOnline,time,glass,Jammed);
        if(Application.isMobilePlatform) {

            if(!cameras&&action.Length>0)hud.DrawMobileUse(MobileActionLabel());

        }
        if(!cameras)hud.DrawMinimap(Position,eye.transform.eulerAngles.y,phone,called,Role==CharacterRole.Tapsee);
        if(!Application.isMobilePlatform)flashlights.DrawHud((int)Role);
        float stamina=host?Motor(Role).State.Stamina:target.stamina;
        if(!Application.isMobilePlatform){GUI.Label(new Rect(790,32,280,35),"STAMINA "+Mathf.RoundToInt(stamina*100)+"%",label);GUI.Label(new Rect(620,75,260,60),security.Powered?"MAIN POWER ON":"POWER OFF / BREAKER: BASEMENT UTILITY",label);}

    }
    string OpeningObjective()
    {
        if(!online)return Role==CharacterRole.Don?"DON / HUNT TAPSEE":"TAPSEE / REACH THE PHONE";
        if(!MultiplayerOpeningRules.Breached(time))return Role==CharacterRole.Don?"FORCING ENTRY / "+MultiplayerOpeningRules.Remaining(time,60)+"s":"DON ENTERS / "+MultiplayerOpeningRules.Remaining(time,60)+"s";
        if(OpeningBlackout)return "SURVIVE BLACKOUT / "+MultiplayerOpeningRules.Remaining(time,120)+"s";
        return Role==CharacterRole.Don?"HUNT / BASEMENT PHONE LIVE":"CALL / BASEMENT OFFICE";
    }
    bool LocalNear(Vector3 point)=>Vector3.Distance(Position+Vector3.up,point)<2.1f&&Vector3.Dot(eye.transform.forward,(point-eye.transform.position).normalized)>.84f&&ClearView(eye.transform.position,point);
    string MobileActionLabel()
    {
        if(!Hidden&&!cameras&&basementPlay!=null){int item=basementPlay.Focus(eye.transform.position,eye.transform.forward,Body(Role));if(item>=0)return basementPlay.Label(item,(int)Role,time,flashlights.Charge((int)Role));}
        if(!Hidden&&!cameras&&bedroomLights!=null) {
            int light=bedroomLights.FocusSwitch(eye);
            if(light>=0)return (bedroomLights.HouseMask&(1<<light))!=0?"LIGHTS\nOFF":"LIGHTS\nON";
        }
        if(!Hidden&&!cameras&&bedroomDoors!=null) {
            int door=bedroomDoors.Focus(eye);if(door>=0)return bedroomDoors.ActionLabel(door).Replace(" ","\n");
        }
        if(Hidden)return "LEAVE";
        if(Role==CharacterRole.Tapsee&&!called&&LocalNear(phone))return PhoneReady?"HOLD TO CALL":"NO POWER";
        if(LocalNear(fuse))return Role==CharacterRole.Don?"CUT POWER":"RESTORE";
        return Role==CharacterRole.Don?"SEARCH":"HIDE";
    }
    string ActionHint()
    {
        if(online&&Role==CharacterRole.Don&&!MultiplayerOpeningRules.Breached(time))return "";
        if(!Hidden&&!cameras&&bedroomLights!=null){int light=bedroomLights.FocusSwitch(eye);if(light>=0)return "E / "+bedroomLights.SwitchLabel(light);}
        if(!Hidden&&bedroomDoors!=null){int door=bedroomDoors.Focus(eye);if(door>=0)return "E / "+bedroomDoors.ActionLabel(door);}
        if(!Hidden&&!cameras&&basementPlay!=null){int item=basementPlay.Focus(eye.transform.position,eye.transform.forward,Body(Role));if(item>=0)return "E / "+basementPlay.Label(item,(int)Role,time,flashlights.Charge((int)Role));}
        if(cameras)return "C / CCTV to close";
        if(Role==CharacterRole.Tapsee&&hidden)return "HIDDEN / E or ACTION to leave";
        if(Role==CharacterRole.Don&&security.Powered&&LocalNear(fuse))return "E / SWITCH OFF MAIN POWER";
        foreach(var spot in HidingLayout.Spots())if(LocalNearHide(spot))return Role==CharacterRole.Don?"E / SEARCH "+spot.name:"E / "+spot.action;
        if(Role==CharacterRole.Tapsee){if(!called&&LocalNear(phone))return PhoneReady?"HOLD F / CALL  "+progress.ToString("0.0")+" / "+RivalRules.CallSeconds+" sec":OpeningBlackout?"PHONE OFFLINE / POWER IN "+MultiplayerOpeningRules.Remaining(time,120)+"s":"PHONE OFFLINE / RESTORE MAIN POWER";if(!security.Powered&&LocalNear(fuse))return "E / RESTORE POWER";if(LocalNear(new Vector3(.65f,1.2f,-6.82f)))return "E / CHECK MAIN DOOR";}
        return "";
    }
    void SuspendLocalControls()
    {
        focused=false;paused=true;touchInput.Reset();if(opening!=null)opening.Present(time,false,false);
        local.x=local.z=0;local.call=local.sprint=false;
        local.action=local.jump=local.crouch=local.flashToggle=local.jam=false;SetCursor(false);
    }
    void OnApplicationFocus(bool value){focused=value;if(!value)SuspendLocalControls();}
    // Online authority keeps running; a background player sends neutral controls.
    // Offline Don pauses its bot through the existing Tick pause guard.
    void OnApplicationPause(bool value){if(value)SuspendLocalControls();else focused=true;}
    void OnDestroy(){wifiDiscovery?.Dispose();peer?.Dispose();if(online)Application.runInBackground=priorBackground;}
}

