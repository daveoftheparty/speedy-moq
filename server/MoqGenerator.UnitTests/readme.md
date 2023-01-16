# new test coverage workflow

Let's say you need to test MockText more thoroughly... well that requires filling out an object blob in MockTextTests

An easy way to get that object blob is to first test InterfaceStore:

1) create new test under InterfaceStore, and c'mon, give the filename stubs a USEFUL name!
2) run [InterfaceStoreTests](../InterfaceStoreTests.cs)
3) get the failing JSON from step 2... validate it... it's not necessarily in the right shape for MockText, but will help immensely in filling that out