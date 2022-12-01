namespace Processing
{
    public interface ICalculator<T>
    {
        public T Add(T arg1, T arg2);
        public T Sub(T arg1, T arg2);
        public T Multiply(T arg1, T arg2);
        public T Divide(T arg1, T arg2);
    }
}
