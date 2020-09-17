# Databricks notebook source
# MAGIC %pip install fmiopendata  

# COMMAND ----------

# Fetch weather observation data for the specified time range for the hard coded location: Kumpula
from fmiopendata.wfs import download_stored_query
import datetime as dt

startTime = dt.datetime.strptime(dbutils.widgets.get("start_time"), "%Y-%m-%dT%H:%M:%S.%f+0000")
endTime = dt.datetime.strptime(dbutils.widgets.get("end_time"), "%Y-%m-%dT%H:%M:%S.%f+0000")
startTime = startTime.isoformat(timespec="seconds") + "Z"
endTime = endTime.isoformat(timespec="seconds") + "Z"
print("{} {}".format(startTime, endTime))
obs = download_stored_query("fmi::observations::weather::multipointcoverage",
                            args=["place=Kumpula",
                                  "starttime=" + startTime,
                                  "endtime=" + endTime,
                                  "timeseries=True"])

# COMMAND ----------

# Create dataframe from multi-dimension dict objects
import pandas as pd
from pyspark.sql.functions import *

location = 'Helsinki Kumpula'
keys = obs.data[location]['times']
temperature = obs.data[location]['t2m']['values']
wind = obs.data[location]['ws_10min']['values']
windHigh = obs.data[location]['wg_10min']['values']
windDirection = obs.data[location]['wd_10min']['values']
humidity = obs.data[location]['rh']['values']
moistPoint = obs.data[location]['td']['values']
precipitation  = obs.data[location]['r_1h']['values']
precIntensity = obs.data[location]['ri_10min']['values']
snowDept = obs.data[location]['snow_aws']['values']
pressure = obs.data[location]['p_sea']['values']
visibility = obs.data[location]['vis']['values']
clouds = obs.data[location]['n_man']['values']
weather = obs.data[location]['wawa']['values']

pdDf = pd.DataFrame({'dt': keys, 't2m': temperature, 'ws_10min': wind, 'wg_10min': windHigh, 'wd_10min': windDirection, 'rh': humidity, 'td': moistPoint, 'r_1h': precipitation, 'ri_10min': precIntensity, 'snow_aws': snowDept, 'p_sea': pressure, 'vis': visibility, 'n_man': clouds, 'wawa': weather })
df = spark.createDataFrame(pdDf).withColumn('month', substring(col('dt'), 0,7))
display(df)


# COMMAND ----------

# Mount Azure storage
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


# COMMAND ----------

# Write the dataframe into a sinle json file under a month folder
df.repartition(1).write.partitionBy("month").mode("append").json(MOUNTPOINT + "/weatherdata.json")


# COMMAND ----------

# Unmount if you wish: dbutils.fs.unmount(MOUNTPOINT)