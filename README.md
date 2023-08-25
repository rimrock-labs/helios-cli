# ☀️ Helios

This is the main focus of the Rimrock dev lab!

We are developing a production-grade performance data collection system.  
This system will not only let teams understand what their dotnet applications are doing (at the code level) but also help teams improve their applications by automatically spotting well-known performance bottlenecks.

![image](https://github.com/rimrock-labs/helios-cli/assets/1128553/9bde6a71-5211-415e-bc19-2d1d875aab8a)

## ⚒️ Active Work Progress (as of 8/23/2023)

The below work is geared towards an **ALPHA** release.

- [x] cli - The main user interface. Work includes support for future extensibility for additional commands
- [x] collection/analysis - This is the bulk of the current effort. It includes adding ETW profiling support for CPU, as well as CSV output of the collected stacks.
  - [x] Data Analyzers - dotnet application CPU, Memory Allocations, Exceptions
  - [x] Output Mechanism - PerfView and Speedscope are supported
  - [x] End-to-End plumbing (Host, DI, Configuration, Logging)
  - [x] Additional data streams (Memory allocations, Exceptions)
  - [ ] Additional data format,
    - [x] PerfView (XML)
    - [x] Speedscope (JSON)
    - [ ] A binary (graph-like) data format

`helios-cli` is also fully functional at the command line to both collect and analyze,

```PowerShell
.\helios-cli.exe collect --output-directory "{pwd}\Collections\{guid}" --duration "00:00:10" --symbol-store-cache "D:\symbols" --output-format "speedscope" --data-analyzer "CPU" --verbose
```

## 🛣️ Roadmap

ALPHA -> BETA -> v1.0

The near future goals include,

**BETA**
- [ ] Documentation
- [ ] Tests

**v1.0**
- [ ] A binary (graph-like) data format (v1.0)
- [ ] Monitoring functionality (v1.0)
  - [ ] Performance counter collection using PDH (Windows)
  - [ ] Trigger support (collect data when X breaches Y)
  - [ ] REST API for extensiblity
- [ ] Ability to view collected data (web based) (v1.0)
  - [ ] Flamegraphs

**+v1.0**
- [ ] Memory dump analysis support (+v1.0)
- [ ] Data aggregation (+v1.0)
- [ ] Automatic issue analysis (+v1.0)

## ⭐ Contributing

Would like to contribute? Awesome!
Please start by opening an issue or joining the discussion.
