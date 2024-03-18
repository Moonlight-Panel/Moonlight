using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Exceptions;


using Moonlight.Features.ServiceManagement.Entities;
using Moonlight.Features.ServiceManagement.Entities.Enums;
using Moonlight.Features.ServiceManagement.Services;
using Moonlight.Features.StoreSystem.Entities;
using Newtonsoft.Json;

namespace Moonlight.Features.StoreSystem.Services;

[Scoped]
public class StoreAdminService
{
    private readonly Repository<Product> ProductRepository;
    private readonly Repository<Category> CategoryRepository;
    private readonly Repository<Service> ServiceRepository;
    private readonly ServiceService ServiceService;

    public StoreAdminService(
        Repository<Product> productRepository,
        Repository<Category> categoryRepository,
        ServiceService serviceService,
        Repository<Service> serviceRepository)
    {
        ProductRepository = productRepository;
        CategoryRepository = categoryRepository;
        ServiceService = serviceService;
        ServiceRepository = serviceRepository;
    }

    public Task<Category> AddCategory(string name, string description, string slug)
    {
        if (CategoryRepository.Get().Any(x => x.Slug == slug))
            throw new DisplayException("A category with this slug does already exist");

        var result = CategoryRepository.Add(new Category()
        {
            Name = name,
            Description = description,
            Slug = slug
        });

        return Task.FromResult(result);
    }

    public Task<Product> AddProduct(string name, string description, string slug, ServiceType type, Action<Product>? modifyProduct = null)
    {
        if (ProductRepository.Get().Any(x => x.Slug == slug))
            throw new DisplayException("A product with that slug does already exist");

        var product = new Product()
        {
            Name = name,
            Description = description,
            Slug = slug,
            Type = type,
            ConfigJson = "{}"
        };
        
        if(modifyProduct != null)
            modifyProduct.Invoke(product);

        var result = ProductRepository.Add(product);
        
        return Task.FromResult(result);
    }

    public Task UpdateCategory(Category category)
    {
        if (CategoryRepository.Get().Any(x => x.Id != category.Id && x.Slug == category.Slug))
            throw new DisplayException("A category with that slug does already exist");
        
        CategoryRepository.Update(category);
        
        return Task.CompletedTask;
    }
    
    public Task UpdateProduct(Product product)
    {
        if (ProductRepository.Get().Any(x => x.Id != product.Id && x.Slug == product.Slug))
            throw new DisplayException("A product with that slug does already exist");

        ProductRepository.Update(product);
        
        return Task.CompletedTask;
    }

    public Task DeleteCategory(Category category)
    {
        var hasProductsInIt = ProductRepository
            .Get()
            .Any(x => x.Category!.Id == category.Id);

        if (hasProductsInIt)
            throw new DisplayException("The category contains products. You need to delete the products in order to delete the category");
        
        CategoryRepository.Delete(category);
        
        return Task.CompletedTask;
    }

    public Task DeleteProduct(Product product)
    {
        if (ServiceRepository.Get().Any(x => x.Product.Id == product.Id))
            throw new DisplayException("Product cannot be deleted as services related to this products exist. Delete the services first");
        
        ProductRepository.Delete(product);
        
        return Task.CompletedTask;
    }
    
    // Product config
    public Type GetProductConfigType(ServiceType type)
    {
        try
        {
            var impl = ServiceService.Definition.Get(type);
            return impl.ConfigType;
        }
        catch (ArgumentException)
        {
            return typeof(object);
        }
    }
    public object CreateNewProductConfig(ServiceType type)
    {
        var config = Activator.CreateInstance(GetProductConfigType(type))!;
        return config;
    }
    public object GetProductConfig(Product product)
    {
        var impl = ServiceService.Definition.Get(product.Type);
        
        return JsonConvert.DeserializeObject(product.ConfigJson, impl.ConfigType) ??
               CreateNewProductConfig(product.Type);
    }

    public void SaveProductConfig(Product product, object config)
    {
        product.ConfigJson = JsonConvert.SerializeObject(config);
        ProductRepository.Update(product);
    }
}