using BookData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookData
{
    public static class BookDbContextFantory
    {
        public static IServiceCollection AddBookData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            var booksApiUrl = "https://softwium.com/api/books";
            string defaultConfifuration = "Host=host.docker.internal;Port=1807;Database=NhaSach;Username=postgres;Password=danghi";
            var defaultConnection = configuration["ConnectionStrings:DefaultConnection"] ?? defaultConfifuration;
            services.AddDbContext<BookDbContext>(options =>
                options.UseNpgsql(defaultConnection));
            services.AddSingleton(new BooksApiUrl { Link = booksApiUrl });
            return services;
        }

    }
}
