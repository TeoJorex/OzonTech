using Domain.DTO.Responses;

namespace Domain.BLL.Services.Interfaces
{
    public interface IProductService
    {
        public void UpdateDegreeOfParallelism(int newDegree);
        public void CancelProcessing();
        public Task<ProcessFileResponse> ProcessFileAsync(string inputFilePath, string outputFilePath);
        public event Action<long, long, long> ProgressChanged;

    }
}
