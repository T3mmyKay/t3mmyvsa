using System.Text;

namespace T3mmyvsa.CLI.Scaffolding;

internal static class ScaffoldFileWriter
{
    public static void WriteGeneratedFile(string path, string content, bool overwrite)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        if (File.Exists(path) && !overwrite)
        {
            Console.WriteLine($"Skipped existing file: {Path.GetFileName(path)}");
            return;
        }

        var existed = File.Exists(path);
        WriteTextAtomically(path, content.TrimEnd() + Environment.NewLine);
        Console.WriteLine($"{(existed ? "Overwrote" : "Created")}: {Path.GetFileName(path)}");
    }

    public static void WriteTextAtomically(string path, string content)
    {
        var directory = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(directory);

        var tempPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

        try
        {
            File.WriteAllText(
                tempPath,
                content,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
