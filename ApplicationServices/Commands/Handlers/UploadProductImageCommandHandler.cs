using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using MediatR;

namespace ApplicationServices.Commands.Handlers
{
    public class UploadProductImageCommandHandler(IWriteProductRepository products, IImageStorage images)
        : IRequestHandler<UploadProductImageCommand, string>
    {
        private const long MaxBytes = 5 * 1024 * 1024;   // 5 MB

        public async Task<string> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
        {
            if (!request.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Only image files are allowed.");
            if (request.Length is 0 or > MaxBytes)
                throw new ValidationException("Image must be between 1 byte and 5 MB.");

            var product = await products.GetByIdAsync(request.ProductId)
                ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

            string blobName = await images.UploadAsync(product.Id, request.Content, request.FileName, request.ContentType);

            string? oldBlob = product.ImageBlobName;
            product.ImageBlobName = blobName;
            await products.SaveChangesAsync();

            if (oldBlob is not null)
                await images.DeleteAsync(oldBlob);   // don't leave orphaned blobs

            return blobName;
        }
    }
}