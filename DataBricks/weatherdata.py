# Databricks notebook source
# Calculate a starting time
import datetime
start_date = (datetime.datetime.utcnow() - datetime.timedelta(days = 1)).strftime("%Y-%m-%dT%H:%M:%SZ")
print(start_date)

# COMMAND ----------

# Fetch the weather information
from urllib.request import urlopen

fmi_url = "https://opendata.fmi.fi/wfs?request=getFeature&storedquery_id=fmi%3A%3Aobservations%3A%3Aweather%3A%3Atimevaluepair&crs=EPSG%3A%3A3067&fmisid=101009&parameters=temperature&starttime={0}".format(start_date)
print(fmi_url)

with urlopen(fmi_url) as response:
    data = response.read()

# COMMAND ----------

# Read temperature values as xml
import xml.etree.ElementTree as etree

root = etree.fromstring(data)
values = root.findall(".//{http://www.opengis.net/waterml/2.0}MeasurementTVP")

# COMMAND ----------

# Read each xml element and generate a dataframe
from pyspark.sql.types import *
schema = StructType([StructField("datetime",StringType(), True),StructField("temperature", FloatType(), True)])
temp_frame = sqlContext.createDataFrame([], schema)

for node in values:
    datetime = node.find("{http://www.opengis.net/waterml/2.0}time").text 
    temperature = float(node.find("{http://www.opengis.net/waterml/2.0}value").text)
    temp_frame = temp_frame.union(spark.createDataFrame([[datetime, temperature]], schema))
display(temp_frame)

# COMMAND ----------

# Write dataframe to Azure Blob as JSON
MOUNTPOINT = "/mnt/weatherstorage"
CONTAINER = dbutils.secrets.get(scope="Azure Key Vault", key="container-name")
STORAGE = dbutils.secrets.get(scope="Azure Key Vault", key="storage-account-name")
SAS = dbutils.secrets.get(scope="Azure Key Vault", key="databricks-accesstoken")
URI = "fs.azure.sas.{container}.{storage}.blob.core.windows.net".format(container=CONTAINER, storage=STORAGE)

try:
  dbutils.fs.mount(
    source = "wasbs://{container}@{storage}.blob.core.windows.net".format(container=CONTAINER, storage=STORAGE), 
    mount_point = MOUNTPOINT,
    extra_configs = {URI:SAS})
except Exception as e:
  if "Directory already mounted" in str(e):
    print("Mount already exists")
    pass
  else:
    raise e

# Repartition to 1 to get only one CSV file
temp_frame.repartition(1).write.mode("overwrite").json(MOUNTPOINT + "/weatherdata.json")


# COMMAND ----------

dbutils.fs.unmount(MOUNTPOINT)