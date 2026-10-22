using UnityEditor;
using UnityEngine;

public static class BakeSoundscape
{
    [MenuItem("Game Over/Rebuild Procedural Audio Assets")]
    public static void Bake()
    {
        ProceduralSoundDesign.Bake("Assets/Resources/Presentation/Audio");AssetDatabase.Refresh();
        Debug.Log("Rebuilt 15 soundscape WAV assets. Listen in Play mode to check the final mix.");
    }
}
