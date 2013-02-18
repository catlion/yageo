module fsharpClient.Main
open System
open Yageo

let samples = [
    "Москва, Красная Площадь";
    "Санкт-Петербург, Невский проспект д. 1";
    "Воронеж, ул. Лизюкова";
    "Бобруйск"
]

let best_match (is: GeoResult seq) = is |> Seq.min

let geocode addr = async {
    let coder = new GeoRequest(addr)
    let! res = coder.AsyncGeocode()
    let best = best_match res
    printfn "%A" best }

[<EntryPoint>]
let main args = 
    samples
        |> List.map geocode
        |> Async.Parallel
        |> Async.Ignore
        |> Async.RunSynchronously
    0
