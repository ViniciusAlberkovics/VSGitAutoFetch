namespace GitAutoFetch
{
    public class Config
    {
        public int DefaultTime { get; set; }
        public int UserTime { get; set; }

        public Config(int defaultTime, int userTime)
        {
            DefaultTime = defaultTime;
            UserTime = userTime;
        }

        public int TimeValue()
        {
            return UserTime > 0 ? UserTime : DefaultTime;
        }
    }
}
