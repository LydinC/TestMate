using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

public static class Program
{
 static IEnumerable<Dictionary<string, string>> CartesianProduct(Dictionary<string, List<string>> data)
    {
        string[] keys = data.Keys.ToArray();
        string[][] values = data.Values.Select(list => list.ToArray()).ToArray();

        // Initialise an array of indices to keep track of the current index of each list of values.
        int[] indices = new int[keys.Length];

        for (var currentIndex = keys.Length - 1; currentIndex >= 0; currentIndex--)
        {
            while (indices[currentIndex] < values[currentIndex].Length)
            {
                var combination = new Dictionary<string, string>();

                // Loop through the keys and add the corresponding value at the current index to the dictionary.
                for (var keyIndex = 0; keyIndex < keys.Length; keyIndex++)
                {
                    combination[keys[keyIndex]] = values[keyIndex][indices[keyIndex]];
                }
                yield return combination;

                indices[currentIndex]++;
            }
            indices[currentIndex] = 0;
        }
    }

    static void Main(string[] args)
    {
        var mappings = new[]
        {
        new Dictionary<string, List<string>> {
            { "Orientation", new List<string> { "Portrait", "Landscape" } },
            { "Brightness", new List<string> { "Low", "High" } }
        },
        new Dictionary<string, List<string>> {
            { "Flashlight", new List<string> { "On"} }
        },
        new Dictionary<string, List<string>> {
            { "Speed", new List<string> { "On", "Off"} }
        }
        };

        if (mappings.Length > 0) {
            var fullCombinations = new List<Dictionary<string, string>>();
            foreach (var mapping in mappings)
            {
                var combinations = CartesianProduct(mapping);
                fullCombinations.AddRange(combinations);
            }
        }

       

    }

}