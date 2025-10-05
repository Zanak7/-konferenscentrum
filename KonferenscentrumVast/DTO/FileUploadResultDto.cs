using System;
namespace KonferenscentrumVast.DTO
{
    //DTO for the file that is used for uploads and returns the filename, path and size.
    public class FileUploadResultDto
    {
        public string FileName { get; set; } = string.Empty;
        public string BlobPath { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}