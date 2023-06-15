module RecklassRekkids.Program

open System

open Search
open Misc






[<EntryPoint>]
let main argv =

    let d0 = DateTime.Parse("01-07-2012")
    printf "1-7-2012=> %A\n" d0

    let d1 = DateTime.Parse("07-01-2012")
    printf "7-1-2012 => %A\n" d1

    printf "argv = %A\n" argv

    let pwd = Environment.CurrentDirectory
    printf "pwd = %A\n" pwd
    
    let xx =
        match argv with
        | [| c; p; u; d; |] -> [ for a in argv -> a ]
        | _ ->
            [ "../../../data/contracts.txt"; "../../../data/partners.txt"; "YouTube"; "12-26-2012" ]
    
    let ans = optional {
        let! programArgs = parseArgs xx
        let contracts = parseContractsFromFileOrDie programArgs.ContractsPath
        let partners = parsePartnersFromFileOrDie programArgs.PartnersPath

        let z = partners |> Seq.filter(fun p -> true)

        let doSearch = consDoSearch contracts partners
        return doSearch programArgs.Partner programArgs.EffectiveDate
    }



    printf "\n\n\nANS =\n"
    match ans with
    | None -> printf "none"
    | Some(yy) -> yy |> Seq.iter(fun y -> printf "\t%A\n\n" y)


    //let doSearch = optional {
    //    let! parsedArgs = xx |> parseArgs
    //    let! partners = parsedArgs.PartnersPath |> parsePartners
    //    let! contracts = parsedArgs.ContractsPath |> parseContracts

    //    // here is where that currying comes into play
    //    let doSearch = consDoSearch contracts partners
    //    return doSearch
    //}


    //let rrargs =
    //    xx //[ for a in Environment.GetCommandLineArgs() -> a ]
    //    |> Seq.toList
    //    |> parseArgs
    //    //|> Option.map(parseArgs)
    //match rrargs |> Option.map(fun rargs ->
    //    rargs.PartnersPath |> parsePartners) with
    //| Some(xx) ->
    //    //printf "some %A" xx
    //    xx |> Seq.iter(fun x -> printf "\t%A\n\n" x)
    //| None -> printf "nothing\n"
    

    //match rrargs |> Option.map(fun rargs ->
    //    rargs.ContractsPath |> parseContracts) with
    //| Some(xx) ->
    //    //printf "ans - \n"
    //    xx |> Seq.iter(fun x -> printf "\t%A\n\n" x)


    0