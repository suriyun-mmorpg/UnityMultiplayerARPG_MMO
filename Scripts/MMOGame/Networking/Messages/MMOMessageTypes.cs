namespace MultiplayerARPG.MMO
{
    public class MMOMessageTypes
    {
        public const short RequestAppServerRegister = 0;
        public const short ResponseAppServerRegister = 1;
        public const short RequestAppServerAddress = 2;
        public const short ResponseAppServerAddress = 3;
        public const short RequestUserLogin = 4;
        public const short ResponseUserLogin = 5;
        public const short RequestUserRegister = 6;
        public const short ResponseUserRegister = 7;
        public const short RequestUserLogout = 8;
        public const short ResponseUserLogout = 9;
        public const short RequestCharacters = 10;
        public const short ResponseCharacters = 11;
        public const short RequestCreateCharacter = 12;
        public const short ResponseCreateCharacter = 13;
        public const short RequestDeleteCharacter = 14;
        public const short ResponseDeleteCharacter = 15;
        public const short RequestSelectCharacter = 16;
        public const short ResponseSelectCharacter = 17;
        public const short RequestSpawnMap = 18;
        public const short ResponseSpawnMap = 19;
        public const short RequestValidateAccessToken = 20;
        public const short ResponseValidateAccessToken = 21;
        public const short UpdateMapUser = 22;
        public const short Chat = 23;
        public const short RequestFacebookLogin = 24;
        public const short RequestGooglePlayLogin = 25;
    }
}
