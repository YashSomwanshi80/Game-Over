using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LivingRoomAuthoring
{
    [MenuItem("Game Over/Existing Mansion/Disable Legacy Room Overrides")]
    public static void DisableOverrides()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.LogWarning("Stop Play mode before updating the saved scene.");return;}
        var scene=SceneManager.GetActiveScene();int count=0;
        foreach(var anchor in Object.FindObjectsByType<LivingRoomArtAnchor>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
            if(anchor.gameObject.scene!=scene||!anchor.gameObject.activeSelf)continue;
            Undo.RecordObject(anchor.gameObject,"Disable legacy room override");
            anchor.gameObject.SetActive(false);count++;
        }
        if(count>0)EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Disabled "+count+" legacy room override(s). Original mansion retained. Undo restores these objects; save the scene to keep this change. Play mode also ignores these overrides automatically.");
    }
}
