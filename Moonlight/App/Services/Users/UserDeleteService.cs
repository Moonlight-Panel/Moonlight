using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Community;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Community;
using Moonlight.App.Services.ServiceManage;

namespace Moonlight.App.Services.Users;

public class UserDeleteService
{
    private readonly Repository<Service> ServiceRepository;
    private readonly Repository<Post> PostRepository;
    private readonly Repository<User> UserRepository;
    private readonly Repository<Transaction> TransactionRepository;
    private readonly Repository<CouponUse> CouponUseRepository;
    private readonly Repository<GiftCodeUse> GiftCodeUseRepository;
    private readonly ServiceService ServiceService;
    private readonly PostService PostService;

    public UserDeleteService(
        Repository<Service> serviceRepository,
        ServiceService serviceService,
        PostService postService,
        Repository<Post> postRepository,
        Repository<User> userRepository,
        Repository<GiftCodeUse> giftCodeUseRepository,
        Repository<CouponUse> couponUseRepository,
        Repository<Transaction> transactionRepository)
    {
        ServiceRepository = serviceRepository;
        ServiceService = serviceService;
        PostService = postService;
        PostRepository = postRepository;
        UserRepository = userRepository;
        GiftCodeUseRepository = giftCodeUseRepository;
        CouponUseRepository = couponUseRepository;
        TransactionRepository = transactionRepository;
    }

    public async Task Perform(User user)
    {
        // Community
        
        // - Posts
        foreach (var post in PostRepository.Get().ToArray())
        {
            await PostService.Delete(post);
        }

        // - Comments
        var posts = PostRepository
            .Get()
            .Where(x => x.Comments.Any(y => y.Author.Id == user.Id))
            .ToArray();

        foreach (var post in posts)
        {
            var comments = PostRepository
                .Get()
                .Include(x => x.Comments)
                .ThenInclude(x => x.Author)
                .First(x => x.Id == post.Id)
                .Comments
                .Where(x => x.Author.Id == user.Id)
                .ToArray();

            foreach (var comment in comments)
                await PostService.DeleteComment(post, comment);
        }
        
        // Services
        foreach (var service in ServiceRepository.Get().Where(x => x.Owner.Id == user.Id).ToArray())
        {
            await ServiceService.Admin.Delete(service);
        }
        
        // Transactions - Coupons - Gift codes
        var userWithDetails = UserRepository
            .Get()
            .Include(x => x.Transactions)
            .Include(x => x.CouponUses)
            .Include(x => x.GiftCodeUses)
            .First(x => x.Id == user.Id);

        var giftCodeUses = userWithDetails.GiftCodeUses.ToArray();
        var couponUses = userWithDetails.CouponUses.ToArray();
        var transactions = userWithDetails.Transactions.ToArray();

        userWithDetails.GiftCodeUses.Clear();
        userWithDetails.CouponUses.Clear();
        userWithDetails.Transactions.Clear();
        
        UserRepository.Update(userWithDetails);

        foreach (var giftCodeUse in giftCodeUses)
            GiftCodeUseRepository.Delete(giftCodeUse);

        foreach (var couponUse in couponUses)
            CouponUseRepository.Delete(couponUse);

        foreach (var transaction in transactions)
            TransactionRepository.Delete(transaction);
        
        // User
        
        // We need to use this in order to entity framework not crashing because of the previous deleted data
        var userToDelete = UserRepository.Get().First(x => x.Id == user.Id);
        UserRepository.Delete(userToDelete);
    }
}