using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace homeapp73.Controllers
{
    [ApiController]
    [Route("[controller]")]
    
    public class WeatherNowController : ControllerBase
    {
        const string jsonStringTemplate = @"{{""datetime"": ""{0}"", ""temperature"": {1}, ""humidity"": {2} }}";

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // TODO: Error handling
            string response = string.Empty;
            string weatherUri = "https://opendata.fmi.fi/wfs?request=getFeature&storedquery_id=fmi%3A%3Aobservations%3A%3Aweather%3A%3Amultipointcoverage&crs=EPSG%3A%3A3067&fmisid=101009&parameters=temperature,N_MAN";
            var startTime = DateTime.Now.AddHours(-1).ToUniversalTime().ToString("s") + "Z";
            weatherUri += string.Format("&starttime={0}", startTime);
            using (var client = new HttpClient())
            {
                var result = await client.GetStreamAsync(weatherUri);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(result);
                var ns = new XmlNamespaceManager(xmlDoc.NameTable);
                ns.AddNamespace("gml", "http://www.opengis.net/gml/3.2");
                ns.AddNamespace("om", "http://www.opengis.net/om/2.0");

                var beginTimeNode = xmlDoc.SelectSingleNode("//om:phenomenonTime/gml:TimePeriod/gml:beginPosition", ns);
                DateTime beginTime = DateTime.Parse(beginTimeNode.InnerText);

                // Get weather values
                var node = xmlDoc.SelectSingleNode("//gml:doubleOrNilReasonTupleList", ns);
                if (node != null)
                {
                    var stringReader = new StringReader(node.InnerText);
                    JArray jsonArray = new JArray();
                    DateTime time = beginTime;
                    string line = string.Empty;
                    while ((line = stringReader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (!String.IsNullOrEmpty(line))
                        {
                            var values = line.Split(" ");
                            // TODO Error handling for array index
                            string jsonString = string.Format(jsonStringTemplate, time.ToUniversalTime().ToString("s") + "Z", values[0], values[1]);
                            Console.WriteLine(jsonString);

                            var jsonObject = JObject.Parse(jsonString);
                            jsonArray.Add(jsonObject);
                            time = time.AddMinutes(10); // Each line contains values for every 10 minutes
                        }
                    }
                    Console.WriteLine(jsonArray);
                    response = jsonArray.ToString();

                }
            }
            return Content(response, "text/plain");
        }
    }
}
