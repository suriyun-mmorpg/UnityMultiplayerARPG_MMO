namespace MultiplayerARPG.MMO
{
    public static partial class Email
    {
        public static bool IsValid(string email)
        {
            if (email.Trim().EndsWith("."))
                return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
