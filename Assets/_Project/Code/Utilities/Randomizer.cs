using System;
using System.Collections.Generic;

#nullable enable
namespace Utilities
{
    public interface IRandomizer
    {
        IList<T> Shuffle<T>(IList<T> list);
        public T GetRandomElement<T>(IList<T> list);
        int Range(int minValue, int maxValue);
    }

    public class Randomizer : IRandomizer
    {
        private readonly Random random = new();  

        public IList<T> Shuffle<T>(IList<T> list)  
        {  
            var n = list.Count;  
            while (n > 1) {  
                n--;  
                var k = random.Next(n + 1);  
                (list[k], list[n]) = (list[n], list[k]);
            }

            return list;
        }

        public T GetRandomElement<T>(IList<T> list)
        {
            if (list.Count == 0)
                throw new ArgumentException("List is empty");
            
            return list[Range(0, list.Count)];
        }

        public int Range(int minValue, int maxValue) => random.Next(minValue, maxValue);
    }
}