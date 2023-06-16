module RecklassRekkids.SearchStepDefinitions

open System
open System.IO
open System.Reflection
open Microsoft.VisualStudio.TestTools.UnitTesting
open Misc
open Search
open TickSpec


// to read the provided data from our resources
let contractsPath = @"contracts.txt"
let partnersPath  = @"partners.txt"

let getDataLines (fileName:string) =
    let assembly = Assembly.GetExecutingAssembly()

    let rns =
        assembly.GetManifestResourceNames()
        |> Seq.filter (fun (n:string) -> n.EndsWith(fileName))

    // not really what we are testing in terms of app functionality, but
    // does serve to verify some of the fsproj config...
    Assert.AreEqual(rns |> Seq.length, 1)

    // get the IO stream for our data file and unfold it into a seq of lines
    let rnsArr = rns |> Seq.toArray
    let rn = rnsArr.[0]

    new StreamReader(assembly.GetManifestResourceStream(rn))
    |> Seq.unfold (fun sr ->
    match sr.ReadLine() with
    | null -> sr.Dispose(); None
    | str -> Some(str, sr))
    // the proceeding sequence reading the stream is lazy.
    // we pipe it to toArray to force evaluation (reading, disposing, etc)
    |> Seq.toArray

    

// functions to compare our Contract types with the Tables provided in the BDD.
type SearchResultRow = {
    Artist:string; Title:string; Usages:string; StartDate:string; EndDate:string }

let contractToSearchResultRow (contract:Contract) : SearchResultRow =
    let endDateStr =
        match contract.EndDate with
        | Some(dt) -> dt.ToString("MM-dd-yyyy")
        | None -> ""
    let usagesStrArr =
        contract.Usages
        |> Seq.map(fun u -> u.ToString)
        |> Seq.toArray

    let usagesStr = (", ", usagesStrArr) |> String.Join

    {   Artist = contract.Artist;
        Title = contract.Title;
        Usages = usagesStr;
        StartDate = contract.StartDate.ToString("MM-dd-yyyy");
        EndDate = endDateStr }


// implementing the BDD clauses
//
// NOTE that the return value of one clause gets passed as the last parameter of
// the following clause.
// this allows us, in particular, to keep things functional while still establishing the
// Given context (our curried doSearch) which is then used in the When, which in turn provides the
// actual search results to the Then, which compares those results against the expected
// table from the BDD.
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


let [<Then>] ``the output should be`` (expectedUnsorted:SearchResultRow[]) (searchResults:seq<Contract>) =

    let actual =
        searchResults
        |> Seq.map(contractToSearchResultRow)
        |> Seq.toArray
        |> Array.sort

    let expected =
        expectedUnsorted
        |> Array.sort

    Assert.AreEqual(actual.Length, expected.Length)

    actual
    |> Array.iteri(fun indx actualElem ->
        let expectedElem = expected.[indx]
        Assert.AreEqual(actualElem, expectedElem))


    Assert.AreEqual(false, false)
    ()
