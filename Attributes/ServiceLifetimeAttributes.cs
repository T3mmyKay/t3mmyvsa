namespace T3mmyvsa.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ScopedServiceAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class SingletonServiceAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class TransientServiceAttribute : Attribute
{
}
