using Ecommerce.Application.Models.ImagesManagement;

namespace Ecommerce.Application.Contracts.Infraestructure;

public interface IManageImageService
{
    Task<ImageResponse> UploadImage(ImageData imageStream);
}
