module Attoparsec.Tests.TokenParser

open Expecto
open Expecto.Flip
open System
open Attoparsec
open Attoparsec.String
open Attoparsec.Token
open Attoparsec.Expr

let exprDef = {
  Language.empty with OpLetter = oneOf "+-*/"
}

let lexer<'a> : TokenParser<'a> = makeTokenParser exprDef

let rec term = lazy (lexer.Parens expr <|> lexer.Natural)
and expr =
  let op sym fn = Infix(lexer.ReservedOp sym >>. ok fn, AssocLeft)
  let table = [
    [op "*" (*); op "/" (/)];
    [op "+" (+); op "-" (-)]
  ]
  buildExpressionParser table term

let exprParser = lexer.WhiteSpace >>. expr

let parseExpr input = parse exprParser input |> ParseResult.feed ""

[<Tests>]
let tests =
  [ "1 + 2", 3
    "1 - 1", 0
    "2 * 3", 6
    "4 / 2", 2
    "1 + 2 * 3", 7
  ]
  |> List.mapi (fun i (input, expected) ->
      testCase (sprintf "expression (%s)" input) <| fun () ->
        match parseExpr input with
        | Done (_, actual) ->
          actual |> Expect.equal "Should equal..." expected
        | x ->
          Tests.failtestf "Didn't parse successfully: %A" x)
  |> testList "expressions"