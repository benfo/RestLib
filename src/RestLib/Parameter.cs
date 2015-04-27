namespace RestLib
{
    public class Parameter
    {
        public Parameter(string name, string value, ParameterType type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        public string Name { get; private set; }

        public string Value { get; private set; }

        public ParameterType Type { get; private set; }
    }
}