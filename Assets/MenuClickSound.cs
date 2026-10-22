using UnityEngine;
// Menu feedback survives a scene transition long enough for the click to finish.
public sealed class MenuClickSound : MonoBehaviour {
 static MenuClickSound instance;AudioSource source;AudioClip clip;
 public static void Play(){if(instance==null){var go=new GameObject("Menu / interface click");DontDestroyOnLoad(go);instance=go.AddComponent<MenuClickSound>();instance.source=go.AddComponent<AudioSource>();instance.source.playOnAwake=false;instance.source.spatialBlend=0;instance.source.ignoreListenerPause=true;instance.clip=Resources.Load<AudioClip>("Presentation/Audio/Click");}if(instance.clip!=null)instance.source.PlayOneShot(instance.clip,.35f);}
 void OnDestroy(){if(instance==this)instance=null;}
}
