namespace Processing
{
    public class IntCalculator : ICalculator<int>
    {
        public int Add(int arg1, int arg2)
        {
            return checked(arg1 + arg2);
        }

        public int Divide(int arg1, int arg2)
        {
            return checked(arg1 / arg2);
        }

        public int Multiply(int arg1, int arg2)
        {
            return checked(arg1 * arg2);
        }

        public int Sub(int arg1, int arg2)
        {
            return checked(arg1 - arg2);
        }
    }
}
