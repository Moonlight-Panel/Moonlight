using Microsoft.EntityFrameworkCore;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Database.Entities.Store;
using Moonlight.Core.Repositories;
using Moonlight.Core.Services.ServiceManage;
using Moonlight.Features.Community.Entities;
using Moonlight.Features.Community.Services;
using Moonlight.Features.StoreSystem.Entities;
using Moonlight.Features.Ticketing.Entities;

namespace Moonlight.Core.Services.Users;

public class UserDeleteService
{
    private readonly Repository<Service> ServiceRepository;
    private readonly Repository<ServiceShare> ServiceShareRepository;
    private readonly Repository<Post> PostRepository;
    private readonly Repository<User> UserRepository;
    private readonly Repository<Transaction> TransactionRepository;
    private readonly Repository<CouponUse> CouponUseRepository;
    private readonly Repository<GiftCodeUse> GiftCodeUseRepository;
    private readonly Repository<Ticket> TicketRepository;
    private readonly Repository<TicketMessage> TicketMessageRepository;
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
        Repository<Transaction> transactionRepository,
        Repository<Ticket> ticketRepository,
        Repository<TicketMessage> ticketMessageRepository,
        Repository<ServiceShare> serviceShareRepository)
    {
        ServiceRepository = serviceRepository;
        ServiceService = serviceService;
        PostService = postService;
        PostRepository = postRepository;
        UserRepository = userRepository;
        GiftCodeUseRepository = giftCodeUseRepository;
        CouponUseRepository = couponUseRepository;
        TransactionRepository = transactionRepository;
        TicketRepository = ticketRepository;
        TicketMessageRepository = ticketMessageRepository;
        ServiceShareRepository = serviceShareRepository;
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
        
        // Service shares
        var shares = ServiceShareRepository
            .Get()
            .Where(x => x.User.Id == user.Id)
            .ToArray();

        foreach (var share in shares)
        {
            ServiceShareRepository.Delete(share);
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
        
        // Tickets and ticket messages

        // First we need to fetch every message this user has sent and delete it as admin accounts can have messages
        // in tickets they dont own
        var messagesFromUser = TicketMessageRepository
            .Get()
            .Where(x => x.Sender.Id == user.Id)
            .ToArray();

        foreach (var message in messagesFromUser)
        {
            TicketMessageRepository.Delete(message);
        }
        
        // Now we can only delete the tickets the user actually owns
        var tickets = TicketRepository
            .Get()
            .Include(x => x.Messages)
            .Where(x => x.Creator.Id == user.Id)
            .ToArray();

        foreach (var ticket in tickets)
        {
            var messages = ticket.Messages.ToArray(); // Cache message models
            
            ticket.Messages.Clear();
            TicketRepository.Update(ticket);
            
            foreach (var ticketMessage in messages)
            {
                TicketMessageRepository.Delete(ticketMessage);
            }
            
            TicketRepository.Delete(ticket);
        }
        
        // User
        
        // We need to use this in order to entity framework not crashing because of the previous deleted data
        var userToDelete = UserRepository.Get().First(x => x.Id == user.Id);
        UserRepository.Delete(userToDelete);
    }
}