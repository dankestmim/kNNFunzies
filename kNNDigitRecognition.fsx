#r @"..\packages\FSharp.Charting.2.1.0\lib\net45\FSharp.Charting.dll"
//#r @"..\packages\FSharp.Data.3.0.0\lib\net45\FSharp.data.dll"

open System.IO
//open FSharp.Data
open FSharp.Charting


// reading in data from MNIST
let dataLines =
    File.ReadAllLines(__SOURCE_DIRECTORY__ + """\trainingsample.csv""").[1..]

// creating our arrays
let dataNumbers =
    dataLines
    |> Array.map (fun line -> line.Split(','))
    |> Array.map (Array.map (int))

// types make things easier to work with (store and label)
type DigitRecord = { Label:int; Pixels:int[] }

let dataRecords =
    dataNumbers
    |> Array.map (fun record ->
                        {Label = record.[0]; Pixels = record.[1..]})
 
// subdividing data
let trainingSet = dataRecords.[..3999]
let crossValidationSet = dataRecords.[4000..4499]
let testSet = dataRecords.[4500..]

// distance finding
let distanceTo (unknownDigit:int[]) (knownDigit:DigitRecord) = 
    Array.map2 (
        fun unknown known ->
            let difference = unknown - known
            int64 (difference * difference)
        ) unknownDigit knownDigit.Pixels
    |> Array.sum

let classifyByNearest k (unknownDigit:int[]) =
    trainingSet
    |> Array.sortBy (distanceTo unknownDigit)
    |> Seq.take k
    |> Seq.countBy (fun digit -> digit.Label)
    |> Seq.maxBy (fun (label,count) -> count)
    |> fun (label,count) -> label

//testSet.[..4]
//    |> Array.iter (fun digit ->
//        printfn "Actual: %d, Predicted: %d"
//            digit.Label
//            (digit.Pixels |> classifyByNearest 1))
    
let calculateAccuracyWithNearest k dataSet =
    dataSet
    |> Array.averageBy (fun digit ->
    if digit.Pixels |> classifyByNearest k = digit.Label then 1.
    else 0.)

let predictionAccuracy =
    [1;3;9;27]
    |> List.map (fun k ->
                    k, crossValidationSet |> calculateAccuracyWithNearest k)

//Chart.Line(predictionAccuracy)
//    |> Chart.Show

let bestK =
    [1..20]
    |> List.maxBy (fun k ->
        crossValidationSet |> calculateAccuracyWithNearest k)