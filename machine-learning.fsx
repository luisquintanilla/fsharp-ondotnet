#r "nuget:Microsoft.ML"

open Microsoft.ML
open Microsoft.ML.Data

[<CLIMutable>]
type ModelInput = {
    [<LoadColumn(0)>] Camis: string
    [<LoadColumn(1)>] Boro: string
    [<LoadColumn(2)>] InspectionType: string
    [<LoadColumn(3)>] InspectionScore: float32
    [<LoadColumn(4)>] Violations: string
    [<LoadColumn(5)>] CriticalViolations: float32
    [<LoadColumn(6)>] TotalViolations: float32
}

[<CLIMutable>]
type ModelOutput = {
    InspectionScore: float32
    Score: float32
}

let ctx = MLContext()

let idv = ctx.Data.LoadFromTextFile<ModelInput>(@"C:\Dev\fsharp-ondotnet\prepdata\*.csv",separatorChar=',', allowQuoting=true, hasHeader=false)

let dataSplit = ctx.Data.TrainTestSplit(idv, samplingKeyColumnName="Camis")

let transforms = 
    EstimatorChain()
        .Append(ctx.Transforms.DropColumns("Camis"))
        .Append(ctx.Transforms.Categorical.OneHotEncoding("EncodedBoro","Boro"))
        .Append(ctx.Transforms.Categorical.OneHotEncoding("EncodedInspectionType","InspectionType"))
        .Append(ctx.Transforms.Categorical.OneHotEncoding("EncodedViolations","Violations"))
        .Append(ctx.Transforms.Concatenate("Features","EncodedBoro","EncodedInspectionType","EncodedViolations","CriticalViolations","TotalViolations"))
        .Append(ctx.Regression.Trainers.Sdca(labelColumnName="InspectionScore"))
        
let model = transforms.Fit(dataSplit.TrainSet)

let predictions = model.Transform(dataSplit.TestSet)

ctx.Data.CreateEnumerable<ModelOutput>(predictions,reuseRowObject=false)

let metrics = ctx.Regression.Evaluate(predictions,"InspectionScore","Score")

printfn $"{metrics.RSquared}"

//41720083,Manhattan,Cycle Inspection / Re-inspection,11,"04H,09C,10F",1,3
let input = {
    Camis="41720083"
    Boro="Manhattan"
    InspectionType="Manhattan,Cycle Inspection / Re-inspection"
    InspectionScore=11.0f
    Violations= "04H,09C,10F"
    CriticalViolations=1.0f
    TotalViolations=1.0f
}

let predictionEngine= ctx.Model.CreatePredictionEngine<ModelInput,ModelOutput> model

let prediction = predictionEngine.Predict(input) |> fun p -> printfn $"Actual: {p.InspectionScore} | Predicted {p.Score}"

ctx.Model.Save(model, idv.Schema,"InspectionModel.zip")
