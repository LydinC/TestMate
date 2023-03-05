using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

public static class Program
{
    public static List<Dictionary<string, object>> GeneratePermutations(Dictionary<string, object[]> input)
    {
        var keys = input.Keys.ToArray();
        var values = input.Values.Select(v => v.ToArray()).ToList();
        var permutations = new List<Dictionary<string, object>>();
        GeneratePermutationsHelper(keys, values, new Dictionary<string, object>(), permutations);
        return permutations;
    }

    private static void GeneratePermutationsHelper(string[] keys, List<object[]> values, Dictionary<string, object> current, List<Dictionary<string, object>> permutations)
    {
        if (current.Count == keys.Length)
        {
            permutations.Add(new Dictionary<string, object>(current));
            return;
        }
        var index = current.Count;
        var key = keys[index];
        foreach (var value in values[index])
        {
            current[key] = value;
            GeneratePermutationsHelper(keys, values, current, permutations);
            current.Remove(key);
        }
    }

    static void Main(string[] args)
    {
        var input = new Dictionary<string, object[]> {
            {"Bluetooth", new object[] {true, false}},
            {"Orientation", new object[] {"3", "4", "5"}}
        };
        var permutations = GeneratePermutations(input);
        Console.WriteLine(permutations.ToString());

        Console.WriteLine(permutations.ToString());

        Console.WriteLine(permutations.ToString());
    }
}