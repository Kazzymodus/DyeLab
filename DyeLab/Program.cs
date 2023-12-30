namespace DyeLab;

static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        using var dyeLab = new DyeLab();
        dyeLab.Run();
    }
}