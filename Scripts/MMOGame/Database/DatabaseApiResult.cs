namespace MultiplayerARPG.MMO
{
    public struct DatabaseApiResult
    {
        public bool IsError { get; set; }
        public bool IsSuccess => !IsError;
        public string Error { get; set; }
    }

    public struct DatabaseApiResult<T>
    {
        public bool IsError { get; set; }
        public bool IsSuccess => !IsError;
        public string Error { get; set; }
        public T Response { get; set; }
    }
}