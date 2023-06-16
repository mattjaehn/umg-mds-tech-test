module RecklassRekkids.Search

open System
open System.Linq
open Misc

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
    member me.ToString =
        match me with
        | DigitalDownload -> "digital download"
        | Streaming -> "streaming"


type Contract =
    { Artist : string
      Title : string
      Usages : List<UsageType>
      StartDate : DateTime
      EndDate : Option<DateTime> }

type Partner = { Name : string; Usages: List<UsageType> }



let collapseListOfOpts listOfOpts =
    let rec loop listOfOpts optAcc =
        match listOfOpts with
        | [] -> optAcc
        | None::_ -> None
        | Some(x)::tail ->
            let newOptAcc = optAcc |> Option.map (fun acc -> x::acc)
            loop tail newOptAcc
    Some([]) |> (listOfOpts |> loop)


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


let parseContracts lns =
    lns
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

let parseContractsFromFileOrDie filePath =
    filePath
    |> readAllLinesOrDie
    |> parseContracts


let parsePartners lns =
    lns
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

let parsePartnersFromFileOrDie filePath =
    filePath
    |> readAllLinesOrDie
    |> parsePartners

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

type ContractSearchType = string -> DateTime -> seq<Contract> 
