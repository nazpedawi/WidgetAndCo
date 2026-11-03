using MediatR;

namespace ApplicationServices.Commands
{
    public record UploadProductImageCommand(string ProductId, Stream Content, string FileName,
                                            string ContentType, long Length) : IRequest<string>;
}