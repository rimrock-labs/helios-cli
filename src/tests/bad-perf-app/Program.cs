// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
while (true)
{
    object obj = new();
    List<string> list = new();
    lock (obj)
    {
        foreach (var number in Enumerable.Range(0, 100000))
        {
            list.Add($"{number}");
        }

        list.Sort();
    }
}