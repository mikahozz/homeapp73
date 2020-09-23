using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs.Models;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace homeapp73.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        #region Set up configuration stuff
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger; 
        private IMemoryCache _cache;
        public WeatherController(IConfiguration configuration, IMemoryCache memoryCache, ILogger<WeatherController> logger)
        {
            _configuration = configuration;
            _cache = memoryCache;
            _logger = logger;
        }
        #endregion

        [HttpGet]
        public IActionResult Get()
        {
            string result = string.Empty;
            string cacheKey = _configuration.GetValue<string>("AppSettings:CacheKey");
            if (_cache.TryGetValue<string>(cacheKey, out result))
            {
                _logger.LogInformation("Read from cache");
            }
            else
            {
                string storageAccountName = _configuration.GetValue<string>("AppSettings:StorageAccountName");
                string containerName = _configuration.GetValue<string>("AppSettings:ContainerName");
                //string sasToken = _configuration.GetValue<string>("AppSettings:SasToken");
                var vaultClient = new SecretClient(new Uri(_configuration.GetValue<string>("AppSettings:KeyVaultUri")), new DefaultAzureCredential(), null);
                string sasToken = vaultClient.GetSecret("HomeApp-Storage-SasToken").Value.Value;
                //TODO: Error handling
                string storageUri = string.Format("https://{0}.blob.core.windows.net/{1}", storageAccountName, containerName);
                // Prepare Azure blob
                //var credentials = new StorageSharedKeyCredential(storageAccountName, sasToken);
                var containerClient = new BlobContainerClient(new Uri(storageUri + "?" + sasToken), null);

                // Read each json file and build a json array
                var jsonArray = new JArray();
                var monthFolder = "month=" + DateTime.Now.ToString("yyyy-MM");
                var col = containerClient.GetBlobsByHierarchy(prefix: "weatherdata.json/" + monthFolder + "/");
                _logger.LogInformation("Blob amount: " + col.Count<BlobHierarchyItem>());
                foreach (BlobHierarchyItem blob in col.Where(item => item.IsBlob))
                {
                    if (!blob.Blob.Name.EndsWith(".json"))
                        continue;   // Skip non-json files

                    _logger.LogInformation("Json blob name: " + blob.Blob.Name);
                    var blobClient = containerClient.GetBlobClient(blob.Blob.Name);
                    using (BlobDownloadInfo download = blobClient.Download())
                    using (StreamReader reader = new StreamReader(download.Content))
                        while (!reader.EndOfStream)
                        {
                            JObject o = JObject.Parse(reader.ReadLine());
                            jsonArray.Add(o);
                        }
                }
                result = jsonArray.ToString();
                _cache.Set<string>(cacheKey, result);
                _logger.LogInformation("Added to cache");
            }
            return Content(result, "application/json");
        }
    }
}
