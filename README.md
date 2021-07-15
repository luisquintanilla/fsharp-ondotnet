# Exploring Spark and ML.NET with F#

This repo contains code shown in [On .NET Live Show - Exploring Spark and ML.NET with F#](https://www.youtube.com/watch?v=Z7KVKHZsWyM)

## Scenario

Train and deploy a machine learning model to predict the score given to a restaurant after inspection using features such as the number of critical violations, violation type, and a few others.

## Prerequisites

- [.NET 5.0 SDK or later](http://dotnet.microsoft.com/download)
- [Configure .NET for Apache Spark](https://docs.microsoft.com/dotnet/spark/tutorials/get-started)
- [VS Code(Optional)](https://code.visualstudio.com/download)
- [Ionide extension(Optional)](https://marketplace.visualstudio.com/items?itemName=Ionide.Ionide-fsharp)
- [.NET Interactive Notebooks extension(Optional)](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)

## Project Assets

- **data** - Location for original dataset.
- **Web API** - F# Saturn Web API to host *InspectionModel.zip* regression model.
  - **Domain.fs** - Contains the schema of the model input and output.
  - **Program.fs** - Web API Entrypoint
  - **InspectionModel.zip** - Regression model to predict the score of a restaurant inspection
- **data-exploration.dib** - .NET Interactive Notebook to explore and visualize data.
- **data.fsx** - F# Interactive script to prepare data for training.
- **machine-learning.fsx** - F# Interactive script to train regression model.
- **submit.cmd** - Console script to start .NET for Apache Spark in Debug mode.

## About the data

The dataset represents the NYC restaurant inspection results. For more information, see [DOHMH New York City Restaurant Inspection Results](https://data.cityofnewyork.us/Health/DOHMH-New-York-City-Restaurant-Inspection-Results/43nn-pn8j).

## Run application

### Prepare data

.NET for Apache Spark is used to prepare the raw data for training. To prep data, use the .NET CLI and F# Interactive to run *data.fsx*.

```dotnetcli
dotnet fsi data.fsx
```

This will prepare the data, create an output directory *prepdata* for the results, and save the results to *.csv* files.

### Train model

ML.NET is used to train a regression model to predict the inspection score. To train your model, use the .NET CLI and F# Interactive to run *machine-learning.fsx*.

```dotnetcli
dotnet fsi machine-learning.fsx
```

This script trains your model and saved the serialized version of it to a file called *InspectionModel.zip*.

### Run Web API

Saturn is used to create an HTTP endpoint to make predictions with the *InspectionModel.zip* model. Use the .NET CLI to start the Web API

Navigate to the *webapi* directory and start your application

```dotnetcli
dotnet run
```

### Make predictions

Once the Web API starts, send an HTTP **POST** request to `http://localhost:5000/predict` with the following JSON body:

```json
{
    "Camis":"41720083",
    "Boro":"Manhattan",
    "InspectionType":"Manhattan,Cycle Inspection / Re-inspection",
    "InspectionScore": 11.0,
    "Violations":"04H,09C,10F",
    "CriticalViolations":1.0,
    "TotalViolations":1.0
}
```

If the request is handled successfully, you should get a response similar to the following:

```json
{
  "InspectionScore": 11,
  "Score": 7.8174763
}
```

## Resources

- [F# Documentation](http://docs.microsoft.com/dotnet/fsharp)
- [.NET for Apache Spark Documentation](http://docs.microsoft.com/dotnet/spark)
- [ML.NET Documentation](http://docs.microsoft.com/dotnet/machine-learning)
- [ML.NET F# Samples](https://github.com/dotnet/machinelearning-samples/tree/main/samples/fsharp)
- [.NET Interactive](https://github.com/dotnet/interactive)
