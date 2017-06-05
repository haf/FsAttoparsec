namespace Attoparsec

// * Original Implementation
//     * FSharpx.Collections.ByteString
// https://github.com/fsprojects/FSharpx.Collections/blob/master/src/FSharpx.Collections/ByteString.fs

open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts

/// A wrapper for byte[] that supports comparison and equality checks.
[<CustomEquality; CustomComparison; Serializable; Struct>]
type ByteString(array: byte[], offset: int, count: int) =
  new (array: byte[]) = ByteString(array, 0, array.Length)

  member x.Array = array
  member x.Offset = offset
  member x.Count = count

  static member Compare (a: ByteString, b: ByteString) =
    let x, o, l = a.Array, a.Offset, a.Count
    let x', o', l' = b.Array, b.Offset, b.Count
    if o = o' && l = l' && x = x' then 0
    elif x = x' then
      if o = o' then if l < l' then -1 else 1
      else if o < o' then -1 else 1
    else
      let left, right = x.[o .. (o + l - 1)], x'.[o' .. (o' + l' - 1)]
      if left = right then 0 elif left < right then -1 else 1

  override x.Equals(other) =
    match other with
    | :? ByteString as other' -> ByteString.Compare(x, other') = 0
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
    member x.CompareTo other =
      match other with
      | :? ByteString as other' ->
        ByteString.Compare(x, other')
      | _ ->
        invalidArg "other" "Cannot compare a value of another type."

  interface System.Collections.Generic.IEnumerable<byte> with
    member x.GetEnumerator() = x.GetEnumerator()
    member x.GetEnumerator() = x.GetEnumerator() :> IEnumerator

module ByteString =

  let empty = ByteString([||])

  let singleton c = ByteString(Array.create 1 c, 0, 1)

  let create arr = ByteString(arr, 0, arr.Length)

  let findIndex pred (bs:ByteString) =
    Array.FindIndex(bs.Array, bs.Offset, bs.Count, Predicate<_>(pred))

  let ofArray array = ByteString(array)

  let ofSeq s = let arr = Array.ofSeq s in ByteString(arr, 0, arr.Length)

  let ofList l = ByteString(Array.ofList l, 0, l.Length)

  let toArray (bs: ByteString) =
    if bs.Count = 0 then [||]
    else bs.Array.[ bs.Offset .. (bs.Offset + bs.Count - 1) ]

  let toSeq (bs: ByteString) = bs :> seq<byte>

  let toList (bs: ByteString) = List.ofSeq bs

  let isEmpty (bs: ByteString) =
    Contract.Requires(bs.Count >= 0)
    bs.Count <= 0

  let length (bs: ByteString) =
    Contract.Requires(bs.Count >= 0)
    bs.Count

  let index (bs: ByteString) pos =
    Contract.Requires(bs.Offset + pos <= bs.Count)
    bs.Array.[ bs.Offset + pos ]

  let head (bs: ByteString) =
    if bs.Count <= 0 then
      failwith "Cannot take the head of an empty Binary."
    else bs.Array.[ bs.Offset ]

  let tail (bs:ByteString) =
    Contract.Requires(bs.Count >= 1)
    if bs.Count = 1 then empty
    else ByteString(bs.Array, bs.Offset + 1, bs.Count - 1)

  let append a b =
    if isEmpty a then b
    elif isEmpty b then a
    else
      let x, o, l = a.Array, a.Offset, a.Count
      let x', o', l' = b.Array, b.Offset, b.Count
      let buffer = Array.zeroCreate<byte> (l + l')
      Buffer.BlockCopy(x, o, buffer, 0, l)
      Buffer.BlockCopy(x', o', buffer, l, l')
      ByteString(buffer, 0, l+l')

  let cons hd (bs:ByteString) =
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

  let split pred (bs:ByteString) =
    if isEmpty bs then empty, empty
    else
      let index = findIndex pred bs
      if index = -1 then bs, empty
      else
        let count = index - bs.Offset
        ByteString(bs.Array, bs.Offset, count),
        ByteString(bs.Array, index, bs.Count - count)

  let span pred bs = split (not << pred) bs

  let splitAt n (bs:ByteString) =
    Contract.Requires(n >= 0)
    if isEmpty bs then empty, empty
    elif n <= 0 then empty, bs
    elif n >= bs.Count then bs, empty
    else
      let x,o,l = bs.Array, bs.Offset, bs.Count
      ByteString(x, o, n), ByteString(x, o + n, l - n)

  let skip n bs = splitAt n bs |> snd

  let skipWhile pred bs = span pred bs |> snd

  let skipUntil pred bs = split pred bs |> snd

  let take n bs = splitAt n bs |> fst

  let takeWhile pred bs = span pred bs |> fst

  let takeUntil pred bs = split pred bs |> fst

  let range pos n ba = take n (skip pos ba)

  let monoid =
    { new Monoid<_> with
      override x.mempty = empty
      override x.mappend (a, b) = append a b }