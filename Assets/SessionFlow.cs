public enum FrontEndAction { None,Resume,Retry,MainMenu }

// A retry is consumed by the next scene once. Normal launches always show the start page.
public static class SessionFlow
{
    static bool quickRetry;
    public static void SetNext(FrontEndAction action){quickRetry=action==FrontEndAction.Retry;}
    public static bool ConsumeQuickRetry(){bool result=quickRetry;quickRetry=false;return result;}
}
