# Windvale 0.1 preview release ceremony

This runbook creates the first official Release Envelope 1 only after its
format, approval records, threat model, and selected source state have passed
the required Windows/Linux gates. It is intentionally not an unattended CI
recipe: accepting custody of the first root is a project-owner action.

Do not run the key-generation examples inside the repository, a synchronized
folder, an ordinary CI runner, or a directory captured by backup/logging policy
that has not been approved for signing keys.

## 1. Prepare custody locations

Use two separately controlled encrypted locations:

- an offline root location that is disconnected after policy signing; and
- a release-signing location with narrower operational access.

Retain at least two protected offline root backups in separate physical failure
domains. Record the public key identity out of band. Never copy either private
PEM into Windvale, GitHub Actions, a release, issue, task, terminal transcript,
or support bundle.

## 2. Generate separate keys

From a trusted checkout using the qualified Node.js runtime, create empty
directories outside the repository and run one role at a time:

```text
node Tools/Release/Create-Release-Envelope.mjs generate-key root <empty-offline-root-directory>
node Tools/Release/Create-Release-Envelope.mjs generate-key release <empty-release-key-directory>
```

The command writes an unencrypted PKCS #8 private PEM with mode `0600` where the
host supports POSIX modes, an SPKI public PEM, and a key-id text file. Therefore
the containing volume and offline custody are required protections. Inspect the
reported identities through a second trusted channel before use.

If project policy requires a hardware token, threshold custody, passphrase-
encrypted key, or audited signing appliance, stop: those require a successor
decision and adapter rather than exporting a weaker PEM.

## 3. Sign the root policy offline

Transfer only these inputs to the offline root environment:

- `Distribution/Releases/Windvale-Root-Policy-1.json`;
- the root private key; and
- the release public key.

Create an empty policy output directory and run:

```text
node Tools/Release/Create-Release-Envelope.mjs create-root \
  <Windvale-Root-Policy-1.json> \
  <root-private.pem> \
  <release-public.pem> \
  <empty-policy-output-directory>
```

Return only `Root-Policy.txt`, `Root-Policy.sig`, the root public PEM, and their
recorded hashes. Disconnect the root environment. The root private key must not
be present during ordinary release construction.

The project owner must explicitly approve the root-key identity before it is
added as the public Windvale trust anchor. Committing a public key is not enough:
its identity must also be distributed through an independent authenticated
channel.

## 4. Select and qualify one exact state

Record the full commit and tree identities:

```text
git rev-parse HEAD
git rev-parse HEAD^{tree}
```

The worktree must be clean and both configured remotes must point at that
commit. Dispatch the complete `Verify` workflow explicitly for this commit.
Ordinary affected-owner runs do not satisfy the final release gate.

Retain canonical Windows and Linux qualification reports naming the selected
commit, tree, retirement-plan digest, workflow/run/job identities, conclusion,
and exact final suite summaries. Report capture must not modify the selected
source state.

## 5. Stage the required artifacts

Create one ordinary staging root outside the checkout containing exactly the
artifacts required by
[`Windvale-Release-Envelope.md`](../../Specifications/Windvale-Release-Envelope.md):

- deterministic source archive for the selected revision/tree;
- qualified `windvale-0.1.0-windows-x64.zip` and
  `windvale-0.1.0-linux-x64.tar.gz` stable installers from the explicit release
  installer input (never the `0.1.0-dev.1` artifacts);
- exact WVDB Query Bundle 1;
- one canonical bundle of the approval and two launch records;
- license inventory;
- bounded provenance;
- Stage 0 recovery reference;
- canonical Windows and Linux qualification reports; and
- the offline verifier source.

For every artifact, record its role, target, staging-relative source path,
release path below `Artifacts/`, exact byte length, and lowercase SHA-256 in one
canonical `windvale-release-envelope-input-1` JSON file. The version is
`0.1.0`, channel is `preview`, sequence is `1`, and revision/tree must equal the
selected Git objects.

Build each deterministic artifact twice before signing. A mismatch stops the
ceremony; do not bless one arbitrary result.

## 6. Create the signed envelope

With the root-policy directory, release private key, canonical input, staging
root, and an existing empty output directory:

```text
node Tools/Release/Create-Release-Envelope.mjs create-release \
  <root-policy-directory> \
  <release-private.pem> \
  <release-input.json> \
  <artifact-staging-root> \
  <empty-release-output-directory>
```

Construct it a second time from the same inputs and compare every file. No
private key may appear in either output.

## 7. Verify offline

On a clean machine with networking disabled, use the independently obtained
root public key:

```text
node Tools/Release/Verify-Release-Envelope.mjs verify \
  <trusted-root-public.pem> \
  <release-directory> \
  1
```

Then run the installer, package, approval-record, Stage 0 reference, and
qualification-report verifiers named by the release documentation. A valid
release signature alone is insufficient.

## 8. Tag and publish

Only after the owner accepts the root identity, the envelope is reproducible,
offline verification passes, and final qualification is green:

1. create a signed annotated `v0.1.0` tag at the selected commit using the
   project's configured Git signing identity;
2. push the immutable tag to both remotes;
3. publish release assets and notes against that exact tag;
4. publish root-key identity, manifest identity, and verification instructions
   through the independent authenticated channel; and
5. retain the complete envelope and recovery evidence in both durable archives.

Never move or reuse `v0.1.0`. A packaging correction uses a new version and
sequence. If the key or selected state is uncertain, stop publication rather
than weakening the gate.

The Ed25519 release-envelope key is not automatically a Git tag-signing key.
Confirm the independent Git signing identity and public verification path before
tagging; do not export or convert the envelope private key merely to satisfy
`git tag -s`.
