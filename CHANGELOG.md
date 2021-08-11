## 0.0.7 (Aug 10, 2021)
- tiny performance improvement, won't keep trolling the file system to find .csproj files for a .cs file that has previously been resolved
- fixes a bug introduced with 0.0.6 where some namespace calculations can blow over/remove previous interface definitions. (As a user: you might have seen the same interface defined in two or more namespaces randomly offer to Generate Moq Setups for all namespaces, then later, for only one of them.)

## 0.0.6 (Aug 9, 2021)

Now handles multiple namespaces -- given an interface `IDealio` that exists in both namespace `Two.Lib.Version1` _**and**_ `Two.Lib.Version2`, you will now be presented with the options:
- Generate Moq setups for namespace Two.Lib.Version1
- Generate Moq setups for namespace Two.Lib.Version2

## 0.0.5 (Aug 7, 2021)
- Cut compile time (time until a user sees the lightbulb to generate moq) in half by reading .csproj files directly instead of trying to build them
- Fixed a bug where other extensions would send diagnostics without a Source, this extension was doing string matching on Source (and assuming it was not null)

## 0.0.4 (Aug 5, 2021)

Initial launch to Marketplace