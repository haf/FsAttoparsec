``` ini

BenchmarkDotNet=v0.10.6, OS=Mac OS X 10.11
Processor=Intel Core i7-4870HQ CPU 2.50GHz (Haswell), ProcessorCount=8
Frequency=10000000 Hz, Resolution=100.0000 ns, Timer=UNKNOWN
  [Host]     : Mono 4.8.1 (Stable 4.8.1.0/22a39d7), 64bit 
  DefaultJob : Mono 4.8.1 (Stable 4.8.1.0/22a39d7), 64bit 


```
 |     Method |              file |           Mean |        Error |       StdDev |         Median | Scaled | ScaledSD |
 |----------- |------------------ |---------------:|-------------:|-------------:|---------------:|-------:|---------:|
 |    **FParsec** | **05.feature-pieces** |    **79,755.4 us** |  **1,489.26 us** |  **2,723.20 us** |    **79,999.6 us** |   **1.00** |     **0.00** |
 | Attoparsec | 05.feature-pieces | 1,294,765.5 us | 24,383.71 us | 22,808.53 us | 1,295,128.8 us |  16.25 |     0.62 |
 |    **FParsec** |           **numbers** |       **556.5 us** |     **11.01 us** |     **27.22 us** |       **545.6 us** |   **1.00** |     **0.00** |
 | Attoparsec |           numbers |             NA |           NA |           NA |             NA |      ? |        ? |
 |    **FParsec** |        **twitter100** |     **2,501.9 us** |     **49.25 us** |     **82.28 us** |     **2,494.4 us** |   **1.00** |     **0.00** |
 | Attoparsec |        twitter100 |    54,971.2 us |    704.41 us |    588.22 us |    55,246.1 us |  21.99 |     0.73 |

Benchmarks with issues:
  JSONParsing.Attoparsec: DefaultJob [file=numbers]
