using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Auth;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace homeapp73.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        #region Set up configuration stuff
        private readonly IConfiguration _configuration;
        public WeatherController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        [HttpGet]
        public IActionResult Get()
        {
            //TODO: Error handling
            string storageUri = string.Format("https://{0}.blob.core.windows.net", _configuration.GetValue<string>("AppSettings:StorageAccountName"));
            // Prepare Azure blob
            var credentials = new StorageCredentials(_configuration.GetValue<string>("AppSettings:SasToken"));
            var blobClient = new CloudBlobClient(new Uri(storageUri), credentials);
            var container = blobClient.GetContainerReference(_configuration.GetValue<string>("AppSettings:ContainerName"));
            var blob = container.GetBlobReference(_configuration.GetValue<string>("AppSettings:BlobPath"));

            // Read json file and build a json array
            var jsonArray = new JArray();
            using (var stream = blob.OpenRead())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        JObject o = JObject.Parse(reader.ReadLine());
                        jsonArray.Add(o);
                    }
                }
            }
            return Content(jsonArray.ToString(), "application/json");
        }
    }
}
