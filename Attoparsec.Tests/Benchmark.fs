module Attoparsec.Tests.Benchmark

open Expecto
open Expecto.Flip
open BenchmarkDotNet
open BenchmarkDotNet.Attributes
open System.Reflection
open System.IO
open System.Diagnostics

let rootPath =
  Path.GetFullPath(
    Path.Combine(
      Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
      "..",
      ".."
    )
  )

let file dir name =
  Path.Combine (
    rootPath,
    dir,
    name
  )

let read (fileName: string) =
  use reader = new StreamReader(file "json-data" fileName)
  reader.ReadToEnd()


type JSONParsing() =
  let data =
    [ "twitter100"
      "numbers"
      "05.feature-pieces" ]
    |> List.map (fun name -> name, sprintf "%s.json" name |> read)
    |> Map.ofList

  [<Params("twitter100", "numbers", "05.feature-pieces")>]
  member val file = "" with get, set

  [<Benchmark(Baseline=true)>]
  member x.FParsec () = JSON.FParsec.parseJsonString data.[x.file]

  [<Benchmark>]
  member x.Attoparsec () = JSON.Atto.parseJsonString data.[x.file]

let benchmarkConfig =
  { benchmarkConfig with
      exporters = Exporters.RPlotExporter.Default :: benchmarkConfig.exporters }

[<Tests>]
let tests =
  testList "benchmark" [
    testList "JSON" [
      benchmark<JSONParsing> "FParsec vs Attoparsec" benchmarkConfig ignore
    ]
  ]