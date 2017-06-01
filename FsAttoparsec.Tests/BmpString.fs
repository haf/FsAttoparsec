﻿module Attoparsec.Tests.BmpString

open Expecto
open Expecto.Flip
open FsCheck
open Attoparsec

module Props =

  let ``append test`` (StringNoNulls s) (StringNoNulls t) =
    BmpString.toString (BmpString.append (BmpString.ofString s) (BmpString.ofString t)) = s + t

  let ``fold test`` (StringNoNulls s) =
    let actual = BmpString.ofString s |> BmpString.fold (fun acc c -> acc + (string c)) ""
    actual = s

  let ``head test`` (StringNoNulls s) =
    ((not <| System.String.IsNullOrEmpty s) ==>
      lazy (BmpString.head (BmpString.ofString s) |> char = s.Chars(0)))

  let ``cons test`` (StringNoNulls s) =
    let s = BmpString.ofString s
    ((not <| BmpString.isEmpty s) ==>
      lazy ((BmpString.cons (BmpString.head s) (BmpString.tail s)) = s))

  let ``span test`` (StringNoNulls s) =
    let text = BmpString.ofString s
    let f = char >> System.Char.IsNumber
    let actual =
      text
      |> BmpString.span f
      |> (fun (x, y) -> BmpString.toString x, BmpString.toString y)
    let f = System.Char.IsNumber
    let expected =
      (System.String(Seq.takeWhile f s |> Seq.toArray),
        System.String(Seq.skipWhile f s |> Seq.toArray))
    actual = expected

  open Helper

  let monoid = BmpString.monoid
  let mempty = monoid.Mempty
  let mappend x y = monoid.Mappend(x, y)

  let ``monoid first law`` (StringNoNulls (Bmp x)) =
    mappend mempty x = x

  let ``monoid second law`` (StringNoNulls (Bmp x)) =
    mappend x mempty = x

  let ``monoid third law`` (StringNoNulls (Bmp x)) (StringNoNulls (Bmp y)) (StringNoNulls (Bmp z)) =
    mappend x (mappend y z) = mappend (mappend x y) z


[<Tests>]
let tests =
  testList "BMP string" [
    yield testList "props" [
      testProperty "append" Props.``append test``
      testProperty "fold" Props.``fold test``
      testProperty "head" Props.``head test``
      testProperty "cons" Props.``cons test``
      testProperty "span" Props.``span test``
      testProperty "monoid first law" Props.``monoid first law``
      testProperty "monoid second law" Props.``monoid second law``
      testProperty "monoid third law" Props.``monoid third law``

    ]

    let splitAtData =
      [ "", 0, "", ""
        "hoge", -10, "", "hoge"
        "hoge", -1,  "", "hoge"
        "hoge", 0,   "", "hoge"
        "hoge", 1,   "h", "oge"
        "hoge", 2,   "ho", "ge"
        "hoge", 3,   "hog", "e"
        "hoge", 4,   "hoge", ""
        "hoge", 5,   "hoge", ""
        "hoge", 5,   "hoge", ""
        "hoge", 100, "hoge", ""
      ]
      |> List.mapi (fun i x -> i, x)

    for i, (str, pos, expectedFront, expectedBack) in splitAtData do
      yield testCase (sprintf "splitAt test %i" i) (fun () ->
        let str = BmpString.ofString str
        let expectedFront = BmpString.ofString expectedFront
        let expectedBack = BmpString.ofString expectedBack
        let act = str |> BmpString.splitAt pos
        act |> Expect.equal "Should have the expected front- and back."
                            (expectedFront, expectedBack))
  ]