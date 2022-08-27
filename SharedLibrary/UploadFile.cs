namespace Shared
{
    public class UploadFile
    {

        public UploadFile() { }

        public string? ComputerName { get; set; }
        public string? FileName { get; set; }
        public int FileLength { get; set; }
        public string? Checksum { get; set; }
        public bool RelativePath { get; set; }

    }
}