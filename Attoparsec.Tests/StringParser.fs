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
      |> ParseResult.toOption
    actual = Some w

  let ``char ``  w s =
    let actual =
      cons w s
      |> parse (pchar w)
      |> ParseResult.toOption
    actual = Some w

  let ``anyChar`` (NonEmptyString s) =
    let p = (parse anyChar s) |> ParseResult.toOption
    match List.ofSeq s with | [] -> p = None | x::_ -> p = Some x

  let ``notChar`` w s =
    (not <| String.IsNullOrEmpty(s)) ==>
      lazy (let v = s.Chars(0) in ParseResult.toOption (parse (notChar w) s) = (if v = w then None else Some v))

  let ``string `` (NonEmptyString s) (NonEmptyString t) =
    (parse (pstring s) (s + t))
    |> ParseResult.toOption
    |> Option.map BMPString.toString = Some s

  let ``takeCount`` k (NonEmptyString s) =
    (k >= 0) ==> lazy (
      match ParseResult.toOption (parse (take k) s) with
      | None -> k > String.length s
      | Some _ -> k <= String.length s)

  let ``takeWhile `` w (NonEmptyString s) =
    let (h, t) = BMPString.span ((=) w) (BMPString.ofString s)
    s
    |> parseOnly (parser {
      let! hp = takeWhile ((=) w)
      let! tp = takeText
      return hp, tp
    })
    |> (=) (Choice1Of2 (h, t))

  let ``takeWhile1`` w (NonEmptyString s) =
    let sp = BMPString.cons w (BMPString.ofString s)
    let (h, t) = BMPString.span (fun x -> x <= w) sp
    sp
    |> BMPString.toString
    |> parseOnly (parser {
      let! hp = takeWhile1 (fun x -> x <= w)
      let! tp = takeText
      return (hp, tp) })
    |> (=) (Choice1Of2 (h, t))

  let ``takeWhile1 empty`` () =
    ""
    |> parse (Attoparsec.String.takeWhile1 (fun _ -> true))
    |> ParseResult.toOption
    |> Expect.isNone "Because there's no input to parse on."

  let ``endOfInput`` (NonEmptyString s) =
    s |> parseOnly endOfInput = (if String.IsNullOrEmpty s then Choice1Of2 () else Choice2Of2 "endOfInput")

  let ``match_`` (s: int) =
    let input =  string s
    let expected = (input, s)
    input
    |> parseOnly (pmatch signedInt |>> (fun (x, y) -> (BMPString.toString x, int y)))
    |> (=) (Choice1Of2 expected)

  let signum =
    (pchar '+' |>> fun _ -> 1)
    <|> (pchar '-' |>> fun _ -> -1)
    <|> ok 1

  let ``signum `` (NonEmptyString s) =
    let bs = BMPString.ofString s
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