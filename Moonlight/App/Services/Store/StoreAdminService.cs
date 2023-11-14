using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Database.Enums;
using Moonlight.App.Exceptions;
using Moonlight.App.Repositories;
using Moonlight.App.Services.ServiceManage;
using Newtonsoft.Json;

namespace Moonlight.App.Services.Store;

public class StoreAdminService
{
    private readonly Repository<Product> ProductRepository;
    private readonly Repository<Category> CategoryRepository;
    private readonly ServiceService ServiceService;

    public StoreAdminService(
        Repository<Product> productRepository,
        Repository<Category> categoryRepository,
        ServiceService serviceService)
    {
        ProductRepository = productRepository;
        CategoryRepository = categoryRepository;
        ServiceService = serviceService;
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
        //TODO: Implement checks if services with that product id exist
        
        ProductRepository.Delete(product);
        
        return Task.CompletedTask;
    }
    
    // Product config
    public Type GetProductConfigType(ServiceType type)
    {
        try
        {
            var impl = ServiceService.Type.Get(type);
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
        var impl = ServiceService.Type.Get(product.Type);
        
        return JsonConvert.DeserializeObject(product.ConfigJson, impl.ConfigType) ??
               CreateNewProductConfig(product.Type);
    }

    public void SaveProductConfig(Product product, object config)
    {
        product.ConfigJson = JsonConvert.SerializeObject(config);
        ProductRepository.Update(product);
    }
}