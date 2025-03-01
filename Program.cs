using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        var context = new MyDbContext(loggerFactory);
        context.Database.EnsureCreated();
        InitializeData(context);

        Console.WriteLine("All posts:");
        var data = context.BlogPosts
            .AsNoTracking()
            .Select(x => x.Title)
            .ToList();

        Console.WriteLine(JsonSerializer.Serialize(data));


        Console.WriteLine("How many comments each user left:");

        var NumberOfCommentsPerUser = context.BlogComments
            .AsNoTracking()
            .GroupBy(c => c.UserName)
            .Select(group => new { name = group.Key, count = group.Count() })
            .ToList();

        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 4
        // Petr: 2
        // Elena: 3

        Console.WriteLine("Posts ordered by date of last comment. Result should include text of last comment:");

        var PostsOrderedByLastCommentDate = context.BlogPosts
            .AsNoTracking()
            .Where(p => p.Comments.Any())
            .Select(p => new
            {
                post = p.Title,
                lastComment = p.Comments
                    .OrderByDescending(c => c.CreatedDate)
                    .Select(l => new
                    {
                        date = l.CreatedDate,
                        text = l.Text
                    })
                    .FirstOrDefault(),
            })
            .OrderByDescending(p => p.lastComment.date)
            .ToList();

        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Post2: '2020-03-06', '4'
        // Post1: '2020-03-05', '8'
        // Post3: '2020-02-14', '9'


        Console.WriteLine("How many last comments each user left:");

        var NumberOfLastCommentsLeftByUser = context.BlogPosts
            .AsNoTracking()
            .Where(p => p.Comments.Any())
            .Select(p => p.Comments
                .OrderByDescending(c => c.CreatedDate)
                .FirstOrDefault().UserName)
            .GroupBy(user => user)
            .Select(group => new { name = group.Key, count = group.Count() })
            .ToList();

        // 'last comment' is the latest Comment in each Post
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 2
        // Petr: 1


        Console.WriteLine("How many comments each user left:");
        foreach (var entry in NumberOfCommentsPerUser)
        {
            Console.WriteLine($"{entry.name}: {entry.count}");
        }

        Console.WriteLine("\nPosts ordered by date of last comment. Result should include text of last comment:");
        foreach (var entry in PostsOrderedByLastCommentDate)
        {
            Console.WriteLine($"{entry.post}: '{entry.lastComment.date:yyyy-MM-dd}', '{entry.lastComment.text}'");
        }

        Console.WriteLine("\nHow many last comments each user left:");
        foreach (var entry in NumberOfLastCommentsLeftByUser)
        {
            Console.WriteLine($"{entry.name}: {entry.count}");
        }

    }

    private static void InitializeData(MyDbContext context)
    {
        context.BlogPosts.Add(new BlogPost("Post1")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("1", new DateTime(2020, 3, 2), "Petr"),
                new BlogComment("2", new DateTime(2020, 3, 4), "Elena"),
                new BlogComment("8", new DateTime(2020, 3, 5), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post2")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("3", new DateTime(2020, 3, 5), "Elena"),
                new BlogComment("4", new DateTime(2020, 3, 6), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post3")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("5", new DateTime(2020, 2, 7), "Ivan"),
                new BlogComment("6", new DateTime(2020, 2, 9), "Elena"),
                new BlogComment("7", new DateTime(2020, 2, 10), "Ivan"),
                new BlogComment("9", new DateTime(2020, 2, 14), "Petr"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post4")
        {
            Comments = new List<BlogComment>()
            {

            }
        });
        context.SaveChanges();
    }
}