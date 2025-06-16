public class FileCopyOptions
{
    public bool Overwrite { get; set; } = false;
    public string HashAlgorithm { get; set; } = "SHA256";
    public bool EnableProgress { get; set; } = false;
}