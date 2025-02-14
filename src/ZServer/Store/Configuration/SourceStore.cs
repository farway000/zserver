using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZMap.Source;

namespace ZServer.Store.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class SourceStore : ISourceStore
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ServerOptions _options;
        private readonly ILogger<SourceStore> _logger;

        private static readonly ConcurrentDictionary<Type, ParameterInfo[]>
            StorageTypeCache =
                new();

        public SourceStore(IConfiguration configuration, IMemoryCache cache, IOptionsMonitor<ServerOptions> options,
            ILogger<SourceStore> logger)
        {
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            _options = options.CurrentValue;
        }

        /// <summary>
        /// 不需要缓存，Source 直接存于 Layer 中，Layer 会缓存
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<ISource> FindAsync(string name)
        {
            // TODO: 是否需要 COPY 对象
            return string.IsNullOrWhiteSpace(name)
                ? null
                : (await _cache.GetOrCreateAsync($"{GetType().FullName}:{name}", entry =>
                {
                    var source = Get(name);
                    entry.SetValue(source);
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.ConfigurationCacheTtl));
                    return Task.FromResult(source);
                })) ;
            
        }

        public async Task<List<ISource>> GetAllAsync()
        {
            var result = new List<ISource>();
            foreach (var child in _configuration.GetSection("sources").GetChildren())
            {
                result.Add(await FindAsync(child.Key));
            }

            return result;
        }

        private ISource Get(string name)
        {
            var key = $"sources:{name}";
            var section = _configuration.GetSection(key);

            var provider = section.GetSection("provider").Get<string>();
            if (string.IsNullOrWhiteSpace(provider))
            {
                _logger.LogError($"数据源 {name} 不存在或驱动为空");
                return null;
            }

            var type = Type.GetType(provider);
            if (type == null)
            {
                _logger.LogError($"数据源 {name} 的驱动 {provider} 不存在");
                return null;
            }

            var parameterInfos = StorageTypeCache.GetOrAdd(type, x =>
            {
                if (!typeof(IVectorSource).IsAssignableFrom(x))
                {
                    _logger.LogError($"数据源 {name} 的驱动 {provider} 不是有效的驱动");
                    return null;
                }

                var result = x.GetConstructors()[0].GetParameters();

                return result;
            });

            ISource source;

            if (parameterInfos.Length == 0)
            {
                source = (ISource)Activator.CreateInstance(type);
            }
            else
            {
                var args = new object[parameterInfos.Length];
                for (var i = 0; i < args.Length; ++i)
                {
                    args[i] = section.GetSection(parameterInfos[i].Name).Get(parameterInfos[i].ParameterType);
                }

                source = (ISource)Activator.CreateInstance(type, args);
            }

            return source;
        }
    }
}