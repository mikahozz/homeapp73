using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System;
using Microsoft.Extensions.Logging;

namespace homeapp73.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClearCache : ControllerBase
    {
        #region Set up configuration stuff
        private readonly IConfiguration _configuration;
        private IMemoryCache _cache;
        private readonly ILogger _logger;
        public ClearCache(IConfiguration configuration, IMemoryCache memoryCache, ILogger<ClearCache> logger)
        {
            _configuration = configuration;
            _cache = memoryCache;
            _logger = logger;
        }

        #endregion

        [HttpGet]
        public IActionResult Get()
        {
            string cacheKey = _configuration.GetValue<string>("AppSettings:CacheKey");
            _cache.Remove(cacheKey);
            _logger.LogInformation("Cache cleared");

            return StatusCode(200);        
        }
    }
}
