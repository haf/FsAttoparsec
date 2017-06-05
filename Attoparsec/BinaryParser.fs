namespace Attoparsec

open Helper
open Patterns

module Binary =

  module ParseResult =
    let feed s (result: ParseResult<_, _>) = ParseResult.feed ByteString.monoid s result
    let done_ (result: ParseResult<_, _>) = ParseResult.done_ ByteString.monoid result

  let parseOnly parser (ByteString input) =
    parseOnly ByteString.skip ByteString.monoid parser input

  let get = get ByteString.skip

  let endOfChunk = endOfChunk ByteString.length

  let wantInput = wantInput ByteString.length

  let atEnd = atEnd ByteString.length

  let ensureSuspended st n = ensureSuspended ByteString.length ByteString.range st n

  let ensure n = ensure ByteString.length ByteString.range n

  let elem p what = elem ByteString.length ByteString.head ByteString.range p what

  let satisfy p = satisfy ByteString.length ByteString.head ByteString.range p

  let skip p what = skip ByteString.length ByteString.head ByteString.range p what

  let skipWhile p = skipWhile ByteString.monoid ByteString.skipWhile ByteString.skip ByteString.length p

  let takeWith n p what = takeWith ByteString.length ByteString.range n p what

  let take n = take ByteString.length ByteString.range n

  let anyByte = satisfy (fun _ -> true)

  let notByte c = (satisfy ((<>) c)) |> as_ ("not '" + (string c) + "'")

  let takeWhile (p: _ -> bool) =
    takeWhile ByteString.monoid ByteString.takeWhile ByteString.length ByteString.skip p

  let takeRest = takeRest ByteString.monoid ByteString.length ByteString.skip

  let takeText = takeText ByteString.monoid ByteString.length ByteString.skip List.fold

  let pbyte c = elem ((=) c) (Some ("'" + (string c) + "'"))
  let bytes (ByteString b) =
    takeWith (ByteString.length b) ((=) b) (Some (b.ToString()))

  let takeWhile1 p =
    takeWhile1 ByteString.monoid ByteString.takeWhile ByteString.length ByteString.skip p

  let scan s p = scan ByteString.monoid ByteString.head ByteString.tail ByteString.take ByteString.length ByteString.skip s p

  let parse p (ByteString init) = parse ByteString.skip ByteString.monoid p init

  let endOfInput = endOfInput ByteString.length

  let phrase m = phrase ByteString.length m

  let parseAll m init = parse (phrase m) init

  let cons m n = cons ByteString.cons m n

  let manySatisfy pred = many ByteString.monoid ByteString.cons (satisfy pred)

  let many p = many List.monoid List.cons p
  let many1 p = many1 List.monoid List.cons p

  let manyTill p q = manyTill List.monoid List.cons p q

  let sepBy1 p s = sepBy1 List.monoid List.cons p s
  let sepBy p s = sepBy List.monoid List.cons p s

  let pmatch p = pmatch ByteString.range p