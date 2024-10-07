namespace Domain.DTO.Responses
{
    public class ProcessFileResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public long LinesRead { get; set; }
        public long ProductsProcessed { get; set; }
        public long ResultsWritten { get; set; }
    }
}
