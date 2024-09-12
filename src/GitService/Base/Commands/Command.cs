using System.Diagnostics;

namespace Git.Base;

internal abstract class Command<T>
{
    protected string CurrentWorkingDirectory { get; set; } = Directory.GetCurrentDirectory();
    public abstract string CommandArgument { get; }
    public abstract T Execute();

    protected Process ExecuteCommand(string argument)
    {
        Process process = new() {
            StartInfo = {
                FileName = "git",
                Arguments = argument,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        if (!Directory.Exists(CurrentWorkingDirectory)) throw new DirectoryNotFoundException();

        process.StartInfo.WorkingDirectory = CurrentWorkingDirectory;

        return process;
    }
}