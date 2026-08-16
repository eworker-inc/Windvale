# Windvale OS boot service composition policy 1

## Status and scope

Boot service composition policy 1 is the first implemented-candidate admission
link between the live process-policy object and the version and limit envelopes
selected from the shared filesystem, bounded-operation, and network-authority
contracts. Its version token is compiled directly into the portable
process-policy WVB used by Probe 40. The focused owners for those three shared
contracts independently prove their complete behavior; the boot policy does
not import or duplicate those implementations.

Before the process foundation returns token 97, it requires policy-version token
1. That token selects this exact fixed envelope:

- filesystem contract version 1 with a 65,536-byte maximum transfer;
- an operation queue limit of four with one slot reserved for control work; and
- IPv4 or IPv6 authority with a nonzero port and one or more of the four
  defined direction rights.

A missing or different policy token prevents the process policy from reaching
its published success token. Full field validation remains in the independent
contract owners so their code is not duplicated into the fixed 768 KiB
supervisor window. This proves an exact contract selection at the boot-policy
boundary; it does not launch a filesystem or network service, create a socket,
parse a disk, or publish a dynamic syscall.

## Limits

The fixed values are construction evidence, not public configuration. Checked
start-request decoding, dynamic provider processes, IPC queue/resource-domain
binding, provider restart, FAT32, link devices, packet/transport services, and
guest application capability binding remain required for the stable service
gate.
