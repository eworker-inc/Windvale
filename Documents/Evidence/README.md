# Windvale evidence records

> Status: Current evidence-record guide
> Authority: Normative for new repository evidence records
> Last reviewed: 2026-09-26

Evidence records answer a narrow question: what exact claim was checked, for
which source state, with which inputs, tools, hosts, and result? They are not a
second progress dashboard and they do not redefine specifications.

## Find existing evidence

Use the generated [historical evidence index](Index.md) to search the older
append-oriented archives by section. It tells you which source owns the claim,
what that source is useful for, and what it cannot prove. Open only the linked
section unless the surrounding history is needed.

## When to create a record

Create one record for a completed qualification run, reproducibility result,
performance measurement, artifact publication, independent review, or other
claim that needs exact reconstruction. Keep ordinary local test output out of
the repository unless it supports a durable claim.

An ordinary change normally needs only a commit or pull-request summary naming
the source revision, checks, results, relevant timings, and limitations. Do not
generate a separate artifact inventory for every check-in. Release, bootstrap,
compiler promotion and qualification retain their required manifests.

Use a short Markdown summary when a person needs interpretation. Put exact,
machine-readable fields in a JSON record conforming to
[`Evidence-Record.schema.json`](Evidence-Record.schema.json). Store large logs
and generated artifacts in their owned artifact location, then link or identify
them from the record.

## Required shape

A record identifies:

- one stable evidence ID and date;
- the subject and exact bounded claim;
- the source commit or other immutable source identity;
- the host, target, tool, and command needed to understand the run;
- relevant inputs and outputs, including size and SHA-256 identity when exact
  bytes matter;
- pass, fail, or incomplete result;
- evidence classes such as machine-verified or independently reproduced;
- known limits and checks deliberately not run; and
- related specifications, decisions, or issues.

Keep secrets, credentials, private paths, mutable local SDK locations, and
unbounded logs out of the record. A digest proves byte identity, not
correctness. A passing command proves only the scope named by that command.

## Compact development records

The existing schema permits omission of `inputs` and `outputs`. Use that form
when an exact source revision and existing artifact manifests already identify
the run. Keep the actual commands, hosts, results, timings and limitations.
For example, a compact record's `source` uses `kind: GitCommit` and the exact
tested commit as its `value`; each `runs` entry names the check and its result.

Do not rehash tracked source already identified by that commit. A pre-commit
run must retain its base plus patch or source snapshot, or be bound to the
eventual commit containing exactly the tested changes. Changes to the inputs
after a passing run invalidate that binding. Untracked supplied tools still
need an immutable identity when the result depends on their bytes; reference
their existing manifest rather than repeating its inventory. Preserve
necessary new artifact identities once, with the run that produced them.

Do not commit per-file hashes of ordinary local logs, machine installations,
or cache contents just to make a record look complete. Detailed inventories
belong to claims that require them, not to all development checks.

## Naming and lifecycle

Use `YYYY-MM-DD-Short-Claim.json` for a standalone record, or keep a tool-owned
record beside its canonical manifest when that is the clearer owner. Evidence
is immutable once used for an accepted or released claim. If a correction is
needed, add a replacement record and link the old record to it rather than
silently changing the claimed run.

An explicitly requested compaction of a routine development record may remove
duplicated inventories when the original record is retained at an identified
immutable Git revision. The compact record must name that revision and path,
keep the claims, commands, results and limitations, and retain or reference the
identities needed to interpret the run. This is an editorial change, not a new
verification result. Do not apply it to signed release manifests, bootstrap
trust records, or qualification records.

The large historical evidence pages under `Documents/Project/` remain valid
archives. New work should prefer small records and generated summaries so a
developer or AI agent can load only the evidence relevant to the current task.
