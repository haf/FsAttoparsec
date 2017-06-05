``` ini

BenchmarkDotNet=v0.10.6, OS=Mac OS X 10.11
Processor=Intel Core i7-4870HQ CPU 2.50GHz (Haswell), ProcessorCount=8
Frequency=10000000 Hz, Resolution=100.0000 ns, Timer=UNKNOWN
  [Host]     : Mono 4.8.1 (Stable 4.8.1.0/22a39d7), 64bit 
  DefaultJob : Mono 4.8.1 (Stable 4.8.1.0/22a39d7), 64bit 


```
 |     Method |              file |           Mean |        Error |       StdDev | Scaled | ScaledSD |
 |----------- |------------------ |---------------:|-------------:|-------------:|-------:|---------:|
 |    **FParsec** | **05.feature-pieces** |    **80,514.7 us** |  **1,604.85 us** |  **3,205.05 us** |   **1.00** |     **0.00** |
 | Attoparsec | 05.feature-pieces | 1,389,026.1 us | 26,654.33 us | 28,519.83 us |  17.28 |     0.77 |
 |    **FParsec** |           **numbers** |       **572.9 us** |     **11.36 us** |     **27.21 us** |   **1.00** |     **0.00** |
 | Attoparsec |           numbers |             NA |           NA |           NA |      ? |        ? |
 |    **FParsec** |        **twitter100** |     **2,468.7 us** |     **48.31 us** |     **72.31 us** |   **1.00** |     **0.00** |
 | Attoparsec |        twitter100 |    65,116.2 us |  1,244.52 us |  1,383.28 us |  26.40 |     0.94 |

Benchmarks with issues:
  JSONParsing.Attoparsec: DefaultJob [file=numbers]
