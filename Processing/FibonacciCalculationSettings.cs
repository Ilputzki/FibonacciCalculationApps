namespace Processing
{
	public class FibonacciCalculationSettings<T>
	{
		public T? FirstNumber { get; set; }

        public T? SecondNumber { get; set; }

        public int CacheSize { get; set; }
    }
}

