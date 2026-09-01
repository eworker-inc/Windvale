# Windvale evidence records

> Status: Current evidence-record guide
> Authority: Normative for new repository evidence records
> Last reviewed: 2026-08-31

Evidence records answer a narrow question: what exact claim was checked, for
which source state, with which inputs, tools, hosts, and result? They are not a
second progress dashboard and they do not redefine specifications.

## When to create a record

Create one record for a completed qualification run, reproducibility result,
performance measurement, artifact publication, independent review, or other
claim that needs exact reconstruction. Keep ordinary local test output out of
the repository unless it supports a durable claim.

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

## Naming and lifecycle

Use `YYYY-MM-DD-Short-Claim.json` for a standalone record, or keep a tool-owned
record beside its canonical manifest when that is the clearer owner. Evidence
is immutable once used for an accepted or released claim. If a correction is
needed, add a replacement record and link the old record to it rather than
silently changing the claimed run.

The large historical evidence pages under `Documents/Project/` remain valid
archives. New work should prefer small records and generated summaries so a
developer or AI agent can load only the evidence relevant to the current task.
