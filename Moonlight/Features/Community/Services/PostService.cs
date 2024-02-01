using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Event;
using Moonlight.Core.Extensions;
using Moonlight.Features.Community.Entities;
using Moonlight.Features.Community.Entities.Enums;

namespace Moonlight.Features.Community.Services;

[Scoped]
public class PostService
{
    private readonly Repository<Post> PostRepository;
    private readonly Repository<PostLike> PostLikeRepository;
    private readonly Repository<PostComment> PostCommentRepository;
    private readonly Repository<WordFilter> WordFilterRepository;

    public PostService(
        Repository<Post> postRepository,
        Repository<PostLike> postLikeRepository,
        Repository<PostComment> postCommentRepository,
        Repository<WordFilter> wordFilterRepository)
    {
        PostRepository = postRepository;
        PostLikeRepository = postLikeRepository;
        PostCommentRepository = postCommentRepository;
        WordFilterRepository = wordFilterRepository;
    }

    // Posts
    public async Task<Post> Create(User user, string title, string content, PostType type)
    {
        if(await CheckTextForBadWords(title))
            throw new DisplayException("Bad word detected. Please follow the community rules");
        
        if(await CheckTextForBadWords(content))
            throw new DisplayException("Bad word detected. Please follow the community rules");
        
        var post = new Post()
        {
            Author = user,
            Title = title,
            Content = content,
            Type = type
        };

        var finishedPost = PostRepository.Add(post);

        await Events.OnPostCreated.InvokeAsync(finishedPost);
        
        return finishedPost;
    }

    public async Task Update(Post post, string title, string content)
    {
        if(await CheckTextForBadWords(title))
            throw new DisplayException("Bad word detected. Please follow the community rules");
        
        if(await CheckTextForBadWords(content))
            throw new DisplayException("Bad word detected. Please follow the community rules");
        
        post.Title = title;
        post.Content = content;
        post.UpdatedAt = DateTime.UtcNow;
        
        PostRepository.Update(post);

        await Events.OnPostUpdated.InvokeAsync(post);
    }

    public async Task Delete(Post post)
    {
        var postWithData = PostRepository
            .Get()
            .Include(x => x.Comments)
            .Include(x => x.Likes)
            .First(x => x.Id == post.Id);
        
        // Cache relational data to delete later on
        var likes = postWithData.Likes.ToArray();
        var comments = postWithData.Comments.ToArray();
        
        // Clear relations
        postWithData.Comments.Clear();
        postWithData.Likes.Clear();
        
        PostRepository.Update(postWithData);
        
        // Delete relational data
        foreach (var like in likes)
            PostLikeRepository.Delete(like);

        foreach (var comment in comments)
            PostCommentRepository.Delete(comment);
        
        // Now delete the post itself
        PostRepository.Delete(post);
        await Events.OnPostDeleted.InvokeAsync(post);
    }

    // Comments
    public async Task<PostComment> CreateComment(Post post, User user, string content)
    {
        // As the comment feature has no edit form or model to validate we do the validation here
        if (string.IsNullOrEmpty(content))
            throw new DisplayException("Comment content cannot be empty");

        if (content.Length > 1024)
            throw new DisplayException("Comment content cannot be longer than 1024 characters");

        if (!Regex.IsMatch(content, "^[ a-zA-Z0-9äöüßÄÖÜẞ,.;_\\n\\t-]+$"))
            throw new DisplayException("Illegal characters in comment content");
        
        if(await CheckTextForBadWords(content))
            throw new DisplayException("Bad word detected. Please follow the community rules");
        
        var comment = new PostComment()
        {
            Author = user,
            Content = content
        };
        
        post.Comments.Add(comment);
        PostRepository.Update(post);

        await Events.OnPostCommentCreated.InvokeAsync(comment);

        return comment;
    }

    public async Task DeleteComment(Post post, PostComment comment)
    {
        var postWithComments = PostRepository
            .Get()
            .Include(x => x.Comments)
            .First(x => x.Id == post.Id);

        var commentToRemove = postWithComments.Comments.First(x => x.Id == comment.Id);
        postWithComments.Comments.Remove(commentToRemove);
        
        PostRepository.Update(postWithComments);
        PostCommentRepository.Delete(commentToRemove);

        await Events.OnPostCommentCreated.InvokeAsync(commentToRemove);
    }
    
    // Other
    public async Task ToggleLike(Post post, User user)
    {
        var postWithLikes = PostRepository
            .Get()
            .Include(x => x.Likes)
            .ThenInclude(x => x.User)
            .First(x => x.Id == post.Id);

        var userLike = postWithLikes.Likes.FirstOrDefault(x => x.User.Id == user.Id);

        if (userLike != null) // Check if person already liked
        {
            postWithLikes.Likes.Remove(userLike);
            
            PostRepository.Update(postWithLikes);
            PostLikeRepository.Delete(userLike);
        }
        else
        {
            postWithLikes.Likes.Add(new()
            {
                User = user
            });
            
            PostRepository.Update(postWithLikes);

            await Events.OnPostLiked.InvokeAsync(postWithLikes);
        }
    }
    
    // Utils
    private Task<bool> CheckTextForBadWords(string input) // This method checks for bad words using the filters added by an admin
    {
        var filters = WordFilterRepository
            .Get()
            .Select(x => x.Filter)
            .ToArray();
        
        //TODO: Add timer for regex matching to create warnings
        
        foreach (var filter in filters)
        {
            if (Regex.IsMatch(input, filter))
                return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }
}