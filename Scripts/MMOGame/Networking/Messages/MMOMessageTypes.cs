namespace MultiplayerARPG.MMO
{
    public class MMOMessageTypes
    {
        public const ushort RequestAppServerRegister = 0;
        public const ushort ResponseAppServerRegister = 1;
        public const ushort RequestAppServerAddress = 2;
        public const ushort ResponseAppServerAddress = 3;
        public const ushort RequestUserLogin = 4;
        public const ushort ResponseUserLogin = 5;
        public const ushort RequestUserRegister = 6;
        public const ushort ResponseUserRegister = 7;
        public const ushort RequestUserLogout = 8;
        public const ushort ResponseUserLogout = 9;
        public const ushort RequestCharacters = 10;
        public const ushort ResponseCharacters = 11;
        public const ushort RequestCreateCharacter = 12;
        public const ushort ResponseCreateCharacter = 13;
        public const ushort RequestDeleteCharacter = 14;
        public const ushort ResponseDeleteCharacter = 15;
        public const ushort RequestSelectCharacter = 16;
        public const ushort ResponseSelectCharacter = 17;
        public const ushort RequestSpawnMap = 18;
        public const ushort ResponseSpawnMap = 19;
        public const ushort RequestValidateAccessToken = 20;
        public const ushort ResponseValidateAccessToken = 21;
        public const ushort UpdateMapUser = 22;
        public const ushort Chat = 23;
        public const ushort RequestFacebookLogin = 24;
        public const ushort RequestGooglePlayLogin = 25;
        public const ushort UpdatePartyMember = 26;
        public const ushort UpdateParty = 27;
    }
}
