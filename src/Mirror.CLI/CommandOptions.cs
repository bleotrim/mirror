using CommandLine;
using System.Threading;

[Verb("copy-file", HelpText = "Copies a single file with hash verification.")]
public class CopyFileOptions
{
    [Option("src", Required = true, HelpText = "Source file path.")]
    public string SourcePath { get; set; }

    [Option("dst", Required = true, HelpText = "Destination file path.")]
    public string DestinationPath { get; set; }

    [Option("overwrite", Default = false, HelpText = "Overwrite if it exists.")]
    public bool Overwrite { get; set; }

    [Option("progress", Default = true, HelpText = "Show progress.")]
    public bool EnableProgress { get; set; }
}

[Verb("copy-dir-content", HelpText = "Copies the files contained in a directory (does not include the root directory).")]
public class CopyDirContentOptions
{
    [Option("src", Required = true, HelpText = "Source directory.")]
    public string SourceDir { get; set; }

    [Option("dst", Required = true, HelpText = "Destination directory.")]
    public string DestinationDir { get; set; }

    [Option("overwrite", Default = false, HelpText = "Overwrite if it exists.")]
    public bool Overwrite { get; set; }

    [Option("progress", Default = true, HelpText = "Show progress.")]
    public bool EnableProgress { get; set; }
}

[Verb("copy-dir", HelpText = "Copies an entire directory (including the root).")]
public class CopyDirOptions
{
    [Option("src", Required = true, HelpText = "Source directory.")]
    public string SourceDir { get; set; }

    [Option("dst", Required = true, HelpText = "Destination directory.")]
    public string DestinationDir { get; set; }

    [Option("overwrite", Default = false, HelpText = "Overwrite if it exists.")]
    public bool Overwrite { get; set; }

    [Option("progress", Default = true, HelpText = "Show progress.")]
    public bool EnableProgress { get; set; }
}

