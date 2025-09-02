using Ecommerce.Application.Models.Authorization;
using Ecommerce.Domain;
using Ecommerce.Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Ecommerce.Infrastructure.Persistence;

public class EcommerceDbContextData
{
    public static async Task LoadDataAsync(
        EcommerceDbContext context,
        UserManager<Usuario> usuarioManager,
        RoleManager<IdentityRole> roleManager,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<EcommerceDbContextData>();
        
        try
        {
            // ✅ CREAR ROLES
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(Role.Admin));
                await roleManager.CreateAsync(new IdentityRole(Role.User));
                logger.LogInformation("Roles creados exitosamente");
            }

            // ✅ CREAR USUARIOS (CORREGIDO - eliminar duplicación de verificación)
            if (!usuarioManager.Users.Any())
            {
                // Primer usuario - Admin
                var usuarioAdmin = new Usuario
                {
                    Nombre = "Luciano",
                    Apellido = "Balot",
                    Email = "luchobalot@example.com",
                    UserName = "luchobalot",
                    Telefono = "123456789",
                    AvatarUrl = "https://images.pexels.com/photos/220453/pexels-photo-220453.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2"
                };

                var resultAdmin = await usuarioManager.CreateAsync(usuarioAdmin, "luchoS1c0$");
                if (resultAdmin.Succeeded)
                {
                    await usuarioManager.AddToRoleAsync(usuarioAdmin, Role.Admin);
                    logger.LogInformation("Usuario admin Luciano creado exitosamente");
                }
                else
                {
                    logger.LogError("Error creando usuario admin: {Errors}", 
                        string.Join(", ", resultAdmin.Errors.Select(e => e.Description)));
                }

                // ✅ SEGUNDO USUARIO (SIN VERIFICAR Users.Any() de nuevo)
                var usuario = new Usuario
                {
                    Nombre = "Nicolas",
                    Apellido = "Gomez",
                    Email = "nicogomez@example.com",
                    UserName = "nicogomez",
                    Telefono = "987654321",
                    AvatarUrl = "https://images.pexels.com/photos/220452/pexels-photo-220452.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2"
                };

                var resultUser = await usuarioManager.CreateAsync(usuario, "luchoS1c0$");
                if (resultUser.Succeeded)
                {
                    // ✅ CORREGIDO: Asignar Role.User en lugar de Role.Admin
                    await usuarioManager.AddToRoleAsync(usuario, Role.User);
                    logger.LogInformation("Usuario regular Nicolas creado exitosamente");
                }
                else
                {
                    logger.LogError("Error creando usuario regular: {Errors}", 
                        string.Join(", ", resultUser.Errors.Select(e => e.Description)));
                }
            }

