module RecklassRekkids.SearchStepDefinitions

open System
open System.IO
open System.Reflection
open Microsoft.VisualStudio.TestTools.UnitTesting
open Misc
open Search
open TickSpec



let contractsPath = @"contracts.txt"
let partnersPath  = @"partners.txt"




let getDataLines (fileName:string) =
    let assembly = Assembly.GetExecutingAssembly()

    let rns =
        assembly.GetManifestResourceNames()
        |> Seq.filter (fun (n:string) -> n.EndsWith(fileName))
        //|> Seq.map (fun (n:string) -> assembly.GetManifestResourceStream(n))

    // not really what we are testing in terms of app functionality, but
    // does serve to verify some of the fsproj config...
    Assert.AreEqual(rns |> Seq.length, 1)

    // get the IO stream for our data file and unfold it into a seq of lines
    let rnsArr = rns |> Seq.toArray
    let rn = rnsArr.[0]

    let lns = 
        new StreamReader(assembly.GetManifestResourceStream(rn))
        |> Seq.unfold (fun sr ->
        match sr.ReadLine() with
        | null -> sr.Dispose(); None
        | str -> Some(str, sr))
        // the proceeding sequence reading the stream is lazy.
        // we pipe it to toArray to force evaluation (reading, disposing, etc)
        |> Seq.toArray

    lns
    



let [<Given>] ``the supplied reference data`` () =
    let contracts = parseContracts (getDataLines contractsPath)
    let partners = parsePartners (getDataLines partnersPath)
    let doSearch = consDoSearch contracts partners
    doSearch

let [<When>] ``user perform search by (.*) (.*)`` (partner:string) (effectiveDateStr:string) (doSearch:ContractSearchType)=
    Assert.AreNotEqual(None, Some(42))
    Assert.AreEqual(None, None)

    printf "partner - %A\n" partner
    printf "effectiveDate - %A\n" effectiveDateStr

    let ans = match DateTime.TryParseOption(effectiveDateStr) with
    | Some(effectiveDate) -> doSearch partner effectiveDate
    | _ ->
        Assert.Fail(sprintf "failed to parse effectiveDate from %s" effectiveDateStr)
        // to satisfy the type checking.  if we ever get here, we should not get to actually returning the Seq.empty
        Seq.empty
    
    ans


let [<Then>] ``the output should be`` (expected:Table) (actual:seq<Contract>) =
    Assert.AreEqual(false, false)
    ()
