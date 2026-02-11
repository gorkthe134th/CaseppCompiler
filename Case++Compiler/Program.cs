using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

internal partial class Program
{
    private static void Main(string[] args)
    {
        //StreamReader reader = new("test.txt");
        Regex regex = Number();
        List<char> list = new(40);

        list.AddRange("1536asdf");
        var span = CollectionsMarshal.AsSpan(list);
        var e = regex.EnumerateMatches(span);
        Console.WriteLine(e.MoveNext());
        Console.WriteLine(span.Slice(e.Current.Index, e.Current.Length));

        list.Insert(0, '-');
        span = CollectionsMarshal.AsSpan(list);
        e = regex.EnumerateMatches(span);
        Console.WriteLine(e.MoveNext());
        Console.WriteLine(span.Slice(e.Current.Index, e.Current.Length));

        list.Insert(0, 'a');
        Console.WriteLine(regex.IsMatch(CollectionsMarshal.AsSpan(list)));
    }

    [GeneratedRegex(@"^[+-]?[0-9]+")]
    private static partial Regex Number();
}