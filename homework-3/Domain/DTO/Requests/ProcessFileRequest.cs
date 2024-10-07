namespace Domain.DTO.Requests
{
    public class ProcessFileRequest
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
        public int DegreeOfParallelism { get; set; }
    }
}
