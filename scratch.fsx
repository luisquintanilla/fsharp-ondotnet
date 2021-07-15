// Install packages
#r "nuget:Microsoft.ML"

// Install packages
open Microsoft.ML
open Microsoft.ML.Data
open Microsoft.ML.Transforms
open System.IO

// Initialize MLContext
let ctx = new MLContext()

// Build IDV Schema
let dvbuilder = DataViewSchema.Builder()

// Define columns
let cols = 
    seq {
        DataViewSchema.DetachedColumn("Col0",NumberDataViewType.Single)
        DataViewSchema.DetachedColumn("Col1",TextDataViewType.Instance)
    }

// Add columns
dvbuilder.AddColumns(cols);

// Build schema
let schema = dvbuilder.ToSchema()

// Load data
let data = 
    seq {
        {|Col0=1.f;Col1="Y"|}
        {|Col0=0.f;Col1="N"|}
        {|Col0=1.f;Col1="Y"|}}

// Load data into IDV
let dv = ctx.Data.LoadFromEnumerable(data,schema)

// ML Pipeline
let pipeline = 
    EstimatorChain()
    .Append(ctx.Transforms.Conversion.MapValueToKey("Col1"))
    .Append(ctx.MulticlassClassification.Trainers.OneVersusAll())