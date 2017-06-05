# FsAttoparsec

[![License][license-image]][license-url]

What is it? A fast, binary parser, ported from [Attoparsec](https://github.com/bos/attoparsec) and [scala-attoparsec](https://github.com/ekmett/scala-attoparsec).

Why not FParsec? Because that is a string parser, not a binary parser, so you'll have trouble e.g. parsing binary attachments to HTTP requests with it. However, you can parse strings with FsAttoparsec.

From [Hackage][hackage-docs] adopted for F#:

 - While attoparsec can consume input incrementally, FParsec cannot. Incremental input is a huge deal for efficient and secure network and system programming, since it gives much more control to users of the library over matters such as resource usage and the I/O model to use.
 - Much of the performance advantage of attoparsec is gained via high-performance parsers such as takeWhile and string. If you use complicated combinators that return lists of bytes or characters, there is less performance difference between the two libraries.
 - attoparsec is specialised to deal only with strict BmpString input. Efficiency concerns rule out both lists and lazy BmpStrings. The usual use for lazy bytestrings would be to allow consumption of very large input without a large footprint. For this need, attoparsec's incremental input provides an excellent substitute, with much more control over when input takes place.
 - FParsec parsers can produce more helpful error messages than attoparsec parsers. This is a matter of focus: attoparsec avoids the extra book-keeping in favour of higher performance.

### Incremental input

attoparsec supports incremental input, meaning that you can feed it a BmpString that represents only part of the expected total amount of data to parse. If your parser reaches the end of a fragment of input and could consume more input, it will suspend parsing and return a Partial continuation.

Supplying the Partial continuation with a bytestring will resume parsing at the point where it was suspended, with the bytestring you supplied used as new input at the end of the existing input. You must be prepared for the result of the resumed parse to be another Partial continuation.

To indicate that you have no more input, supply the Partial continuation with an empty BmpString.

Remember that some parsing combinators will not return a result until they reach the end of input. They may thus cause Partial results to be returned.

If you do not need support for incremental input, consider using the parseOnly function to run your parser. It will never prompt for more input.

Note: incremental input does not imply that attoparsec will release portions of its internal state for garbage collection as it proceeds. Its internal representation is equivalent to a single BmpString: if you feed incremental input to a parser, it will require memory proportional to the amount of input you supply. (This is necessary to support arbitrary backtracking.)

### Performance considerations

If you write an attoparsec-based parser carefully, it can be realistic to expect it to perform similarly to a hand-rolled C parser (measuring megabytes parsed per second).

To actually achieve high performance, there are a few guidelines that it is useful to follow.

Use the ByteString-oriented parsers whenever possible, e.g. takeWhile1 instead of many1 anyWord8. There is about a factor of 100 difference in performance between the two kinds of parser.

For very simple byte-testing predicates, write them by hand instead of using inClass or notInClass. For instance, both of these predicates test for an end-of-line byte, but the first is much faster than the second:

let endOfLine_fast w =
  w = 13 || w = 10

let endOfLine_slow =
  inClass "\r\n"

Make active use of benchmarking and profiling tools to measure, find the problems with, and improve the performance of your parser.

## Usage

FsAttoparsec is available on nuget as `Attoparsec`.

```fsharp

```

## API

### `[<AutoOpen>] Attoparsec.Parser`

 - `(>>=): Parser<_,_> -> _ -> Parser<_,_>`

## Differences from the Haskell variant

 - `ByteString` -> `BmpString`
 - `pure` -> `Parser.ok`
 - `return` -> `Parser.ok`
 - `mempty` -> `Parser.zero`
 - `mappend` -> `ParseResult.feed`
 - `<$>` -> `<!>`

[hackage-docs]: https://hackage.haskell.org/package/attoparsec-0.13.1.0/docs/Data-Attoparsec-ByteString.html
[license-url]: https://github.com/haf/FsAttoparsec/blob/master/LICENSE
[license-image]: https://img.shields.io/github/license/haf/FsAttoparsec.svg
