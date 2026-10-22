public enum DonDifficulty { Easy,Normal,Hard }

// Normal preserves the accepted baseline. No preset grants sight through cover or faster movement.
public readonly struct DonDifficultyProfile
{
    public readonly float Response,Memory,SearchRadius,SearchInterval,Hearing;
    public DonDifficultyProfile(float response,float memory,float radius,float interval,float hearing)
    {Response=response;Memory=memory;SearchRadius=radius;SearchInterval=interval;Hearing=hearing;}
}
public static class DonDifficultyRules
{
    public static DonDifficulty Valid(int value)=>value>=0&&value<=2?(DonDifficulty)value:DonDifficulty.Normal;
    public static DonDifficultyProfile For(DonDifficulty value)
    {
        switch(value){
            case DonDifficulty.Easy:return new DonDifficultyProfile(.65f,4,2,2.8f,.8f);
            case DonDifficulty.Hard:return new DonDifficultyProfile(1.25f,10,4.2f,1.3f,1.2f);
            default:return new DonDifficultyProfile(1,7,3,2,1);
        }
    }
    public static string Description(DonDifficulty value)
    {
        switch(value){
            case DonDifficulty.Easy:return "Slower detection. Shorter searches. More room to escape.";
            case DonDifficulty.Hard:return "Quicker detection. Longer, wider searches. Same movement speed.";
            default:return "Balanced detection and searches. The standard survival challenge.";
        }
    }
}
