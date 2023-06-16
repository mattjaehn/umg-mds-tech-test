module RecklassRekkids.Program

open System

open Search






[<EntryPoint>]
let main argv =
    match [ for a in argv -> a ] |> parseArgs with
    | None ->
        printf "invalid arguments.\n"
        printf "correct usage: /path/to/RecklassRekkids [path to contracts data] [path to partners data] [partner name] [effective date]\n"
        Environment.Exit 1
        
    | Some(parsedArgs:RRArgs) ->
        let contracts = parsedArgs.ContractsPath |> parseContractsFromFileOrDie
        let partners = parsedArgs.PartnersPath |> parsePartnersFromFileOrDie
        let doSearch = consDoSearch contracts partners
        let ansRows =
            doSearch parsedArgs.Partner parsedArgs.EffectiveDate

        printf "\n\nsearch results:\n"
        ansRows
        |> Seq.iter (fun r -> printf "\t%A\n\n" r)
        ()

    0