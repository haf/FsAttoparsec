module Attoparsec.Tests.BMPString

open Expecto
open Expecto.Flip
open FsCheck
open Attoparsec

module Props =

  let ``append test`` (NonEmptyString s) (NonEmptyString t) =
    BMPString.toString (BMPString.append (BMPString.ofString s) (BMPString.ofString t)) = s + t

  let ``fold test`` (NonEmptyString s) =
    let actual = BMPString.ofString s |> BMPString.fold (fun acc c -> acc + (string c)) ""
    actual = s

  let ``head test`` (NonEmptyString s) =
    ((not <| System.String.IsNullOrEmpty s) ==>
      lazy (BMPString.head (BMPString.ofString s) |> char = s.Chars(0)))

  let ``cons test`` (NonEmptyString s) =
    let s = BMPString.ofString s
    ((not <| BMPString.isEmpty s) ==>
      lazy ((BMPString.cons (BMPString.head s) (BMPString.tail s)) = s))

  let ``span test`` (NonEmptyString s) =
    let text = BMPString.ofString s
    let f = char >> System.Char.IsNumber
    let actual =
      text
      |> BMPString.span f
      |> (fun (x, y) -> BMPString.toString x, BMPString.toString y)
    let f = System.Char.IsNumber
    let expected =
      (System.String(Seq.takeWhile f s |> Seq.toArray),
        System.String(Seq.skipWhile f s |> Seq.toArray))
    actual = expected

  open Helper

  let monoid = BMPString.monoid
  let mempty = monoid.mempty
  let mappend x y = monoid.mappend(x, y)

  let ``monoid first law`` (NonEmptyString (Bmp x)) =
    mappend mempty x = x

  let ``monoid second law`` (NonEmptyString (Bmp x)) =
    mappend x mempty = x

  let ``monoid third law`` (NonEmptyString (Bmp x)) (NonEmptyString (Bmp y)) (NonEmptyString (Bmp z)) =
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
        let str = BMPString.ofString str
        let expectedFront = BMPString.ofString expectedFront
        let expectedBack = BMPString.ofString expectedBack
        let act = str |> BMPString.splitAt pos
        act |> Expect.equal "Should have the expected front- and back."
                            (expectedFront, expectedBack))
  ]