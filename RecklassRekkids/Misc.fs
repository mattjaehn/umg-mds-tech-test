module RecklassRekkids.Misc

open System
open System.IO

/// 

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




let tryReadAllLines filePath =
    try filePath |> File.ReadAllLines |> Ok
    with e -> e.Message |> Error

let readAllLinesOrDie filePath =
    match filePath |> tryReadAllLines with
    | Ok(lns) -> lns
    | Error(err) ->
        printf "Fatal error while trying to read %A - %A.  Goodbye." filePath err
        Environment.Exit 2
        // we never get here but in order to keep the function signature sane.
        [||]