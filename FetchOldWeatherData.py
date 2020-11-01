# Databricks notebook source
# Fetch weather data for the whole year in 7 day batches
import datetime as dt

startTime = dt.datetime.strptime("2020-01-01 00:00:00", "%Y-%m-%d %H:%M:%S")

while startTime < dt.datetime.utcnow():
  endTime = startTime + dt.timedelta(minutes=(6*24*60)-10) # Seven days is max
  if(endTime > dt.datetime.utcnow()):
    endTime = dt.datetime.utcnow()
  print("{} {}".format(startTime, endTime))
  dbutils.notebook.run("/Users/<userid>/Homeapp73/weatherdata", 600, {"start_time": startTime, "end_time": endTime })
  startTime = endTime + dt.timedelta(minutes=10)

# COMMAND ----------

# Fetch weather data by exact dates
params = {"start_time":"2020-09-14T00:00:00.000+0000","end_time":"2020-09-17T16:32:39.520+0000"}
dbutils.notebook.run("/Users/mika.ahopelto@outlook.com/Homeapp73/weatherdata", 600, params)