using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Community;
using Moonlight.App.Database.Enums;
using Moonlight.App.Event;
using Moonlight.App.Exceptions;
using Moonlight.App.Extensions;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Community;

public class PostService
{
    private readonly Repository<Post> PostRepository;
    private readonly Repository<PostLike> PostLikeRepository;
    private readonly Repository<PostComment> PostCommentRepository;

    public PostService(Repository<Post> postRepository, Repository<PostLike> postLikeRepository, Repository<PostComment> postCommentRepository)
    {
        PostRepository = postRepository;
        PostLikeRepository = postLikeRepository;
        PostCommentRepository = postCommentRepository;
    }

    // Posts
    public async Task<Post> Create(User user, string title, string content, PostType type)
    {
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
        post.Title = title;
        post.Content = content;
        
        PostRepository.Update(post);

        await Events.OnPostUpdated.InvokeAsync(post);
    }

    public async Task Delete(Post post)
    {
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

        if (!Regex.IsMatch(content, "^[a-zA-Z0-9äöüßÄÖÜẞ,.;_\\n\\t-]+$"))
            throw new DisplayException("Illegal characters in comment content");
        
        //TODO: Swear word filter
        
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
}