using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

[InitializeOnLoad]
public static class ValidateMansion
{
    const string Running="GameOver.LayoutValidation.Running";
    static int frames;
    static ValidateMansion() { EditorApplication.update+=Poll; }

    // Run in a disposable project copy with -batchmode -executeMethod ValidateMansion.Batch.
    public static void Batch()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Layout validation bootstrap").AddComponent<GameOver>();
        SessionState.SetBool(Running,true);
        EditorApplication.EnterPlaymode();
    }
    static void Poll()
    {
        if(!SessionState.GetBool(Running,false)||!EditorApplication.isPlaying) return;
        if(++frames<30) return;
        SessionState.SetBool(Running,false);
        int failures=Check();
        Debug.Log("MANSION_VALIDATION_COMPLETE failures="+failures);
        EditorApplication.Exit(failures==0?0:1);
    }
    [MenuItem("Game Over/Check Navigation in Play Mode")]
    public static void CheckFromMenu()
    {
        if(!EditorApplication.isPlaying) { Debug.LogWarning("Press Play, then run Game Over > Check Navigation in Play Mode."); return; }
        int failures=Check();
        Debug.Log("Mansion navigation: "+(failures==0?"all routes passed":failures+" route(s) failed"));
    }
    static int Check()
    {
        string[] names={"Kitchen","Living room","Garage","Store room","First-floor landing","Bedroom 1","Bedroom 2","Washroom 1","Washroom 2","West balcony","Front balcony","Basement exit","Office","Gym","Game room","Safe room","Storage","Laundry","Utility","Workshop","Library","Tech studio","Garden room","Front pedestrian gate","Driveway","Rear terrace"};
        Vector3[] targets={new Vector3(-7,0,-1),new Vector3(7,0,-5),new Vector3(13,0,-3),new Vector3(-7,0,-5),new Vector3(0,3.4f,6.1f),new Vector3(-6,3.4f,3),new Vector3(5,3.4f,3),new Vector3(-3,3.4f,4.2f),new Vector3(8.5f,3.4f,4.2f),new Vector3(-11,3.4f,0),new Vector3(0,3.4f,-8),new Vector3(3.2f,-3.4f,2.7f),new Vector3(-7,-3.4f,3.5f),new Vector3(-7,-3.4f,-4),new Vector3(-3,-3.4f,-2),new Vector3(0,-3.4f,4),new Vector3(4.5f,-3.4f,3),new Vector3(7,-3.4f,4),new Vector3(3,-3.4f,-4),new Vector3(7,-3.4f,-3),new Vector3(-7,3.4f,4.5f),new Vector3(4.4f,3.4f,3.5f),new Vector3(-13,.05f,-2),new Vector3(0,0,-16.8f),new Vector3(13,0,-16.8f),new Vector3(6,0,9)};
        NavMeshHit start;
        if(!NavMesh.SamplePosition(targets[0],out start,.5f,NavMesh.AllAreas)) { Debug.LogError("NAV FAIL: player spawn is not on navigation mesh"); return names.Length; }
        int failures=0;
        for(int i=0;i<targets.Length;i++) {
            NavMeshHit end;var path=new NavMeshPath();
            bool passed=NavMesh.SamplePosition(targets[i],out end,.5f,NavMesh.AllAreas)&&NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
            if(passed) passed=NavMesh.CalculatePath(end.position,start.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
            if(passed) Debug.Log("NAV PASS: "+names[i]+" reachable in both directions; return corners="+path.corners.Length);
            else { failures++;Debug.LogError("NAV FAIL: "+names[i]+" (not a complete route from player spawn)"); }
        }
        foreach(var spot in HidingLayout.Spots()) {
            NavMeshHit exit;var path=new NavMeshPath();
            bool passed=NavMesh.SamplePosition(new Vector3(spot.ex,spot.ey,spot.ez),out exit,.5f,NavMesh.AllAreas)
                &&NavMesh.CalculatePath(start.position,exit.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete
                &&NavMesh.CalculatePath(exit.position,start.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
            if(passed) Debug.Log("NAV PASS: hiding exit / "+spot.name);
            else { failures++;Debug.LogError("NAV FAIL: hiding exit / "+spot.name); }
        }
        Physics.SyncTransforms();
        string[] actions={"Living-room telephone","Security fuse box","Main entrance"};
        Vector3[] approaches={new Vector3(7,0,2.8f),MainPowerLayout.Approach,new Vector3(0,0,-5.7f)};
        Vector3[] prompts={new Vector3(7,1.2f,4),MainPowerLayout.Prompt,new Vector3(.65f,1.2f,-6.82f)};
        for(int i=0;i<actions.Length;i++) {
            NavMeshHit end;var path=new NavMeshPath();
            bool passed=NavMesh.SamplePosition(approaches[i],out end,.3f,NavMesh.AllAreas)
                &&NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
            if(passed) {
                var from=end.position+Vector3.up*1.55f;
                passed=Vector3.Distance(end.position+Vector3.up,prompts[i])<2.1f
                    &&!Physics.Linecast(from,prompts[i],~(1<<8),QueryTriggerInteraction.Ignore);
            }
            if(passed) Debug.Log("INTERACTION PASS: reachable approach and clear standing view / "+actions[i]);
            else {failures++;Debug.LogError("INTERACTION FAIL: approach or view / "+actions[i]);}
        }
        var door=GameObject.Find("Main entrance / working double door");
        var leaves=door!=null?door.GetComponentsInChildren<BoxCollider>():new BoxCollider[0];
        if(leaves.Length==2&&leaves[0].enabled&&leaves[1].enabled)
            Debug.Log("ENTRANCE PASS: paired working entrance leaves have collision");
        else {failures++;Debug.LogError("ENTRANCE FAIL: working entrance collision leaves missing");}
        // This point lies on the switchback landing, not either floor. A complete
        // route here detects a disconnected turn even when floor routes exist.
        NavMeshHit turn;
        var turnPath=new NavMeshPath();
        var turnPoint=new Vector3(2.6f,-1.8f,6.15f);
        bool turnPassed=NavMesh.SamplePosition(turnPoint,out turn,.3f,NavMesh.AllAreas)
            &&NavMesh.CalculatePath(start.position,turn.position,NavMesh.AllAreas,turnPath)
            &&turnPath.status==NavMeshPathStatus.PathComplete;
        if(turnPassed) Debug.Log("NAV PASS: basement switchback landing");
        else { failures++;Debug.LogError("NAV FAIL: basement switchback landing"); }
        return failures;
    }
}

