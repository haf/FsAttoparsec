module Attoparsec.Tests.Attoparsec.RFC2616

// import Control.Applicative
// import Data.Attoparsec.ByteString as P
// import Data.Attoparsec.ByteString.Char8 (char8, endOfLine, isDigit_w8)
// import Data.ByteString (ByteString)
// import Data.Word (Word8)
// import Data.Attoparsec.ByteString.Char8 (isEndOfLine, isHorizontalSpace)

open Attoparsec
module P = Attoparsec.ByteString

let isToken (w: byte) =
  w <= 127uy
  && notInClass "\0-\31()<>@,;:\\\"/[]?={} \t" w

// skipSpaces :: Parser ()
let skipSpaces =
  satisfy isHorizontalSpace *> skipWhile isHorizontalSpace

[<Struct>]
type Request =
  { requestMethod  : ByteString
    requestUri     : ByteString
    requestVersion : ByteString }
  static member create m u v =
    { requestMethod = m
      requestUri = u
      requestVersion = v }

// httpVersion :: Parser ByteString
let httpVersion = "HTTP/" *> P.takeWhile (fun c -> isDigit_w8 c || c == 46)

// requestLine :: Parser Request
let requestLine =
      Request.create
  <!> (takeWhile1 isToken <* char8 ' ')
  <*> (takeWhile1 ((<>) 32) <* char8 ' ')
  <*> (httpVersion <* endOfLine)

[<Struct>]
type Header =
  { headerName  : ByteString
    headerValue : ByteString list }
  static member create name value =
    { headerName = name
      headerValue = value }

// messageHeader :: Parser Header
let messageHeader =
      Header.create
  <!> (P.takeWhile isToken <* char8 ':' <* skipWhile isHorizontalSpace)
  <*> ((:) <!> (takeTill isEndOfLine <* endOfLine)
           <*> (many $ skipSpaces *> takeTill isEndOfLine <* endOfLine))

// request :: Parser (Request, [Header])
let request = (,) <!> requestLine <*> many messageHeader <* endOfLine

type Response =
  { responseVersion : ByteString
    responseCode    : ByteString
    responseMsg     : ByteString
  }

// responseLine :: Parser Response
let responseLine =
       Response.create
  <!> (httpVersion <* char8 ' ')
  <*> (P.takeWhile isDigit_w8 <* char8 ' ')
  <*> (takeTill isEndOfLine <* endOfLine)

// response :: Parser (Response, [Header])
let response =
  (,) <!> responseLine <*> many messageHeader <* endOfLine