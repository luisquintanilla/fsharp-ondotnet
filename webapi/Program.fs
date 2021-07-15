open Domain
open Saturn
open Giraffe
open Microsoft.Extensions.ML

module Config = 

    open System
    open Microsoft.Extensions.DependencyInjection

    // Configure services
    let servicesConfig (services:IServiceCollection) = 
        services
            .AddPredictionEnginePool<ModelInput, ModelOutput>()
            .FromFile("InspectionModel.zip")
        |> ignore

        services

module Handlers = 

    open System.Text.Json
    open FSharp.Control.Tasks

    // Define HttpHandler
    let postPredictionHandler : HttpHandler = 
        handleContext (
            fun ctx ->
                task {
                    let predEnginePool = ctx.GetService<PredictionEnginePool<ModelInput,ModelOutput>>()
                    let! observation = ctx.BindJsonAsync<Domain.ModelInput>()
                    let prediction = observation |> predEnginePool.Predict
                    return! ctx.WriteTextAsync (JsonSerializer.Serialize prediction)
                }
        )

let apiRouter = router {
    post "/predict" Handlers.postPredictionHandler
}

let app = application {
    service_config Config.servicesConfig
    use_router apiRouter
}

run app