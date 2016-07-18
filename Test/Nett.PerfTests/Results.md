# Nett.PerfTests.ClassToTomlTablePerfTestsV1+BenchmarkClrToTomlTable
__Basic test to measure conversion of .Net structure to TOML table.__
_18.07.2016 12:22:28_
### System Info
```ini
NBench=NBench, Version=0.3.0.0, Culture=neutral, PublicKeyToken=null
OS=Microsoft Windows NT 6.2.9200.0
ProcessorCount=4
CLR=4.0.30319.42000,IsMono=False,MaxGcGeneration=2
WorkerThreads=32767, IOThreads=4
```

### NBench Settings
```ini
RunMode=Throughput, TestMode=Measurement
NumberOfIterations=3, MaximumRunTime=00:00:01
Concurrent=False
Tracing=False
```

## Data
-------------------

### Totals
Metric |           Units |             Max |         Average |             Min |          StdDev |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
[Counter] ToClrTableCounter |      operations |        6.609,00 |        6.609,00 |        6.609,00 |            0,00 |

### Per-second Totals
Metric |       Units / s |         Max / s |     Average / s |         Min / s |      StdDev / s |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
[Counter] ToClrTableCounter |      operations |        6.620,73 |        6.597,79 |        6.558,99 |           33,79 |

### Raw Data
#### [Counter] ToClrTableCounter
Run # |      operations |  operations / s | ns / operations |
---------------- |---------------- |---------------- |---------------- |
1 |        6.609,00 |        6.613,65 |      151.202,33 |
2 |        6.609,00 |        6.620,73 |      151.040,77 |
3 |        6.609,00 |        6.558,99 |      152.462,50 |


# Nett.PerfTests.ReadAndMapTomlFileV1+Benchmark
__Basic test to measure parsing and mapping of TOML files__
_18.07.2016 12:22:37_
### System Info
```ini
NBench=NBench, Version=0.3.0.0, Culture=neutral, PublicKeyToken=null
OS=Microsoft Windows NT 6.2.9200.0
ProcessorCount=4
CLR=4.0.30319.42000,IsMono=False,MaxGcGeneration=2
WorkerThreads=32767, IOThreads=4
```

### NBench Settings
```ini
RunMode=Throughput, TestMode=Measurement
NumberOfIterations=3, MaximumRunTime=00:00:01
Concurrent=False
Tracing=False
```

## Data
-------------------

### Totals
Metric |           Units |             Max |         Average |             Min |          StdDev |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
[Counter] TestCounter |      operations |        2.806,00 |        2.806,00 |        2.806,00 |            0,00 |

### Per-second Totals
Metric |       Units / s |         Max / s |     Average / s |         Min / s |      StdDev / s |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
[Counter] TestCounter |      operations |        2.822,62 |        2.816,93 |        2.806,07 |            9,41 |

### Raw Data
#### [Counter] TestCounter
Run # |      operations |  operations / s | ns / operations |
---------------- |---------------- |---------------- |---------------- |
1 |        2.806,00 |        2.822,62 |      354.280,66 |
2 |        2.806,00 |        2.822,10 |      354.345,90 |
3 |        2.806,00 |        2.806,07 |      356.370,33 |


# Nett.PerfTests.ReadTomlFileV1+Benchmark
__Basic test to measure parsing of TOML files__
_18.07.2016 12:22:46_
### System Info
```ini
NBench=NBench, Version=0.3.0.0, Culture=neutral, PublicKeyToken=null
OS=Microsoft Windows NT 6.2.9200.0
ProcessorCount=4
CLR=4.0.30319.42000,IsMono=False,MaxGcGeneration=2
WorkerThreads=32767, IOThreads=4
```

### NBench Settings
```ini
RunMode=Throughput, TestMode=Measurement
NumberOfIterations=3, MaximumRunTime=00:00:01
Concurrent=False
Tracing=False
```

## Data
-------------------

### Totals
Metric |           Units |             Max |         Average |             Min |          StdDev |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
[Counter] TestCounter |      operations |        3.659,00 |        3.659,00 |        3.659,00 |            0,00 |

### Per-second Totals
Metric |       Units / s |         Max / s |     Average / s |         Min / s |      StdDev / s |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
[Counter] TestCounter |      operations |        3.683,74 |        3.679,40 |        3.676,37 |            3,86 |

### Raw Data
#### [Counter] TestCounter
Run # |      operations |  operations / s | ns / operations |
---------------- |---------------- |---------------- |---------------- |
1 |        3.659,00 |        3.683,74 |      271.463,00 |
2 |        3.659,00 |        3.678,07 |      271.881,83 |
3 |        3.659,00 |        3.676,37 |      272.007,11 |


# Nett.PerfTests.WriteTomlTablePerfTestsV1+BenchmarkWriteTomlTable
__Basic test to measure writing of a TomlTable structure to a stream.__
_18.07.2016 12:22:55_
### System Info
```ini
NBench=NBench, Version=0.3.0.0, Culture=neutral, PublicKeyToken=null
OS=Microsoft Windows NT 6.2.9200.0
ProcessorCount=4
CLR=4.0.30319.42000,IsMono=False,MaxGcGeneration=2
WorkerThreads=32767, IOThreads=4
```

### NBench Settings
```ini
RunMode=Throughput, TestMode=Measurement
NumberOfIterations=3, MaximumRunTime=00:00:01
Concurrent=False
Tracing=False
```

## Data
-------------------

### Totals
Metric |           Units |             Max |         Average |             Min |          StdDev |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
[Counter] WriteTomlTableV1Counter |      operations |       25.716,00 |       25.716,00 |       25.716,00 |            0,00 |

### Per-second Totals
Metric |       Units / s |         Max / s |     Average / s |         Min / s |      StdDev / s |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
[Counter] WriteTomlTableV1Counter |      operations |       26.025,74 |       25.856,34 |       25.725,70 |          153,73 |

### Raw Data
#### [Counter] WriteTomlTableV1Counter
Run # |      operations |  operations / s | ns / operations |
---------------- |---------------- |---------------- |---------------- |
1 |       25.716,00 |       26.025,74 |       38.423,50 |
2 |       25.716,00 |       25.817,57 |       38.733,31 |
3 |       25.716,00 |       25.725,70 |       38.871,63 |


