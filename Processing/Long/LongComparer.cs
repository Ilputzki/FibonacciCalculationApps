namespace Processing
{
    public class LongComparer : IComparer<long>
    {
        public int Compare(long x, long y)
        {
            return x.CompareTo(y);
        }
    }
}
