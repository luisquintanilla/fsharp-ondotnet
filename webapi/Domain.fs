// Define model input and output schema
module Domain

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