namespace Processing
{
    public interface IProcessor<T>
    {
        public T Process(T item);
    }
}
