public enum MatchMode { SinglePlayer,Host,Join }
public enum CharacterRole { Tapsee,Don }
public static class MatchOptions
{
    public static MatchMode Mode=MatchMode.SinglePlayer;
    public static CharacterRole Role=CharacterRole.Tapsee;
    public static string Address=""; // Kept for legacy validation tools; gameplay joins by RoomCode.
    public static string RoomCode="";
    public const int Port=27841;
    public static CharacterRole Opposite(CharacterRole role)=>role==CharacterRole.Tapsee?CharacterRole.Don:CharacterRole.Tapsee;
}
