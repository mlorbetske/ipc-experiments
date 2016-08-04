namespace ConsoleApplication1.Contracts
{
    public interface IPrimitiveWrapper<T>
        where T : struct 
    {
        T Value { get; set; }
    }
}