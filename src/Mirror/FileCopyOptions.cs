public class FileCopyOptions
{
    public bool Overwrite { get; set; } = false;
    public HashAlgorithmType HashAlgorithm { get; set; } = HashAlgorithmType.SHA256;
    public bool EnableProgress { get; set; } = false;
    public bool DeleteSourceIfVerified { get; set; } = false;
}