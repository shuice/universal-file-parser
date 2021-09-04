using System;
using System.Linq;


namespace System
{
    internal class SampleClass : IComparable<int>
    {
        public int CompareTo(int other)
        {
            throw new NotImplementedException();
        }
    }
    public static class ClassExtention
    {
        private static Type middle_base_type = null;
        private static Type runtime_type = typeof(Object).GetType();

        static ClassExtention()
        {
            middle_base_type = typeof(SampleClass).GetInterfaces()[0].GetType();
        }

        public static Type GetReleaseType(this Object o)
        {
            Type r = o.GetType();
            if ((r == runtime_type) || (r == middle_base_type))
            {
                r = r.GetType();
            }
            return r;
        }
    }
}