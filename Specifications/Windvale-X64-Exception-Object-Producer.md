# Windvale x64 exception object producer

## Status and scope

This contract owns the small installer object used by the normal Probe 40 link.
It replaces a frozen Stage 0-produced WVO in the ordinary path without changing
the exception ABI or making privileged x64 instruction encoding part of WVA.
The retained host entry points are:

```text
Tools/Native/Produce-X64-Exception-Object.cmd <output.wvo>
Tools/Native/Produce-X64-Exception-Object.sh <output.wvo>
```

The launchers admit the exact paired native application, require a new `.wvo`
destination in an existing directory, remove a newly created invalid output,
and require the final 483-byte object at SHA-256
`9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c`.

## Object recipe

The hosted Windvale producer uses the verified
[WVO construction boundary](Windvale-Wvo-Object-Construction.md) to emit:

- one 222-byte executable `.text` section aligned to 16 bytes;
- exported function `Windvale_kernel_x64_exception_install` covering that
  section;
- imported functions `Windvale_kernel_x64_exception_13_entry` and
  `Windvale_kernel_x64_exception_6_entry`; and
- two signed relative-i32 relocations, at code offsets 63 and 115, both with
  addend `-4`, targeting exception entries 6 and 13 respectively.

The code installs the retained x64 exception descriptors and returns the same
status as the frozen Stage 0 recipe. Its bytes are architecture-specific data in
this focused producer. They do not define Windvale source semantics, WVA syntax,
or a general machine-code injection facility.

## Fixed verification

The three-case native lane requires exact output plus independent WVO admission,
existing-destination preservation, and invalid-extension rejection without an
output. These expectations are immutable native contract data, so the lane
continues to work after the C# recovery implementation is archived or removed.
The normal Probe 40 lane separately proves that the generated object preserves
the complete linked EFI identity.
