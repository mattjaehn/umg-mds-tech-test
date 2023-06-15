module RecklassRekkids.Program

open System
open FSharp.Data
open System.Linq
open System.IO

open Search

// this defines an optional computation expression.
// such a comp expr will allow us to assemble
// together various vals wrapped in optionals
// in a clean syntax ( see the code block
// `optional { .. }` below.
// see https://gist.github.com/kekyo/cadc0ec4b016368a0cee81d87fbee63a
[<Struct>]
type OptionalBuilder =
    // this implements the let! in comp expr
    member __.Bind(opt, binder) =
        match opt with
        | Some value -> binder value
        | None -> None
    // this implements return in the comp expr
    member __.Return(value) =
        Some value
let optional = OptionalBuilder()


type System.DateTime with
    static member TryParseOption (str:string) =
        match System.DateTime.TryParse(str) with
        | true, ans -> Some(ans)
        | _ -> None

type RRArgs =
    { ContractsPath : string
      PartnersPath : string
      Partner: string
      EffectiveDate: DateTime }

// types for our domain data
type UsageType =
    | DigitalDownload
    | Streaming
with
    static member FromString (str:string) =
        match str with
        | "digital download" -> Some(DigitalDownload)
        | "streaming" -> Some(Streaming)
        | _ -> None


type Contract =
    { Artist : string
      Title : string
      Usages : List<UsageType>
      StartDate : DateTime
      EndDate : Option<DateTime> }

type Partner = { Name : string; Usages: List<UsageType> }


let tryReadAllLines filePath =
    try filePath |> File.ReadAllLines |> Ok
    with e -> e.Message |> Error

let readAllLinesOrDie filePath =
    match filePath |> tryReadAllLines with
    | Ok(lns) -> lns
    | Error(err) ->
        printf "Fatal error while trying to read %A - %A.  Goodbye." filePath err
        Environment.Exit 1
        // we never get here but in order to keep the function signature sane.
        [||]

let g xx =
    let rec loop yy acc =
        match yy with
        | [] -> acc
        | y::tail -> loop tail (y + acc)
    let ans = loop xx 0
    ans


let collapseListOfOpts listOfOpts =
    let rec loop listOfOpts optAcc =
        match listOfOpts with
        | [] -> optAcc
        | None::_ -> None
        | Some(x)::tail ->
            let newOptAcc = optAcc |> Option.map (fun acc -> x::acc)
            loop tail newOptAcc
    Some([]) |> (listOfOpts |> loop)


//let antiLift listOfOpts =
//    let rec loop listOfOpts acc =
//        match listOfOpts with
//        | [] -> acc
//        | Some(x)::tail -> loop tail (x::acc)
//        | None::tail -> []
//    loop listOfOpts []



let parseUsagesColumn (str:string) =
    str.Split(", ")
    |> Seq.map(UsageType.FromString)
    |> Seq.toList
    |> collapseListOfOpts



let unlift xx =
    xx
    |> (fun xx ->
        ([], xx)
        ||> Seq.fold(
            fun acc elem ->
                match elem with
                | Some(v) -> v::acc
                | None -> acc))
    |> Seq.rev



 // handle command line args

let parseArgs (aa: string list) =
    match aa.Length with
    | l when l < 4 -> None
    | _ ->
        match aa.[0..3] with
        | [ contracts; partners; partner; effDtStr ] ->
            DateTime.TryParseOption(effDtStr)
                |> Option.map (fun dt -> {
                    ContractsPath = contracts;
                    PartnersPath = partners;
                    Partner = partner; EffectiveDate = dt})
        | _ -> None


let parseContracts filePath =
    filePath
    |> readAllLinesOrDie
    //|> File.ReadAllLines
//    |> Seq.map(fun x -> printf "LINE: %A\n" x; x)
    |> Seq.skip(1)  // skip header line
    |> Seq.map(fun (ln:string) ->

        match ln.Split("|") with
        | [| a; t; u; s; e |] ->
            optional {
                let! usages = u |> parseUsagesColumn
                let! startDate = s |> DateTime.TryParseOption
                //NOTE that since our EndDate field is an optional, we actually
                //use regular let here and not let!  if e is the empty string, then
                //TryParseOption will return None, which is what we want.
                let endDate = e |> DateTime.TryParseOption
                return { Artist = a; Title = t; Usages = usages; StartDate = startDate; EndDate = endDate }
            }
        | _ -> None)
    |> unlift

let parsePartners filePath =
    filePath
    |> readAllLinesOrDie
    //|> File.ReadAllLines
    |> Seq.skip(1)
    |> Seq.map (fun (ln:string) ->
        match ln.Split("|") with
        | [| n; u |] ->
            optional {
                let! usages = u |> parseUsagesColumn
                return { Name = n; Usages = usages }
            }
        | _ -> None)
    |> unlift



// this is a function to construct (hence "cons" in the name) the function
// that we will use for searching.
// NOTE that F# functions curry naturally.
// that is, a function with 4 params is actually a function with 1 param
// that returns a function with 3 params, etc.
let consDoSearch contracts partners partnerName effectiveDate =
    let targetUsages =
        partners
        |> Seq.filter(fun p -> true)
        |> Seq.filter(fun p -> p.Name.Equals(partnerName))
        |> Seq.map(fun p -> p.Usages)
        |> Seq.concat

    contracts
    // became available before our effectiveDate
    |> Seq.filter(fun c -> c.StartDate.CompareTo(effectiveDate) <= 0)
    // for endDate, we only want to filter when the EndDate field is there
    // (i.e., not None)
    |> Seq.filter(fun c ->
        match c.EndDate with
        | Some(endDate) -> endDate.CompareTo(effectiveDate) >= 0
        | None -> true)
    // here, we remove any usages that are not in our target usage.
    // note that some records may now have empty Usages list...
    |> Seq.map(fun c ->
        let usagesIntersection = c.Usages |> Seq.filter(fun u -> targetUsages.Contains(u))
        { c with Usages = usagesIntersection |> Seq.toList })
    // and now we filter out by those empty usages, and we now have our answer.
    |> Seq.filter(fun c -> c.Usages |> Seq.length > 0)








//let h =
//    "./data/contracts.txt"
//    |> File.ReadLines
//    |> Seq.iter (fun ln ->
//        printf "LINE: %A\n" ln)










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
        let contracts = parseContracts programArgs.ContractsPath
        let partners = parsePartners programArgs.PartnersPath

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