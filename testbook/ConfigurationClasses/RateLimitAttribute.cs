namespace testbook.ConfigurationClasses
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RateLimitAttribute : Attribute
    {
        public int Limit { get; set; }
        public int WindowTime { get; set; }

        public RateLimitAttribute(int limit, int windowTime)
        {
            Limit = limit;
            WindowTime = windowTime;
        }
    }
}
