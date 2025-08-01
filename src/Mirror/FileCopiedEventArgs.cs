public class FileCopiedEventArgs : EventArgs
{
    public string SourcePath { get; init; } = string.Empty;
    public string DestinationPath { get; init; } = string.Empty;
    public long SizeInBytes { get; init; }
}
