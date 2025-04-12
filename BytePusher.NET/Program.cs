namespace BytePusher.NET;
class Program
{
    private static readonly Core.BytePusher bytePusher = new Core.BytePusher();
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dotnet BytePusher.NET.dll ROM_path");
            return;
        }

        if (!File.Exists(args[0]))
        {
            Console.WriteLine($"File \"{args[0]}\" does not exist");
            Environment.Exit(1);
        }

        bytePusher.Load(args[0]);

        var window = new Graphics(bytePusher);
        window.Run();
    }
}