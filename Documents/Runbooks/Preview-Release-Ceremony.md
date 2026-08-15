# Windvale 0.1 preview release ceremony

This runbook creates the first official Release Envelope 1 only after its
format, approval records, threat model, and selected source state have passed
the required Windows/Linux gates. It is intentionally not an unattended CI
recipe: accepting custody of the first root is a project-owner action.

Do not run the key-generation examples inside the repository, a synchronized
folder, an ordinary CI runner, or a directory captured by backup/logging policy
that has not been approved for signing keys.

## 1. Prepare custody locations

Use two separately controlled protected storage locations. Protection may be
provided by an encrypted containing volume or by the Decision 0566 encrypted
private-key wrapper on owner-controlled detachable storage:

- an offline root location that is disconnected after policy signing; and
- a release-signing location with narrower operational access.

Retain at least two protected offline root backups in separate physical failure
domains. Record the public key identity out of band. Never copy either private
key into Windvale, GitHub Actions, a release, issue, task, terminal transcript,
or support bundle.

For the first detachable-disk ceremony under Decision 0566, use `Keys A` for
the primary protected root key, `Keys B` for its protected backup, and `Release`
for the protected operational release and Git tag-signing keys. The backing
storage and host access must be owner-controlled. Move the detached `Keys B`
backing object to a separate durable failure domain before publication; three
virtual disks on one host are not three physical backups.

## 2. Generate separate keys

From a trusted checkout using the qualified Node.js runtime, create empty
directories outside the repository and run one role at a time:

```text
node Tools/Release/Create-Release-Envelope.mjs generate-key root <empty-offline-root-directory> --key-passphrase
node Tools/Release/Create-Release-Envelope.mjs generate-key release <empty-release-key-directory> --key-passphrase
```

The command reads and confirms the passphrase through a masked terminal prompt;
do not place it in an argument, environment variable, redirected file, task, or
transcript. It writes a scrypt/AES-256-GCM-protected `.wvkey` containing PKCS #8
bytes, an SPKI public PEM, and a key-id text file. The private file uses mode
`0600` where the host supports POSIX modes. Inspect the reported public
identities through a second trusted channel before use.

Copy the complete protected root output to `Keys B`, compare every byte, then
detach the backup. Do not independently generate the backup: both root copies
must recover the same public identity. Use a different passphrase for the
release key.

If project policy requires a hardware token, threshold custody, passphrase-
manager integration, or audited signing appliance, stop: those require another
successor decision and adapter. Decision 0566 is the approved software
passphrase adapter; do not export its decrypted PKCS #8 bytes.

## 3. Sign the root policy offline

Transfer only these inputs to the offline root environment:

- `Distribution/Releases/Windvale-Root-Policy-1.json`;
- the root private key; and
- the release public key.

Create an empty policy output directory and run:

```text
node Tools/Release/Create-Release-Envelope.mjs create-root \
  <Windvale-Root-Policy-1.json> \
  <root-private.wvkey> \
  <release-public.pem> \
  <empty-policy-output-directory> \
  --key-passphrase
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
  <release-private.wvkey> \
  <release-input.json> \
  <artifact-staging-root> \
  <empty-release-output-directory> \
  --key-passphrase
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
`git tag -s`. For the first release, create a separate passphrase-protected
Ed25519 SSH signing key on the `Release` disk and register only its public key as
a GitHub SSH signing key.
