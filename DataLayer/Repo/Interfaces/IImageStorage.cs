namespace DataLayer.Repo.Interfaces
{
    public interface IImageStorage
    {
        Task<string> UploadAsync(string productId, Stream content, string fileName, string contentType);
        Uri GetReadUrl(string blobName, TimeSpan validFor);
        Task DeleteAsync(string blobName);
    }
}