using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.DbContextPoolMultiplexer
{
    public static class DbContextPoolMultiplexerServiceCollectionExtensions
    {
        /// <summary>
        /// Extension method to bootstrap creation of <see cref="DbContextPoolMultiplexerServiceBuilder{T}" />.
        /// </summary>
        /// <typeparam name="T">A type that extends DbContext.</typeparam>
        /// <param name="services">An instance of <see cref="IServiceCollection" />.</param>
        /// <returns>An instance of <see cref="DbContextPoolMultiplexerServiceBuilder{T}" />.</returns>
        public static DbContextPoolMultiplexerServiceBuilder<T> BeginRegisteringDbContextPoolMultiplexerService<T>(
            this IServiceCollection services) where T : DbContext
        {
            return new DbContextPoolMultiplexerServiceBuilder<T>(services);
        }
    }
}
