#r "nuget:Microsoft.Spark"

open Microsoft.Spark.Sql

// Init session
let sparkSession = 
    SparkSession
        .Builder()
        .AppName("on-dotnet-fsharp")
        .GetOrCreate()

// Load data
let df = 
    sparkSession
        .Read()
        .Option("header","true")
        .Option("inferSchema","true")
        .Csv("data/nyc-restaurant-inspections.csv")     

df.PrintSchema()

// Get borough counts
let boroughs = 
    df.GroupBy([|Functions.Col("BORO")|]).Count()

boroughs.Show()

// Remove bad data
let cleanBoroughs = boroughs.Filter(Functions.Col("BORO").NotEqual("0"))

cleanBoroughs.Show()

#r "nuget: Plotly.NET, 2.0.0-preview.6"
// #r "nuget: Plotly.NET.Interactive, 2.0.0-preview.6"

// Plot borough data
open Plotly.NET

let borough,counts = 
    cleanBoroughs
      .Select("BORO","count")
      .OrderBy(Functions.Col("count").Desc())
      .Collect()
    |> Seq.map(fun row -> (string row.[0], string row.[1] |> int))
    |> Seq.unzip

let boroughColumn = Chart.Column(borough,counts)

boroughColumn
|> Chart.SaveHtmlAs("borough")

// Get coordinates
let coordinates = 
  df
    .Select(
      Functions.Col("CAMIS"),Functions.Col("Latitude"),Functions.Col("Longitude"))
    .DropDuplicates("CAMIS")

coordinates.Show()

// Remove bad data
let nonZeroCoordinates = coordinates.Where("Latitude != 0.0 OR Longitude != 0.0")

// Plot on map
let labels, lat, lon = 
    nonZeroCoordinates.Select("CAMIS","Latitude","Longitude").Collect()
    |> Seq.map(fun row -> string row.[0], string row.[1] |> float, string row.[2] |> float)
    |> Seq.unzip3

let pointMapbox = 
    Chart.PointMapbox(
        lon,lat,
        Labels = labels,
        TextPosition = StyleParam.TextPosition.TopCenter
    )
    |> Chart.withMapbox(
        Mapbox.init(
            Style=StyleParam.MapboxStyle.OpenStreetMap,
            Center=(-73.99,40.73),
            Zoom=8.
        )
    )

pointMapbox
|> Chart.SaveHtmlAs("map")

df.Columns()

// Prep data
let prepData = 
    df
      .Select("CAMIS","BORO","INSPECTION DATE","INSPECTION TYPE","VIOLATION CODE","CRITICAL FLAG","SCORE")
      .Where(Functions.Col("INSPECTION DATE").NotEqual("01/01/1900"))
      .Where(Functions.Col("SCORE").IsNotNull())
      .Where(Functions.Col("BORO").NotEqual("0"))
      .WithColumn("CRITICAL FLAG",Functions.When(Functions.Col("CRITICAL FLAG").EqualTo("Critical"),1).Otherwise(0))
      .GroupBy("CAMIS","BORO","INSPECTION DATE","INSPECTION TYPE", "SCORE") //"INSPECTION TYPE","VIOLATION CODE"
      .Agg(
          Functions.CollectList(Functions.Col("CRITICAL FLAG")).Alias("VIOLATIONS"),
          Functions.CollectList(Functions.Col("VIOLATION CODE")).Alias("CODES"))
      .WithColumn("CRITICAL VIOLATIONS", Functions.Expr("AGGREGATE(VIOLATIONS, 0, (acc, val) -> acc + val)"))
      .WithColumn("TOTAL VIOLATIONS", Functions.Size(Functions.Col("VIOLATIONS")))
      .WithColumn("CODES",Functions.ArrayJoin(Functions.Col("CODES"),","))
      .Drop("VIOLATIONS","INSPECTION DATE")
      .OrderBy("CAMIS")

prepData.Show()

prepData.Write().Mode(SaveMode.Overwrite).Csv("prepdata")