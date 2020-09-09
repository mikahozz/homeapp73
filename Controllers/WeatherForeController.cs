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
    public class WeatherForeController : ControllerBase
    {
        const string forecastUri = "http://opendata.fmi.fi/wfs?service=WFS&version=2.0.0&request=getFeature&storedquery_id=fmi::forecast::hirlam::surface::point::multipointcoverage&place=tapanila,helsinki&parameters=temperature,humidity";
        const string jsonStringTemplate = @"{{""datetime"": ""{0}"", ""temperature"": {1}, ""humidity"": {2} }}";

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // TODO: Error handling
            string response = string.Empty;
            using (var client = new HttpClient())
            {
                // Get xml data from FMI
                var result = await client.GetStreamAsync(forecastUri);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(result);
                var ns = new XmlNamespaceManager(xmlDoc.NameTable);
                ns.AddNamespace("gml", "http://www.opengis.net/gml/3.2");
                ns.AddNamespace("om", "http://www.opengis.net/om/2.0");

                var beginTimeNode = xmlDoc.SelectSingleNode("//om:phenomenonTime/gml:TimePeriod/gml:beginPosition", ns);
                DateTime beginTime = DateTime.Parse(beginTimeNode.InnerText);

                // Read space separated weather values
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
                        if (!String.IsNullOrEmpty(line)) {
                            // Each line contains temp and humidity values, split them into an array
                            var values = line.Split(" ");
                            // TODO: Error handling for array index
                            string jsonString = string.Format(jsonStringTemplate, time.ToUniversalTime().ToString("s") + "Z", values[0], values[1]);
                            Console.WriteLine(jsonString);

                            var jsonObject = JObject.Parse(jsonString);
                            jsonArray.Add(jsonObject);
                            time = time.AddHours(1);    // Each line in doubleOrNilReasonTupleList contains values for each our
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
