namespace MultiplayerARPG.MMO
{
    public static partial class ProcessArguments
    {
        // Database manager server
        public const string CONFIG_USE_CUSTOM_DATABASE_CLIENT = "useCustomDatabaseClient";
        public const string ARG_USE_CUSTOM_DATABASE_CLIENT = "-" + CONFIG_USE_CUSTOM_DATABASE_CLIENT;
        public const string CONFIG_DATABASE_OPTION_INDEX = "databaseOptionIndex";
        public const string ARG_DATABASE_OPTION_INDEX = "-" + CONFIG_DATABASE_OPTION_INDEX;
        public const string CONFIG_DATABASE_DISABLE_CACHE_READING = "databaseDisableCacheReading";
        public const string ARG_DATABASE_DISABLE_CACHE_READING = "-" + CONFIG_DATABASE_DISABLE_CACHE_READING;
        public const string CONFIG_DATABASE_ADDRESS = "databaseManagerAddress";
        public const string ARG_DATABASE_ADDRESS = "-" + CONFIG_DATABASE_ADDRESS;
        public const string CONFIG_DATABASE_PORT = "databaseManagerPort";
        public const string ARG_DATABASE_PORT = "-" + CONFIG_DATABASE_PORT;
        // Start servers
        public const string CONFIG_START_DATABASE_SERVER = "startDatabaseServer";
        public const string ARG_START_DATABASE_SERVER = "-" + CONFIG_START_DATABASE_SERVER;
    }
}