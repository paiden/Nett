# Nett.PerfTests.ReadAndMapTomlFileV1+Benchmark
__Basic test to measure parsing and mapping of TOML files__
_14.07.2016 18:19:49_
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
[Counter] TestCounter |      operations |        2.723,00 |        2.723,00 |        2.723,00 |            0,00 |

### Per-second Totals
Metric |       Units / s |         Max / s |     Average / s |         Min / s |      StdDev / s |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
[Counter] TestCounter |      operations |        2.868,95 |        2.868,37 |        2.867,33 |            0,90 |

### Raw Data
#### [Counter] TestCounter
Run # |      operations |  operations / s | ns / operations |
---------------- |---------------- |---------------- |---------------- |
1 |        2.723,00 |        2.868,84 |      348.572,71 |
2 |        2.723,00 |        2.868,95 |      348.560,17 |
3 |        2.723,00 |        2.867,33 |      348.756,35 |


# Nett.PerfTests.ReadTomlFileV1+Benchmark
__Basic test to measure parsing of TOML files__
_14.07.2016 18:19:58_
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
[Counter] TestCounter |      operations |        3.732,00 |        3.732,00 |        3.732,00 |            0,00 |

### Per-second Totals
Metric |       Units / s |         Max / s |     Average / s |         Min / s |      StdDev / s |
---------------- |---------------- |---------------- |---------------- |---------------- |---------------- |
[Counter] TestCounter |      operations |        3.749,32 |        3.746,66 |        3.742,24 |            3,86 |

### Raw Data
#### [Counter] TestCounter
Run # |      operations |  operations / s | ns / operations |
---------------- |---------------- |---------------- |---------------- |
1 |        3.732,00 |        3.749,32 |      266.715,00 |
2 |        3.732,00 |        3.748,43 |      266.778,58 |
3 |        3.732,00 |        3.742,24 |      267.219,80 |


