using System.Collections.Generic;
using System.Linq;

namespace DefaultNamespace
{
    public static class Utilities
    {
        
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            int index = UnityEngine.Random.Range(0, source.Count());
            return source.ElementAt(index);
        }
    }
}