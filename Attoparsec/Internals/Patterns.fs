/// Module for pattern matching with active patterns.
module Attoparsec.Patterns

/// Convert the input string into a Basic-Multilingual Plane string.
let (|BMP|) str =
  BMPString.ofString str

/// Convert the input byte array into a ByteString.
let (|ByteString|) array =
  ByteString.create array