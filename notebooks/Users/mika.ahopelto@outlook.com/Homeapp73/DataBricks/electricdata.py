# Databricks notebook source
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

# Load electric yearly coonsumption from an Excel file retrieved manually from the electric company
from pyspark.sql.functions import *

elDf = (spark.read.format("com.crealytics.spark.excel")
.option("header", "true")
.option("treatEmptyValuesAsNulls", "false")
.option("inferSchema", "true")
.option("addColorColumns", "false")
.load("/mnt/weatherstorage/electric-usage.xlsx"))
elDf = elDf.where("Tila <> 'Puuttuva'").withColumn("DateHour", date_format(to_timestamp(concat(col("Päivämäärä"), lit(" "), col("Tunti")), "d.M.yyyy H:mm"), "yyyy-MM-dd HH"))
display(elDf)

# COMMAND ----------

# Load the temperature data and group it by each hour calculating the average
temperDf = spark.read.format("json").load("/mnt/weatherstorage/weatherdata.json")
temperDf = temperDf.withColumn("DateHour", date_format(to_timestamp(col("datetime")), "yyyy-MM-dd HH")).groupBy("DateHour").agg(avg("temperature").alias("Temperature"))
display(temperDf)

# COMMAND ----------

# Join the earlier fetched temperature data to the electric usage data
from pyspark.sql import *
# Ugly way of joining:
#    combDf = temperDf.join(elDf, temperDf["DateHour"] == elDf["DateHour"]).select(temperDf["DateHour"], "Temperature", "kWh")
# SQL way:
temperDf.registerTempTable("temper")
elDf.registerTempTable("elusage")
combDf = sqlContext.sql("""SELECT elusage.DateHour, temper.Temperature, elusage.kWh FROM temper RIGHT OUTER JOIN elusage ON temper.DateHour == elusage.DateHour""")

display(combDf)