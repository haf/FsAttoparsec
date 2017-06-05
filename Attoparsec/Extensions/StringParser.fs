namespace Attoparsec

open System
open Helper
open Patterns

module String =

  let private monoid = BMPString.monoid

  module ParseResult =
    let feed s (result: ParseResult<_, _>) = ParseResult.feed monoid (BMPString.ofString s) result
    let done_ (result: ParseResult<_, _>) = ParseResult.done_ monoid result

  let parseOnly parser input =
    let input = BMPString.ofString input
    parseOnly BMPString.skip monoid parser input

  let get = get BMPString.skip

  let endOfChunk = endOfChunk BMPString.length

  let wantInput = wantInput BMPString.length

  let atEnd = atEnd BMPString.length

  let ensureSuspended st n = ensureSuspended BMPString.length BMPString.substring st n

  let ensure n = ensure BMPString.length BMPString.substring n

  let elem p what = elem BMPString.length BMPString.head BMPString.substring p what

  let satisfy p = satisfy BMPString.length BMPString.head BMPString.substring p

  let skip p what = skip BMPString.length BMPString.head BMPString.substring p what

  let skipWhile p = skipWhile monoid BMPString.skipWhile p

  let takeWith n p what = takeWith BMPString.length BMPString.substring n p what

  let take n = take BMPString.length BMPString.substring n

  let anyChar = satisfy (fun _ -> true)

  let notChar c = satisfy ((<>) c) |> as_ ("not '" + (string c) + "'")

  let takeWhile (p: _ -> bool) = takeWhile monoid BMPString.takeWhile BMPString.length BMPString.skip p

  let takeRest = takeRest monoid BMPString.length BMPString.skip

  let takeText = takeText monoid BMPString.length BMPString.skip List.fold

  let pchar c = elem ((=) c) (Some ("'" + (string c) + "'"))

  let pstring (BMP s) =
    takeWith (BMPString.length s) ((=) s) (Some ("\"" + (BMPString.toString s) + "\""))

  let stringTransform f (BMP s) what =
    let what = match what with | Some s -> Some s | None -> Some "stringTransform(...)"
    takeWith (BMPString.length s) (fun x -> f x = f s) what

  let takeWhile1 p = takeWhile1 monoid BMPString.takeWhile BMPString.length BMPString.skip p

  let private addDigit (a: decimal) c = a * 10M + ((decimal (int64  c)) - 48M)

  let pdecimal = takeWhile1 Char.IsDigit |>> BMPString.fold addDigit 0M

  let signedInt = pchar '-' >>. map (~-) pdecimal <|> (pchar '+' >>. pdecimal) <|> pdecimal

  let scientific = parser {
    let! positive = satisfy (fun c -> c = '-' || c = '+') |>> ((=) '+') <|> ok true
    let! n = pdecimal
    let! s =
      (satisfy ((=) '.') >>. takeWhile Char.IsDigit
      |>> (fun f -> decimal ((string n) + "." + (BMPString.toString f))))
      <|> ok (decimal n)
    let sCoeff = if positive then s else -s
    return!
      satisfy (fun c -> c = 'e' || c = 'E')
      >>. signedInt
      >>= (fun x ->
        if int x > Int32.MaxValue then error ("Exponent too large: " + string s)
        else ok (s * (decimal (Math.Pow(10.0, float x))))) <|> ok sCoeff
  }

  let scan s p = scan monoid BMPString.head BMPString.tail BMPString.take BMPString.length BMPString.skip s p

  let parse p (BMP init) = parse BMPString.skip monoid p init

  let endOfInput = endOfInput BMPString.length

  let phrase m = phrase BMPString.length m

  let parseAll m init = parse (phrase m) init

  let oneOf chars = satisfy (Helper.inClass chars)
  let noneOf chars = satisfy (Helper.inClass chars >> not)

  let alphaNum =
    satisfy (inClass "a-zA-Z")
    <|> satisfy Char.IsNumber
  let letter = satisfy Char.IsLetter

  let stringsSepBy p s =
    cons BMPString.append p ((s >>. sepBy1 BMPString.monoid BMPString.append p s) <|> ok BMPString.monoid.mempty) <|> ok BMPString.monoid.mempty
    |> as_ ("sepBy(" + p.ToString() + "," + s.ToString() + ")")

  let cons m n = cons List.cons m n

  let manySatisfy pred = many BMPString.monoid BMPString.cons (satisfy pred)

  let many p = many List.monoid List.cons p
  let many1 p = many1 List.monoid List.cons p

  let manyTill p q = manyTill List.monoid List.cons p q

  let sepBy1 p s = sepBy1 List.monoid List.cons p s
  let sepBy p s = sepBy List.monoid List.cons p s

  let newline = manySatisfy (fun i -> inClass "\r\n" i || inClass "\r" i || inClass "\n" i);
  let spaces = manySatisfy (fun i -> inClass "\r\n" i || inClass "\r" i || inClass "\n" i || Char.IsWhiteSpace i)

  let pmatch p = pmatch BMPString.substring p
