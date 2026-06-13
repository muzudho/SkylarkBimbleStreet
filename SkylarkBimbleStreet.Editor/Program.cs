namespace SkylarkBimbleStreet.Editor;

internal static class Program
{
    private static void Main()
    {
        using var editor = new EditorGame();
        editor.Run();
    }
}