using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.DbContextPoolMultiplexer
{
    public class DbContextPoolMultiplexerService<T> where T : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _typeMappings = new Dictionary<string, Type>();

        internal DbContextPoolMultiplexerService(IServiceCollection services,
            Dictionary<string, Action<IServiceProvider, DbContextOptionsBuilder>> connectionDetails)
        {
            foreach (var d in connectionDetails)
            {
                var contextType = BuildContextType(d.Key);

                void OptionsAction(IServiceProvider provider, DbContextOptionsBuilder builder)
                {
                    d.Value(provider, builder);
                }

                typeof(EntityFrameworkServiceCollectionExtensions).GetMethods().First(m =>
                        m.Name == "AddDbContextPool" && m.GetGenericArguments().Length == 1 && m.GetParameters()
                            .First(p => p.Name == "optionsAction").ParameterType.GenericTypeArguments.Length == 2)
                    .MakeGenericMethod(contextType).Invoke(null,
                        new object[]
                            {services, (Action<IServiceProvider, DbContextOptionsBuilder>) OptionsAction, 1200});

                _typeMappings.Add(d.Key, contextType);
            }

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Retrieve one instance of <see cref="DbContext" /> name by the name it was registered under.
        /// </summary>
        /// <param name="name">The name of the <see cref="DbContext" /> to retrieve.</param>
        /// <returns>An instance of the class extending <see cref="DbContext" />.</returns>
        public T GetDbContext(string name) => (T)_serviceProvider.GetService(_typeMappings[name]);

        /// <summary>
        /// Retrieve one instance of <see cref="DbContext" /> for each registered set of connection details.
        /// </summary>
        /// <returns>A dictionary containing registered names and DbContexts</returns>
        public Dictionary<string, T> GetAllContexts() => _typeMappings.Select(m => new KeyValuePair<string, T>(m.Key, (T)_serviceProvider.GetService(m.Value))) as Dictionary<string, T>;

        /// <summary>
        /// Retrieve the name of all registered sets of connection details.
        /// </summary>
        /// <returns>An instance of <see cref="IEnumerable{T}." /></returns>
        public IEnumerable<string> GetContextNames() => _typeMappings.Keys;

        private Type BuildContextType(string name)
        {
            var typeBuilder =
                AssemblyBuilder
                    .DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run)
                    .DefineDynamicModule("core").DefineType($"DbContext_{GenerateSlug(name)}");

            var parentType = typeof(T);

            var parentConstructor = parentType.GetConstructor(new[] { typeof(DbContextOptions) });

            typeBuilder.SetParent(parentType);

            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard,
                new[] { typeof(DbContextOptions) });

            var constructorIlGenerator = constructorBuilder.GetILGenerator();
            constructorIlGenerator.Emit(OpCodes.Ldarg_0);
            constructorIlGenerator.Emit(OpCodes.Ldarg_1);
            constructorIlGenerator.Emit(OpCodes.Call, parentConstructor);
            constructorIlGenerator.Emit(OpCodes.Ret);

            return typeBuilder.CreateType();
        }
        private string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }
    }

}
