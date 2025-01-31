using System.Collections.Generic;


namespace DDSLib.Compression
{

    public static class SteppedEnumerable
    {

        public static IEnumerable<int> SteppedRange(int fromInclusive, int toExclusive, int step)
        {
            for (int i = fromInclusive; i < toExclusive; i += step)
            {
                yield return i;
            }
        }

    }

}
