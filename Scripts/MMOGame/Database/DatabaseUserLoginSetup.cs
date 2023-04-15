using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [DisallowMultipleComponent]
    public class DatabaseUserLoginSetup : MonoBehaviour, IDatabaseUserLogin
    {
        public string GenerateNewId()
        {
            return GenericUtils.GetUniqueId();
        }

        public string GetHashedPassword(string password)
        {
            return password.PasswordHash();
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return password.PasswordVerify(hashedPassword);
        }

        void Start()
        {
            BaseDatabase database = GetComponent<BaseDatabase>();
            if (database.UserLoginManager == null)
                database.UserLoginManager = GetComponentInChildren<IDatabaseUserLogin>();
            if (database.UserLoginManager == null)
                database.UserLoginManager = this;
        }
    }
}
