namespace Attoparsec

type Monoid<'T> =
  abstract member mempty : 'T
  abstract member mappend : 'T * 'T -> 'T

module List =

  let monoid<'T> =
    { new Monoid<'T list> with
        member this.mempty = []
        member this.mappend(x, y) = List.append x y }

  let cons x xs = x :: xs