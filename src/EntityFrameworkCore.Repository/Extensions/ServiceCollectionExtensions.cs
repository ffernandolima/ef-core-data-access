using EntityFrameworkCore.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace EntityFrameworkCore.Repository.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomRepository<TService, TImplementation>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TService : class, IRepository
            where TImplementation : class, TService
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services), $"{nameof(services)} cannot be null.");
            }

            if (!(typeof(TImplementation).BaseType?.IsGenericType(typeof(Repository<>)) ?? false))
            {
                throw new ArgumentException("Implementation constraint has not been satisfied.");
            }

            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    {
                        services.TryAddSingleton<TService, TImplementation>();
                    }
                    break;
                case ServiceLifetime.Scoped:
                    {
                        services.TryAddScoped<TService, TImplementation>();
                    }
                    break;
                case ServiceLifetime.Transient:
                    {
                        services.TryAddTransient<TService, TImplementation>();
                    }
                    break;
                default:
                    break;
            }

            return services;
        }

        public static IServiceCollection AddRepository<TService, TImplementation>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TService : class, IRepository
            where TImplementation : class, TService
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services), $"{nameof(services)} cannot be null.");
            }

            if (!typeof(TImplementation).IsGenericType(typeof(Repository<>)))
            {
                throw new ArgumentException("Implementation constraint has not been satisfied.");
            }

            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    {
                        services.TryAddSingleton<TService, TImplementation>();
                    }
                    break;
                case ServiceLifetime.Scoped:
                    {
                        services.TryAddScoped<TService, TImplementation>();
                    }
                    break;
                case ServiceLifetime.Transient:
                    {
                        services.TryAddTransient<TService, TImplementation>();
                    }
                    break;
                default:
                    break;
            }

            return services;
        }
    }
}
