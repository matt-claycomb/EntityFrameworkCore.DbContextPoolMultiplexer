using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.DbContextPoolMultiplexer
{
    public class DbContextPoolMultiplexerServiceBuilder<T> where T : DbContext
    {
        private readonly Dictionary<string, Action<IServiceProvider, DbContextOptionsBuilder>>
            _connectionDetails = new Dictionary<string, Action<IServiceProvider, DbContextOptionsBuilder>>();

        private readonly IServiceCollection _serviceCollection;

        internal DbContextPoolMultiplexerServiceBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        /// <summary>
        /// Adds a set of connection details to the service builder. Each call of this method equates to
        /// registering one instance of the <see cref="DbContext" />.
        /// </summary>
        /// <param name="contextName">The name which will be used to retrieve the <see cref="DbContext" />.</param>
        /// <param name="optionsAction">
        /// A delegate for configuring the <see cref="DbContext" /> instances.
        /// This is the same as if manually registering a <see cref="DbContext" /> with the service provider.
        /// </param>
        /// <returns>The current instance of <see cref="DbContextPoolMultiplexerServiceBuilder{T}" />.</returns>
        public DbContextPoolMultiplexerServiceBuilder<T> AddConnectionDetails(string contextName,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
        {
            _connectionDetails.Add(contextName,
                    optionsAction);

            return this;
        }

        /// <summary>
        /// Denote that no more connection details will be added, to build the multiplexer
        /// and register it in the service container.
        /// </summary>
        /// <returns>An instance of <see cref="IServiceCollection" />.</returns>
        public IServiceCollection FinishRegisteringDbContextPoolMultiplexerService()
        {
            var dbContextPoolMultiplexerService =
                new DbContextPoolMultiplexerService<T>(_serviceCollection, _connectionDetails);

            return _serviceCollection.AddSingleton(dbContextPoolMultiplexerService);
        }
    }
}
