# homeapp73
Small react app that reads weather stuff from Finnish Meteorology Institute, analyses/stores it in Azure and shows it on a web page

Current status: MVP. 
- Shows current temp on web page
- Lists temperature/humidity 24h forecast on a table
- Reads a json stats file from Azure blob storage and draws a Recharts chart as well as a table with data

Get started
- Clone repo
- Set up Azure environment for storing json file in a blob storage
- Add appsettings.Development.json file and .gitignore (optional)
  - Add your own Azure information into the section as follows:
    "AppSettings": {
      "StorageAccountName": "<your storage account name",
      "SasToken": "<your generated sas token>",
      "ContainerName": "<your blob's container name>",
      "BlobPath": "<path to the json file containing weather stats (without domain part or container name)>"
    }
- dotnet run 

Better documentation and info coming up after solution matures...
