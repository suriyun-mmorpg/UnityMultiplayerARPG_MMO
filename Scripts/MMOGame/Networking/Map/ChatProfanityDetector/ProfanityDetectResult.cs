namespace MultiplayerARPG.MMO
{
    public struct ProfanityDetectResult
    {
        public string message;
        public bool shouldMutePlayer;
        public bool shouldKickPlayer;
        public int muteMinutes;
    }
}