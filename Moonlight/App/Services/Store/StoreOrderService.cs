using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Exceptions;
using Moonlight.App.Repositories;
using Moonlight.App.Services.ServiceManage;

namespace Moonlight.App.Services.Store;

public class StoreOrderService
{
    private readonly IServiceScopeFactory ServiceScopeFactory;

    public StoreOrderService(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
    }

    public Task Validate(User u, Product p, int durationMultiplier, Coupon? c)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var productRepo = scope.ServiceProvider.GetRequiredService<Repository<Product>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
        var serviceRepo = scope.ServiceProvider.GetRequiredService<Repository<Service>>();
        var couponRepo = scope.ServiceProvider.GetRequiredService<Repository<Coupon>>();
        
        // Ensure the values are safe and loaded by using the created scope to bypass the cache
        
        var user = userRepo
            .Get()
            .Include(x => x.CouponUses)
            .ThenInclude(x => x.Coupon)
            .FirstOrDefault(x => x.Id == u.Id);

        if (user == null)
            throw new DisplayException("Unsafe value detected. Please reload the page to proceed");

        
        var product = productRepo
            .Get()
            .FirstOrDefault(x => x.Id == p.Id);
        
        if (product == null)
            throw new DisplayException("Unsafe value detected. Please reload the page to proceed");


        Coupon? coupon = c;

        if (coupon != null) // Only check if the coupon actually has a value
        {
            coupon = couponRepo
                .Get()
                .FirstOrDefault(x => x.Id == coupon.Id);
            
            if(coupon == null)
                    throw new DisplayException("Unsafe value detected. Please reload the page to proceed");
        }
        
        // Perform checks on selected order

        if (coupon != null && user.CouponUses.Any(x => x.Coupon.Id == coupon.Id))
            throw new DisplayException("Coupon already used");

        if (coupon != null && coupon.Amount == 0)
            throw new DisplayException("No coupon uses left");
        
        var price = product.Price * durationMultiplier;

        if (coupon != null)
            price = Math.Round(price * coupon.Percent / 100, 2);

        if (user.Balance < price)
            throw new DisplayException("Order is too expensive");

        var userServices = serviceRepo
            .Get()
            .Where(x => x.Product.Id == product.Id)
            .Count(x => x.Owner.Id == user.Id);

        if (userServices >= product.MaxPerUser)
            throw new DisplayException("The limit of this product on your account has been reached");

        if (product.Stock < 1)
            throw new DisplayException("The product is out of stock");
        
        return Task.CompletedTask;
    }

    public async Task<Service> Process(User u, Product p, int durationMultiplier, Coupon? c)
    {
        // Validate to ensure we dont process an illegal order
        await Validate(u, p, durationMultiplier, c);
        
        // Create scope and get required services
        using var scope = ServiceScopeFactory.CreateScope();
        var serviceService = scope.ServiceProvider.GetRequiredService<ServiceService>();
        var transactionService = scope.ServiceProvider.GetRequiredService<TransactionService>();

        // Calculate price
        var price = p.Price * durationMultiplier;

        if (c != null)
            price = Math.Round(price * c.Percent / 100, 2);

        //  Calculate duration
        var duration = durationMultiplier * p.Duration;
        
        // Add transaction
        await transactionService.Add(u, -1 * price, $"Bought product '{p.Name}' for {duration} days");

        // Create service
        return await serviceService.Admin.Create(u, p, service =>
        {
            service.RenewAt = DateTime.UtcNow.AddDays(duration);
        });
    }
}