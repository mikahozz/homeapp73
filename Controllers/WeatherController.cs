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
            string storageAccountName = _configuration.GetValue<string>("AppSettings:StorageAccountName");
            string containerName = _configuration.GetValue<string>("AppSettings:ContainerName");
            string sasToken = _configuration.GetValue<string>("AppSettings:SasToken");
            //TODO: Error handling
            string storageUri = string.Format("https://{0}.blob.core.windows.net/{1}", storageAccountName, containerName);
            // Prepare Azure blob
            //var credentials = new StorageSharedKeyCredential(storageAccountName, sasToken);
            var containerClient = new BlobContainerClient(new Uri(storageUri + "?" + sasToken), null);

            // Read each json file and build a json array
            var jsonArray = new JArray();
            var monthFolder = "month=" + DateTime.Now.ToString("yyyy-MM");
            var col = containerClient.GetBlobsByHierarchy(prefix: "weatherdata.json/" + monthFolder + "/");
            Console.WriteLine("blobs: " + col.Count<BlobHierarchyItem>());
            foreach (BlobHierarchyItem blob in col.Where(item => item.IsBlob))
            {
               if(!blob.Blob.Name.EndsWith(".json"))
                    continue;   // Skip non-json files

                Console.WriteLine("blob.Blob.Name: " + blob.Blob.Name);
                var blobClient = containerClient.GetBlobClient(blob.Blob.Name);
                using (BlobDownloadInfo download = blobClient.Download())
                using (StreamReader reader = new StreamReader(download.Content))
                while (!reader.EndOfStream)
                {
                    JObject o = JObject.Parse(reader.ReadLine());
                    jsonArray.Add(o);
                }
            }
            return Content(jsonArray.ToString(), "application/json");
        }
    }
}