            // ✅ CARGAR CATEGORÍAS (con ruta corregida y manejo de errores)
            if (!context.Categories!.Any())
            {
                try
                {
                    // Intentar diferentes rutas posibles
                    string categoryFile = "";
                    var possiblePaths = new[]
                    {
                        "Data/category.json",
                        "../Infrastructure/Data/category.json",
                        "Infrastructure/Data/category.json"
                    };

                    foreach (var path in possiblePaths)
                    {
                        if (File.Exists(path))
                        {
                            categoryFile = path;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(categoryFile))
                    {
                        var categoryData = await File.ReadAllTextAsync(categoryFile);
                        var categoriesJson = JsonConvert.DeserializeObject<List<dynamic>>(categoryData);
                        
                        var categories = categoriesJson!.Select(c => new Category 
                        { 
                            Id = Guid.NewGuid(),
                            Nombre = c.nombre?.ToString(),
                            CreatedDate = DateTime.Now,
                            CreatedBy = "system"
                        }).ToList();

                        await context.Categories!.AddRangeAsync(categories);
                        await context.SaveChangesAsync();
                        logger.LogInformation($"Cargadas {categories.Count} categorías desde {categoryFile}");
                    }
                    else
                    {
                        logger.LogWarning("Archivo category.json no encontrado en ninguna ubicación");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error cargando categorías desde archivo JSON");
                }
            }

            // ✅ CARGAR PRODUCTOS (con CategoryId válido)
            if (!context.Products!.Any())
            {
                try
                {
                    string productFile = "";
                    var possiblePaths = new[]
                    {
                        "Data/product.json",
                        "../Infrastructure/Data/product.json",
                        "Infrastructure/Data/product.json"
                    };

                    foreach (var path in possiblePaths)
                    {
                        if (File.Exists(path))
                        {
                            productFile = path;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(productFile))
                    {
                        // ✅ OBTENER CATEGORÍA VÁLIDA PRIMERO
                        var firstCategory = await context.Categories!.FirstOrDefaultAsync();
                        if (firstCategory == null)
                        {
                            logger.LogWarning("No hay categorías disponibles para asignar a los productos");
                            return;
                        }

                        var productData = await File.ReadAllTextAsync(productFile);
                        var productsJson = JsonConvert.DeserializeObject<List<dynamic>>(productData);
                        
                        var products = productsJson!.Select(p => new Product
                        {
                            Id = Guid.NewGuid(),
                            Nombre = p.nombre?.ToString(),
                            Precio = Convert.ToDecimal(p.precio),
                            Descripcion = p.descripcion?.ToString(),
                            Rating = Convert.ToInt32(p.rating ?? 0),
                            Vendedor = p.vendedor?.ToString(),
                            Stock = Convert.ToInt32(p.stock ?? 0),
                            CategoryId = firstCategory.Id, // ✅ USAR CATEGORYID VÁLIDO
                            Status = ProductStatus.Activo,
                            CreatedDate = DateTime.Now,
                            CreatedBy = "system"
                        }).ToList();

                        await context.Products!.AddRangeAsync(products);
                        await context.SaveChangesAsync();
                        logger.LogInformation($"Cargados {products.Count} productos desde {productFile}");
                    }
                    else
                    {
                        logger.LogWarning("Archivo product.json no encontrado en ninguna ubicación");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error cargando productos desde archivo JSON");
                }
            }

            // ✅ CARGAR REVIEWS (con ProductId válido)
            if (!context.Reviews!.Any())
            {
                try
                {
                    string reviewFile = "";
                    var possiblePaths = new[]
                    {
                        "Data/review.json",
                        "../Infrastructure/Data/review.json",
                        "Infrastructure/Data/review.json"
                    };

                    foreach (var path in possiblePaths)
                    {
                        if (File.Exists(path))
                        {
                            reviewFile = path;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(reviewFile))
                    {
                        // ✅ OBTENER PRODUCTOS VÁLIDOS
                        var products = await context.Products!.Take(10).ToListAsync();
                        if (!products.Any())
                        {
                            logger.LogWarning("No hay productos disponibles para asignar reviews");
                            return;
                        }

                        var reviewData = await File.ReadAllTextAsync(reviewFile);
                        var reviewsJson = JsonConvert.DeserializeObject<List<dynamic>>(reviewData);
                        
                        var reviews = new List<Review>();
                        for (int i = 0; i < reviewsJson!.Count && i < products.Count; i++)
                        {
                            var r = reviewsJson[i];
                            reviews.Add(new Review
                            {
                                Id = Guid.NewGuid(),
                                Nombre = r.nombre?.ToString(),
                                Rating = Convert.ToInt32(r.rating ?? 5),
                                Comentario = r.comentario?.ToString(),
                                ProductId = products[i].Id, // ✅ USAR PRODUCTID VÁLIDO
                                CreatedDate = DateTime.Now,
                                CreatedBy = "system"
                            });
                        }

                        await context.Reviews!.AddRangeAsync(reviews);
                        await context.SaveChangesAsync();
                        logger.LogInformation($"Cargadas {reviews.Count} reviews desde {reviewFile}");
                    }
                    else
                    {
                        logger.LogWarning("Archivo review.json no encontrado en ninguna ubicación");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error cargando reviews desde archivo JSON");
                }
            }

            // ✅ CARGAR PAÍSES
            if (!context.Countries!.Any())
            {
                try
                {
                    string countryFile = "";
                    var possiblePaths = new[]
                    {
                        "Data/countries.json",
                        "../Infrastructure/Data/countries.json",
                        "Infrastructure/Data/countries.json"
                    };

                    foreach (var path in possiblePaths)
                    {
                        if (File.Exists(path))
                        {
                            countryFile = path;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(countryFile))
                    {
                        var countryData = await File.ReadAllTextAsync(countryFile);
                        var countriesJson = JsonConvert.DeserializeObject<List<dynamic>>(countryData);
                        
                        var countries = countriesJson!.Select(c => new Country
                        {
                            Id = Guid.NewGuid(),
                            Nombre = c.name?.ToString(),
                            Iso2 = c.iso2?.ToString(),
                            Iso3 = c.iso3?.ToString(),
                            CreatedDate = DateTime.Now,
                            CreatedBy = "system"
                        }).ToList();

                        await context.Countries!.AddRangeAsync(countries);
                        await context.SaveChangesAsync();
                        logger.LogInformation($"Cargados {countries.Count} países desde {countryFile}");
                    }
                    else
                    {
                        logger.LogWarning("Archivo countries.json no encontrado en ninguna ubicación");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error cargando países desde archivo JSON");
                }
            }

            logger.LogInformation("Proceso de carga de datos completado exitosamente");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error general durante la carga de datos: {ErrorMessage}", e.Message);
            throw; // Re-lanzar para que se maneje en Program.cs
        }
    }
}