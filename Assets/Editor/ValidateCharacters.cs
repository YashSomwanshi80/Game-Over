using System.Linq;
using UnityEngine;
using UnityEditor;

public static class ValidateCharacters
{
    [MenuItem("Game Over/Check Rigged Characters")]
    public static void Check()
    {
        int failures=0;
        var hair=Resources.Load<Shader>("Characters/CharacterHair");
        if(hair==null){Debug.LogError("CHARACTER FAIL: hair shader missing.");failures++;}
        else foreach(var message in ShaderUtil.GetShaderMessages(hair))if(message.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error){Debug.LogError("CHARACTER FAIL: hair shader "+message.message);failures++;}
        foreach(string name in new[]{"Don","Tapsee"}) {
            var prefab=Resources.Load<GameObject>("Characters/"+name);
            if(prefab==null){Debug.LogError("CHARACTER FAIL: "+name+" prefab missing. Run Game Over > Rebuild Rigged Characters.");failures++;continue;}
            var animator=prefab.GetComponentInChildren<Animator>();
            if(animator==null||animator.avatar==null||!animator.avatar.isValid||animator.runtimeAnimatorController==null){Debug.LogError("CHARACTER FAIL: "+name+" rig/avatar/controller missing or invalid.");failures++;}
            else {
                foreach(string parameter in new[]{"Speed","Crouched"})if(!animator.parameters.Any(p=>p.name==parameter)){Debug.LogError("CHARACTER FAIL: "+name+" parameter "+parameter);failures++;}
                if(animator.runtimeAnimatorController.animationClips.Distinct().Count()<5){Debug.LogError("CHARACTER FAIL: "+name+" requires all five locomotion clips.");failures++;}
            }
            var group=prefab.GetComponent<LODGroup>();
            if(group==null||group.GetLODs().Length!=3){Debug.LogError("CHARACTER FAIL: "+name+" needs three LODs.");failures++;}
            foreach(var renderer in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
                var mesh=renderer.sharedMesh;
                if(mesh==null||renderer.bones.Length==0||renderer.sharedMaterials.Length!=mesh.subMeshCount||renderer.sharedMaterials.Any(m=>m==null)){Debug.LogError("CHARACTER FAIL: "+name+" mesh/bones/material slots "+renderer.name);failures++;continue;}
                if(renderer.name.EndsWith("LOD0")&&mesh.triangles.Length/3>50000){Debug.LogError("CHARACTER FAIL: "+name+" exceeds the 50k main-mesh triangle budget.");failures++;}
            }
        }
        Debug.Log("RIGGED_CHARACTER_CHECK_COMPLETE failures="+failures+". Motion, lighting and performance still need Play-mode/device checks.");
    }
}
