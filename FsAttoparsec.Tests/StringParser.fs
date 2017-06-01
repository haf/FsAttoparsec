module Attoparsec.Tests.StringParserTest

open FsCheck
open Expecto
open Expecto.Flip
open System
open Attoparsec
open Attoparsec.Parser
open Attoparsec.String

module Props =

  let cons (w: char) s = (string w) + s

  let ``satisfy`` w s =
    let actual =
      cons w s
      |> parse (satisfy (fun x -> x <= w))
      |> ParseResult.option
    actual = Some w

  let ``char ``  w s =
    let actual =
      cons w s
      |> parse (pchar w)
      |> ParseResult.option
    actual = Some w

  let ``anyChar`` (StringNoNulls s) =
    let p = (parse anyChar s).Option
    match List.ofSeq s with | [] -> p = None | x::_ -> p = Some x

  let ``notChar`` w s =
    (not <| String.IsNullOrEmpty(s)) ==>
      lazy (let v = s.Chars(0) in (parse (notChar w) s).Option = (if v = w then None else Some v))

  let ``string `` (StringNoNulls s) (StringNoNulls t) =
    (parse (pstring s) (s + t)).Option
    |> Option.map BmpString.toString = Some s

  let ``takeCount`` k (StringNoNulls s) =
    (k >= 0) ==> lazy (match (parse (take k) s).Option with | None -> k > String.length s | Some _ -> k <= String.length s)

  let ``takeWhile `` w (StringNoNulls s) =
    let (h, t) = BmpString.span ((=) w) (BmpString.ofString s)
    s
    |> parseOnly (parser {
      let! hp = takeWhile ((=) w)
      let! tp = takeText
      return (hp, tp)
    })
    |> (=) (Choice1Of2 (h, t))

  let ``takeWhile1`` w (StringNoNulls s) =
    let sp = BmpString.cons w (BmpString.ofString s)
    let (h, t) = BmpString.span (fun x -> x <= w) sp
    sp
    |> BmpString.toString
    |> parseOnly (parser {
      let! hp = takeWhile1 (fun x -> x <= w)
      let! tp = takeText
      return (hp, tp) })
    |> (=) (Choice1Of2 (h, t))

  let ``takeWhile1 empty`` () =
    ""
    |> parse (Attoparsec.String.takeWhile1 (fun _ -> true))
    |> ParseResult.option
    |> Expect.isNone "Because there's no input to parse on."

  let ``endOfInput`` (StringNoNulls s) =
    s |> parseOnly endOfInput = (if String.IsNullOrEmpty s then Choice1Of2 () else Choice2Of2 "endOfInput")

  let ``match_`` (s: int) =
    let input =  string s
    let expected = (input, s)
    input
    |> parseOnly (pmatch signedInt |>> (fun (x, y) -> (BmpString.toString x, int y)))
    |> (=) (Choice1Of2 expected)

  let signum =
    (pchar '+' |>> fun _ -> 1)
    <|> (pchar '-' |>> fun _ -> -1)
    <|> ok 1

  let ``signum `` (StringNoNulls s) =
    let bs = BmpString.ofString s
    ((s.StartsWith("-") || s.StartsWith("+")) |> not) ==>
      (match parse signum ("+" + s) with ParseResult.Done(s, 1) when bs = s -> true | _ -> false
      && match parse signum ("-" + s) with ParseResult.Done(s, -1) when bs = s -> true | _ -> false
      && match parse signum s |> ParseResult.feed "" with ParseResult.Done(s, 1) when bs = s -> true | _ -> false)

[<Tests>]
let tests =
  testList "string parser" [
    testProperty "satisfy" Props.satisfy
    testProperty "char" Props.``char ``
    testProperty "anyChar" Props.anyChar
    testProperty "notChar" Props.notChar
    testProperty "string" Props.``string ``
    testProperty "takeCount" Props.takeCount
    testProperty "takeWhile" Props.``takeWhile ``
    testProperty "takeWhile1" Props.takeWhile1
    testCase "takeWhile1 empty" Props.``takeWhile1 empty``
    testProperty "endOfInput" Props.endOfInput
    testProperty "signum" Props.``signum ``
  ]