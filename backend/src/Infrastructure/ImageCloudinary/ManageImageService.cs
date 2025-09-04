using Ecommerce.Application.Contracts.Infraestructure;
using Ecommerce.Application.Models.ImagesManagement;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System.Net;

namespace Ecommerce.Infrastructure.ImageCloudinary;

public class ManageImageService : IManageImageService
{
    public CloudinarySettings _cloudinarySettings { get; }

    public ManageImageService(IOptions<CloudinarySettings> cloudinarySettings)
    {
        _cloudinarySettings = cloudinarySettings.Value;
    }

    // ✅ CORREGIDO: Retorna Task<ImageResponse> en lugar de Task<string>
    public async Task<ImageResponse> UploadImage(ImageData imageStream)
    {
        try
        {
            var account = new Account(
                _cloudinarySettings.CloudName,
                _cloudinarySettings.ApiKey,
                _cloudinarySettings.ApiSecret
            );

            var cloudinary = new Cloudinary(account);
            var uploadImage = new ImageUploadParams()
            {
                File = new FileDescription(imageStream.Nombre, imageStream.ImageStream)
            };

            // ✅ CORREGIDO: Agregar await
            var uploadResult = await cloudinary.UploadAsync(uploadImage);

            // ✅ CORREGIDO: Verificar StatusCode correctamente
            if (uploadResult.StatusCode == HttpStatusCode.OK)
            {
                return new ImageResponse
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.Url.ToString() // ✅ CORREGIDO: Cambié "uploadResult" por "Url"
                };
            }

            // ✅ CORREGIDO: Retornar error en lugar de lanzar excepción
            return new ImageResponse
            {
                PublicId = null,
                Url = null,
                // Agregar propiedades de error si las tienes en ImageResponse
            };
        }
        catch (Exception ex)
        {
            // ✅ MEJOR: Manejar excepción y retornar error
            return new ImageResponse
            {
                PublicId = null,
                Url = null,
                // Si tienes propiedades de error en ImageResponse:
                // Error = ex.Message,
                // IsSuccess = false
            };
        }
    }
}