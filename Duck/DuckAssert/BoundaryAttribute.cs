namespace DuckLib
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class BoundaryAttribute : Attribute
    {
        /*
         * 내부와 외부 경계를 구분하기 위해 사용되는 특성입니다.
         */
        public string Description { get; }

        public BoundaryAttribute(string description)
        {
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class NoThrowAttribute : Attribute
    {
        /*
         * 이 메서드가 예외를 던지지 않음을 나타내는 특성입니다.
         */
        public string Description { get; }

        public NoThrowAttribute(string description)
        {
            Description = description;
        }
    }
}

