# ☀️ Helios

This is the main focus of the Rimrock dev lab!

We are looking to develop a production-grade performance data collection system.  
This system will not only let teams understand what their dotnet applications are doing (at the code level) but also help teams improve their applications by automatically spotting well-known performance bottlenecks.

We are just coming online so stay tuned!

## ⚒️ Active Work Areas (as of 8/11/2023)

The below work is geared towards an **ALPHA** release.

- [x] cli - The main user interface. Work includes support for future extensibility for additional commands
- [x] collection/analysis - This is the bulk of the current effort. It includes adding ETW profiling support for CPU, as well as CSV output of the collected stacks.
  - [x] Data Analyzers - just CPU for now
  - [x] Output Mechanism - just CSV for now
  - [x] End-to-End plumbing (Host, DI, Configuration, Logging)
  - [ ] Additional data streams (Memory allocations, Exceptions)
  - [ ] Additional data format,
    - [ ] PerfView XML
    - [ ] Chromium (?)
    - [ ] Speedscope (?)
    - [ ] A binary (graph-like) data format

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

If you have suggestions or ideas about any of the above, please join the discussion!
