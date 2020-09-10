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
        const string forecastUri = "http://opendata.fmi.fi/wfs?service=WFS&version=2.0.0&request=getFeature&storedquery_id=fmi::forecast::hirlam::surface::point::multipointcoverage&place=tapanila,helsinki&parameters=Temperature,Pressure,Humidity,WindDirection,WindSpeedMS,MaximumWind,WindGust,DewPoint,TotalCloudCover,WeatherSymbol3,LowCloudCover,MediumCloudCover,HighCloudCover,Precipitation1h,PrecipitationAmount";
        const string jsonValuesTemplate = @"
        {
            {
                ""temperature"": {0}, 
                ""pressure"": {1}, 
                ""humidity"": {2}, 
                ""WindDirection"": {3}, 
                ""WindSpeedMS"": {4}, 
                ""MaximumWind"": {5}, 
                ""WindGust"": {6}, 
                ""DewPoint"": {7}, 
                ""TotalCloudCover"": {8}, 
                ""WeatherSymbol3"": {9}, 
                ""LowCloudCover"": {10}, 
                ""MediumCloudCover"": {11}, 
                ""HighCloudCover"": {12}, 
                ""Precipitation1h"": {13}, 
                ""PrecipitationAmount"": {14} 
            }
        }";

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
                            /*string jsonString = string.Format(jsonValuesTemplate, values);
                            Console.WriteLine(jsonString);*/
                            var jsonObject = new JObject();
                            jsonObject.Add(new JProperty("Datetime", time.ToUniversalTime().ToString("s") + "Z"));
                            jsonObject.Add(new JProperty("Temperature", float.Parse(values[0])));
                            jsonObject.Add(new JProperty("Pressure", float.Parse(values[1])));
                            jsonObject.Add(new JProperty("Humidity", float.Parse(values[2])));
                            jsonObject.Add(new JProperty("WindDirection", float.Parse(values[3])));
                            jsonObject.Add(new JProperty("WindSpeedMS", float.Parse(values[4])));
                            jsonObject.Add(new JProperty("MaximumWind", float.Parse(values[5])));
                            jsonObject.Add(new JProperty("WindGust", float.Parse(values[6])));
                            jsonObject.Add(new JProperty("DewPoint", float.Parse(values[7])));
                            jsonObject.Add(new JProperty("TotalCloudCover", float.Parse(values[8])));
                            jsonObject.Add(new JProperty("WeatherSymbol3", float.Parse(values[9])));
                            jsonObject.Add(new JProperty("LowCloudCover", float.Parse(values[10])));
                            jsonObject.Add(new JProperty("MediumCloudCover", float.Parse(values[11])));
                            jsonObject.Add(new JProperty("HighCloudCover", float.Parse(values[12])));
                            jsonObject.Add(new JProperty("Precipitation1h", float.Parse(values[13])));
                            jsonObject.Add(new JProperty("PrecipitationAmount", float.Parse(values[14])));
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
