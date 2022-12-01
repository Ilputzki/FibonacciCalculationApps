using Microsoft.Extensions.Options;

namespace Processing
{
    public class FibonacciProcessor<T> : IProcessor<T>
    {
        private const string invalidArgumentErrorMessage = "Invalid element of the Fibonacci sequence passed";

        private readonly LinkedList<T> lastNumbersCache = new();

        private readonly ICalculator<T> calculator;

        private readonly IComparer<T> comparer;

        private readonly int cacheSize;

        private readonly ReaderWriterLockSlim cacheLock = new(LockRecursionPolicy.NoRecursion);

        public FibonacciProcessor(ICalculator<T> calculator, IComparer<T> comparer, IOptions<FibonacciCalculationSettings<T>> settings)
        {
            if (settings.Value.CacheSize <= 0)
                throw new ArgumentException($"Property value less than or equal or not specified - {nameof(settings.Value.CacheSize)}");

            if (settings.Value.FirstNumber == null)
                throw new ArgumentException($"Property value not specified - {nameof(settings.Value.SecondNumber)}");

            if (settings.Value.SecondNumber == null)
                throw new ArgumentException($"Property value not specified - {nameof(settings.Value.SecondNumber)}");

            this.calculator = calculator;
            this.comparer = comparer;
            this.cacheSize = settings.Value.CacheSize;
            lastNumbersCache.AddFirst(settings.Value.FirstNumber);
            lastNumbersCache.AddLast(settings.Value.SecondNumber);
        }

        public T Process(T item)
        {
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                var currentNumber = lastNumbersCache.Last!.Value;
                if (comparer.Compare(currentNumber, item) <= 0)
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        while (comparer.Compare(currentNumber, item) < 0)
                        {
                            currentNumber = calculator.Add(currentNumber, lastNumbersCache.Last!.Previous!.Value);
                            AddToCache(currentNumber);
                        }
                        if (comparer.Compare(currentNumber, item) != 0)
                            throw new ArgumentException(invalidArgumentErrorMessage);
                        currentNumber = calculator.Add(currentNumber, lastNumbersCache.Last!.Previous!.Value);
                        AddToCache(currentNumber);
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                }
                else
                {
                    cacheLock.EnterReadLock();
                    try
                    {
                        currentNumber = lastNumbersCache.First!.Value;
                        if (comparer.Compare(currentNumber, item) > 0)
                        {
                            var previousNumber = calculator.Sub(lastNumbersCache.First!.Next!.Value, currentNumber);
                            while (comparer.Compare(previousNumber, item) > 0)
                            {
                                var temp = previousNumber;
                                previousNumber = calculator.Sub(currentNumber, previousNumber);
                                currentNumber = temp;
                            }
                            if (comparer.Compare(previousNumber, item) != 0)
                                throw new ArgumentException(invalidArgumentErrorMessage);
                        }
                        else
                        {
                            var foundNumber = lastNumbersCache.Find(item);
                            if (foundNumber == null)
                                throw new ArgumentException(invalidArgumentErrorMessage);
                            currentNumber = foundNumber.Next!.Value;
                        }
                    }
                    finally
                    {
                        cacheLock.ExitReadLock();
                    }
                }
                return currentNumber;
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }

        private void AddToCache(T number)
        {
            lastNumbersCache.AddLast(number);
            if (lastNumbersCache.Count > cacheSize)
                lastNumbersCache.RemoveFirst();
        }
    }
}
