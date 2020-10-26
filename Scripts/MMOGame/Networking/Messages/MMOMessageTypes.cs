namespace MultiplayerARPG.MMO
{
    public class MMOMessageTypes
    {
        public const ushort RequestAppServerRegister = 0;
        public const ushort ResponseAppServerRegister = 1;
        public const ushort RequestAppServerAddress = 2;
        public const ushort ResponseAppServerAddress = 3;
        public const ushort RequestUserLogin = 4;
        public const ushort RequestUserRegister = 6;
        public const ushort RequestUserLogout = 8;
        public const ushort RequestCharacters = 10;
        public const ushort RequestCreateCharacter = 12;
        public const ushort RequestDeleteCharacter = 14;
        public const ushort RequestSelectCharacter = 16;
        public const ushort RequestSpawnMap = 18;
        public const ushort RequestValidateAccessToken = 20;
        public const ushort UpdateMapUser = 22;
        public const ushort Chat = 23;
        public const ushort UpdatePartyMember = 26;
        public const ushort UpdateParty = 27;
        public const ushort UpdateGuildMember = 29;
        public const ushort UpdateGuild = 30;
        public const ushort GenericResponse = 31;
    }
}
