using System.Text.RegularExpressions;

namespace MultiplayerARPG.MMO
{
    /// <summary>
    /// Validate character name and guild name, it will allow A-Z, a-z, and 0-9 by default, other characters won't be allowed
    /// You can modify this class or set `overrideCharacterNameValidating` and `overrideGuildNameValidating` to change how name validated
    /// </summary>
    public static partial class NameValidating
    {
        public delegate bool ValidateDelegate(string name);

        public static ValidateDelegate overrideUsernameValidating;
        public static ValidateDelegate overrideCharacterNameValidating;
        public static ValidateDelegate overrideGuildNameValidating;

        public static bool ValidateUsername(string name)
        {
            if (overrideUsernameValidating != null)
                return overrideUsernameValidating.Invoke(name);
            return Regex.Match(name, "^[a-zA-Z0-9_]*$").Success;
        }

        public static bool ValidateCharacterName(string name)
        {
            if (overrideCharacterNameValidating != null)
                return overrideCharacterNameValidating.Invoke(name);
            return Regex.Match(name, "^[a-zA-Z0-9_]*$").Success;
        }

        public static bool ValidateGuildName(string name)
        {
            if (overrideGuildNameValidating != null)
                return overrideGuildNameValidating.Invoke(name);
            return Regex.Match(name, "^[a-zA-Z0-9_]*$").Success;
        }
    }
}
