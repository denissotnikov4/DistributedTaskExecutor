namespace WorkerService.Cli.Helpers;

public static class DirectoryHelper
{
    public static void DeleteIfExists(string path, bool recursive = true)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
    }
}