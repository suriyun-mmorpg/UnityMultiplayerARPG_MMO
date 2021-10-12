namespace MultiplayerARPG.MMO
{
    /// <summary>
    /// It uses simple MD5 hashing without salt since the first version
    /// You can modify this class or set `overrideHash` and `overrideVerify` to change password hashing algorithm
    /// </summary>
    public static partial class PasswordHashing
    {
        public delegate string HashDelegate(string password);
        public delegate bool VerifyDelegate(string password, string hashedPassword);

        public static HashDelegate overrideHash;
        public static VerifyDelegate overrideVerify;

        public static string PasswordHash(this string password)
        {
            if (overrideHash != null)
                return overrideHash.Invoke(password);
            return password.GetMD5();
        }

        public static bool PasswordVerify(this string password, string hashedPassword)
        {
            if (overrideVerify != null)
                return overrideVerify.Invoke(password, hashedPassword);
            return password.GetMD5().Equals(hashedPassword);
        }
    }
}
