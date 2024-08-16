using BookApplicationApi.Interface;
using BookApplicationApi.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BookApplicationApi
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddBookServices(this IServiceCollection services)
        {
            // Đăng ký BookRepositories với interface IBookService
            services.AddScoped<IBookService, BookRespositories>();

            // Thêm các dịch vụ khác nếu cần
            return services;
        }
    }
}
