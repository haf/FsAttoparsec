namespace Attoparsec

/// Result from a parse – can be either Done, Fail or Partial.
/// https://hackage.haskell.org/package/attoparsec-0.13.1.0/docs/Data-Attoparsec-ByteString.html#t:IResult
type ParseResult<'i, 'r> =
  /// The parse failed. The i parameter is the input that had not yet been consumed when the failure occurred. The [String] is a list of contexts in which the error occurred. The String is the message describing the error, if any.
  | Fail of input:'i * stack:string list * message:string
  /// Supply this continuation with more input so that the parser can resume. To indicate that no more input is available, pass an empty string to the continuation.
  /// Note: if you get a Partial result, do not call its continuation more than once.
  | Partial of kont:('i -> ParseResult<'i, 'r>)
  /// The parse succeeded. The i parameter is the input that had not yet been consumed (if any) when the parse succeeded.
  | Done of remainder:'i * result:'r

  with
    member x.map (f) =
      match x with
      | Fail (input, stack, message) ->
        Fail (input, stack, message)
      | Partial k ->
        Partial (fun s -> (k s).map(f))
      | Done (input, result) ->
        Done (input, f result)

    member x.feed (m: Monoid<_>, s) =
      match x with
      | Fail _ ->
        x
      | Partial k ->
        k s
      | Done(input, result) ->
        Done(m.mappend(input, s), result)

    member x.Done_(m: Monoid<_>) =
      match x with
      | Fail _
      | Done _ ->
        x
      | Partial _ ->
        x.feed(m, m.mempty)

module ParseResult =

  /// Map the value of the parse result with f.
  let inline map f (result: ParseResult<_, _>) = result.map f

  let inline feed m s (result: ParseResult<_, _>) = result.feed(m, s)

  let inline done_ m (result: ParseResult<_, _>) = result.Done_(m)

  let inline toOption (result: ParseResult<_, _>) =
    match result with
    | Fail _
    | Partial _ ->
      None
    | Done(_, result) ->
      Some result

  let inline toChoice (result: ParseResult<_, _>) =
    match result with
    | Fail (_, _, message) ->
      Choice2Of2 message
    | Partial _ ->
      Choice2Of2 "incomplete input"
    | Done (_, result) ->
      Choice1Of2 result