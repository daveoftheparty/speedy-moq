## 0.0.5 (Aug 7, 2021)
- Cut compile time (time until a user sees the lightbulb to generate moq) in half by reading .csproj files directly instead of trying to build them
- Fixed a bug where other extensions would send diagnostics without a Source, this extension was doing string matching on Source (and assuming it was not null)

## 0.0.4 (Aug 5, 2021)

Initial launch to Marketplace