using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Exceptions;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Store;

public class StoreGiftService
{
    private readonly IServiceScopeFactory ServiceScopeFactory;

    public StoreGiftService(IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
    }

    public async Task Redeem(User u, string code)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var giftCodeRepo = scope.ServiceProvider.GetRequiredService<Repository<GiftCode>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
        
        var user = userRepo
            .Get()
            .Include(x => x.GiftCodeUses)
            .ThenInclude(x => x.GiftCode)
            .FirstOrDefault(x => x.Id == u.Id);
        
        if(user == null)
            throw new DisplayException("Unsafe value detected. Please reload the page to proceed");
        
        var giftCode = giftCodeRepo
            .Get()
            .FirstOrDefault(x => x.Code == code);

        if (giftCode == null)
            throw new DisplayException("The gift code does not exist");

        if (giftCode.Amount < 1)
            throw new DisplayException("The gift code can no longer be used as it has exceeded the max use amount");

        if (user.GiftCodeUses.Any(x => x.GiftCode.Id == giftCode.Id))
            throw new DisplayException("The gift code has already been used on this account");

        giftCode.Amount--;
        giftCodeRepo.Update(giftCode);
        
        var giftCardUse = new GiftCodeUse()
        {
            GiftCode = giftCode
        };
        
        user.GiftCodeUses.Add(giftCardUse);
        userRepo.Update(user);
        
        var transactionService = scope.ServiceProvider.GetRequiredService<TransactionService>();
        await transactionService.Add(u, giftCode.Value, $"Redeemed gift code '{giftCode.Code}'");
    }
}