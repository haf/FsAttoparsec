﻿namespace Attoparsec

open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts

/// BMP: Basic Multilingual Plane
[<CustomEquality; CustomComparison; Serializable; Struct>]
type BMPString(array: char[], offset: int, count: int) =
  new (array: char[]) = BMPString(array, 0, array.Length)

  member x.Array = array
  member x.Offset = offset
  member x.Count = count

  static member Compare (a:BMPString, b:BMPString) =
    let x, o, l = a.Array, a.Offset, a.Count
    let x', o', l' = b.Array, b.Offset, b.Count
    if o = o' && l = l' && x = x' then 0
    elif x = x' then
      if o = o' then if l < l' then -1 else 1
      else if o < o' then -1 else 1
    else
      if l < l' then -1
      elif l > l' then 1
      else
        let left, right = x.[o .. (o + l - 1)], x'.[o' .. (o' + l' - 1)]
        if left = right then 0 elif left < right then -1 else 1

  override x.Equals(other) =
    match other with
    | :? BMPString as other' -> BMPString.Compare(x, other') = 0
    | _ -> false

  override x.GetHashCode() = hash (x.Array,x.Offset,x.Count)

  member x.GetEnumerator() =
    if x.Count = 0 then
      { new IEnumerator<_> with
          member self.Current = invalidOp "!"
        interface System.Collections.IEnumerator with
          member self.Current = invalidOp "!"
          member self.MoveNext() = false
          member self.Reset() = ()
        interface System.IDisposable with
          member self.Dispose() = () }
    else
      let segment = x.Array
      let minIndex = x.Offset
      let maxIndex = x.Offset + x.Count - 1
      let currentIndex = ref <| minIndex - 1
      { new IEnumerator<_> with
          member self.Current =
            if !currentIndex < minIndex then
              invalidOp "Enumeration has not started. Call MoveNext."
            elif !currentIndex > maxIndex then
              invalidOp "Enumeration already finished."
            else segment.[!currentIndex]
        interface System.Collections.IEnumerator with
          member self.Current =
            if !currentIndex < minIndex then
              invalidOp "Enumeration has not started. Call MoveNext."
            elif !currentIndex > maxIndex then
              invalidOp "Enumeration already finished."
            else box segment.[!currentIndex]
          member self.MoveNext() =
            if !currentIndex < maxIndex then
              incr currentIndex
              true
            else false
          member self.Reset() = currentIndex := minIndex - 1
        interface System.IDisposable with
          member self.Dispose() = () }

  interface System.IComparable with
    member x.CompareTo(other) =
      match other with
      | :? BMPString as other' -> BMPString.Compare(x, other')
      | _ -> invalidArg "other" "Cannot compare a value of another type."

  interface System.Collections.Generic.IEnumerable<char> with
    member x.GetEnumerator() = x.GetEnumerator()
    member x.GetEnumerator() = x.GetEnumerator() :> IEnumerator

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module BMPString =

  let empty = BMPString([||])

  let singleton c = BMPString(Array.create 1 c, 0, 1)

  let create arr = BMPString(arr, 0, arr.Length)

  let findIndex pred (bs:BMPString) =
    Array.FindIndex(bs.Array, bs.Offset, bs.Count, Predicate<_>(pred))

  let ofSeq s = let arr = Array.ofSeq s in BMPString(arr, 0, arr.Length)

  let ofList l = BMPString(Array.ofList l, 0, l.Length)

  let ofString (s:string) = s.ToCharArray() |> create

  let toArray (bs:BMPString) =
    if bs.Count = 0 then [||]
    else bs.Array.[bs.Offset..(bs.Offset + bs.Count - 1)]

  let toSeq (bs:BMPString) = bs :> seq<char>

  let toList (bs:BMPString) = List.ofSeq bs

  let toString (bs:BMPString): string = System.String(bs.Array, bs.Offset, bs.Count)

  let isEmpty (bs:BMPString) =
    Contract.Requires(bs.Count >= 0)
    bs.Count <= 0

  let length (bs:BMPString) =
    Contract.Requires(bs.Count >= 0)
    bs.Count

  let index (bs:BMPString) pos =
    Contract.Requires(bs.Offset + pos <= bs.Count)
    bs.Array.[bs.Offset + pos]

  let head (bs:BMPString) =
    if bs.Count <= 0 then
      failwith "Cannot take the head of an empty BMPString."
    else bs.Array.[bs.Offset]

  let tail (bs:BMPString) =
    Contract.Requires(bs.Count >= 1)
    if bs.Count = 1 then empty
    else BMPString(bs.Array, bs.Offset + 1, bs.Count - 1)

  let append a b =
    if isEmpty a then b
    elif isEmpty b then a
    else
      let x, o, l = a.Array, a.Offset, a.Count
      let x', o', l' = b.Array, b.Offset, b.Count
      let s = sizeof<char>
      let buffer = Array.zeroCreate<char> (l + l')
      Buffer.BlockCopy(x, o * s, buffer, 0, l*s)
      Buffer.BlockCopy(x', o' * s, buffer, l*s, l'*s)
      BMPString(buffer, 0, l+l')

  let cons hd (bs:BMPString) =
    let hd = singleton hd
    if length bs = 0 then hd
    else append hd bs

  let fold f seed bs =
    let rec loop bs acc =
      if isEmpty bs then acc
      else
        let hd, tl = head bs, tail bs
        loop tl (f acc hd)
    loop bs seed

  let split pred (bs:BMPString) =
    if isEmpty bs then empty, empty
    else
      let index = findIndex pred bs
      if index = -1 then bs, empty
      else
        let count = index - bs.Offset
        BMPString(bs.Array, bs.Offset, count),
        BMPString(bs.Array, index, bs.Count - count)

  let span pred bs = split (not << pred) bs

  let splitAt n (bs:BMPString) =
    Contract.Requires(n >= 0)
    if isEmpty bs then empty, empty
    elif n <= 0 then empty, bs
    elif n >= bs.Count then bs, empty
    else
      let x,o,l = bs.Array, bs.Offset, bs.Count
      BMPString(x, o, n), BMPString(x, o + n, l - n)

  let skip n bs = splitAt n bs |> snd

  let skipWhile pred bs = span pred bs |> snd

  let skipUntil pred bs = split pred bs |> snd

  let take n bs = splitAt n bs |> fst


  /// | Consume input as long as the predicate returns 'True', and return
  /// the consumed input.
  ///
  /// This parser does not fail.  It will return an empty string if the
  /// predicate returns 'False' on the first byte of input.
  ///
  /// /Note/: Because this parser does not fail, do not use it with
  /// combinators such as 'Control.Applicative.many', because such
  /// parsers loop until a failure occurs.  Careless use will thus result
  /// in an infinite loop.
  let takeWhile pred bs = span pred bs |> fst

  let takeUntil pred bs = split pred bs |> fst

  let substring pos n bs = take n (skip pos bs)

  let monoid = { new Monoid<_> with
    override x.mempty = empty
    override x.mappend(a, b) = append a b }
