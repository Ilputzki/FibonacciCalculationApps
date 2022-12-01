namespace Processing
{
    public class LongCalculator : ICalculator<long>
    {
        public long Add(long arg1, long arg2)
        {
            return checked(arg1 + arg2);
        }

        public long Divide(long arg1, long arg2)
        {
            return checked(arg1 / arg2);
        }

        public long Multiply(long arg1, long arg2)
        {
            return checked(arg1 * arg2);
        }

        public long Sub(long arg1, long arg2)
        {
            return checked(arg1 - arg2);
        }
    }
}
