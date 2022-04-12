using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace EntityFrameworkCore.UnitOfWork.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUnitOfWork(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services), $"{nameof(services)} cannot be null.");
            }

            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    {
                        services.TryAddSingleton<IRepositoryFactory, UnitOfWork>();
                        services.TryAddSingleton<IUnitOfWork, UnitOfWork>();
                    }
                    break;
                case ServiceLifetime.Scoped:
                    {
                        services.TryAddScoped<IRepositoryFactory, UnitOfWork>();
                        services.TryAddScoped<IUnitOfWork, UnitOfWork>();
                    }
                    break;
                case ServiceLifetime.Transient:
                    {
                        services.TryAddTransient<IRepositoryFactory, UnitOfWork>();
                        services.TryAddTransient<IUnitOfWork, UnitOfWork>();
                    }
                    break;
                default:
                    break;
            }

            return services;
        }

        public static IServiceCollection AddUnitOfWork<T>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where T : DbContext
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services), $"{nameof(services)} cannot be null.");
            }

            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    {
                        services.TryAddSingleton<IRepositoryFactory<T>, UnitOfWork<T>>();
                        services.TryAddSingleton<IUnitOfWork<T>, UnitOfWork<T>>();
                    }
                    break;
                case ServiceLifetime.Scoped:
                    {
                        services.TryAddScoped<IRepositoryFactory<T>, UnitOfWork<T>>();
                        services.TryAddScoped<IUnitOfWork<T>, UnitOfWork<T>>();
                    }
                    break;
                case ServiceLifetime.Transient:
                    {
                        services.TryAddTransient<IRepositoryFactory<T>, UnitOfWork<T>>();
                        services.TryAddTransient<IUnitOfWork<T>, UnitOfWork<T>>();
                    }
                    break;
                default:
                    break;
            }

            return services;
        }

        public static IServiceCollection AddPooledUnitOfWork<T>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where T : DbContext
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services), $"{nameof(services)} cannot be null.");
            }

            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    {
                        services.TryAddSingleton<IRepositoryFactory<T>, PooledUnitOfWork<T>>();
                        services.TryAddSingleton<IUnitOfWork<T>, PooledUnitOfWork<T>>();
                        services.TryAddSingleton<IPooledUnitOfWork<T>, PooledUnitOfWork<T>>();
                    }
                    break;
                case ServiceLifetime.Scoped:
                    {
                        services.TryAddScoped<IRepositoryFactory<T>, PooledUnitOfWork<T>>();
                        services.TryAddScoped<IUnitOfWork<T>, PooledUnitOfWork<T>>();
                        services.TryAddScoped<IPooledUnitOfWork<T>, PooledUnitOfWork<T>>();
                    }
                    break;
                case ServiceLifetime.Transient:
                    {
                        services.TryAddTransient<IRepositoryFactory<T>, PooledUnitOfWork<T>>();
                        services.TryAddTransient<IUnitOfWork<T>, PooledUnitOfWork<T>>();
                        services.TryAddTransient<IPooledUnitOfWork<T>, PooledUnitOfWork<T>>();
                    }
                    break;
                default:
                    break;
            }

            return services;
        }
    }
}
