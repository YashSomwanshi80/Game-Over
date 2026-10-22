using UnityEngine;
using UnityEngine.Video;

// StreamingAssets keeps the same local MP4 available in both editor and packaged builds.
public sealed class IntroVideo : MonoBehaviour
{
    VideoPlayer video;
    readonly IntroPlaybackPolicy playback=new IntroPlaybackPolicy();
    public bool Finished { get; private set; }
    public bool Failed { get; private set; }

    void Awake()
    {
        video=gameObject.AddComponent<VideoPlayer>();
        video.playOnAwake=false; video.isLooping=false; video.waitForFirstFrame=true;
        video.source=VideoSource.Url;
        string path=Application.streamingAssetsPath+"/Game-Over-Intro-v2.mp4";
        video.url=path.Contains("://")||path.StartsWith("jar:")?path:new System.Uri(path).AbsoluteUri;
        video.renderMode=VideoRenderMode.APIOnly;
        video.audioOutputMode=VideoAudioOutputMode.Direct;
        video.prepareCompleted+=Ready;
        video.loopPointReached+=Ended;
        video.errorReceived+=Error;
        video.Prepare();
    }
    void Ready(VideoPlayer source)
    {
        if(Finished||Failed) return;
        playback.Ready();
        if(source.audioTrackCount>0) source.SetDirectAudioVolume(0,.8f*GamePreferences.Master);
        SyncPlayback();
    }
    void Ended(VideoPlayer source) { Finish(); }
    void Error(VideoPlayer source,string message)
    {
        if(Finished) return;
        Debug.LogWarning("Intro video unavailable; using storyboard. "+message);
        Failed=true; source.Stop();
    }
    void Update()
    {
        if(Finished||Failed) return;
        string timeout=playback.Step(Time.unscaledDeltaTime,video.time);
        if(timeout!=null){Error(video,timeout);return;}
        if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Escape)) Finish();
    }
    public void Finish()
    {
        if(Finished||Failed) return;
        Finished=true; video.Stop();
    }
    public void Draw()
    {
        GUI.DrawTexture(new Rect(0,0,1100,700),Texture2D.blackTexture);
        if(video.texture!=null) GUI.DrawTexture(new Rect(0,0,1100,700),video.texture,ScaleMode.ScaleToFit,false);
        else GUI.Label(new Rect(420,320,400,50),"Loading introduction...");
        if(GUI.Button(new Rect(850,615,225,60),Application.isMobilePlatform?"SKIP INTRO":"SKIP INTRO [SPACE]")) Finish();
    }
    void SyncPlayback()
    {
        if(video==null||Finished||Failed||!playback.Prepared)return;
        if(playback.Active){if(!video.isPlaying)video.Play();}
        else video.Pause();
    }
    void OnApplicationFocus(bool focus){playback.Focused=focus;SyncPlayback();}
    void OnApplicationPause(bool pause){playback.Backgrounded=pause;SyncPlayback();}
    void OnDestroy()
    {
        if(video==null) return;
        video.prepareCompleted-=Ready;video.loopPointReached-=Ended;video.errorReceived-=Error;
        video.Stop(); Destroy(video);
    }
}
