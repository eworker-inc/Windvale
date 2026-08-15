#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Release-Envelope.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-release-envelope.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-release-envelope.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

creator=$repository_root/Tools/Release/Create-Release-Envelope.mjs
verifier=$repository_root/Tools/Release/Verify-Release-Envelope.mjs
fixture_tool=$repository_root/Tools/Native/Create-Release-Envelope-Fixture.mjs
for directory in \
    Fixture Root-Key Release-Key Other-Root-Key Other-Release-Key Policy \
    First Second Tamper-Artifact Tamper-Manifest Tamper-Root Extra \
    Unsafe-Out Missing-Out Wrong-Key-Out Changed-Out \
    Protected-Fixture Protected-Root-Key Protected-Release-Key Protected-Policy Protected-First \
    Protected-Wrong-Out Protected-Missing-Out Protected-Tampered-Key Protected-Tamper-Out; do
    mkdir -- "$work/$directory" || exit 1
done
test_passphrase=windvale-test-passphrase-1

echo 'native release envelope step=create-key-policy item=1/16'
node "$fixture_tool" create "$work/Fixture" || exit $?
node "$creator" generate-test-key root "$work/Root-Key" >/dev/null || exit $?
node "$creator" generate-test-key release "$work/Release-Key" >/dev/null || exit $?
node "$creator" generate-test-key root "$work/Other-Root-Key" >/dev/null || exit $?
node "$creator" generate-test-key release "$work/Other-Release-Key" >/dev/null || exit $?
[[ $(stat -c '%a' -- "$work/Root-Key/root-private.pem") == 600 ]] || exit 1
[[ $(stat -c '%a' -- "$work/Release-Key/release-private.pem") == 600 ]] || exit 1
node "$creator" create-root \
    "$work/Fixture/Root-Input.json" \
    "$work/Root-Key/root-private.pem" \
    "$work/Release-Key/release-public.pem" \
    "$work/Policy" >/dev/null || exit $?

echo 'native release envelope step=create-first item=2/16'
node "$creator" create-release \
    "$work/Policy" \
    "$work/Release-Key/release-private.pem" \
    "$work/Fixture/Release-Input.json" \
    "$work/Fixture/Sources" \
    "$work/First" >/dev/null || exit $?

echo 'native release envelope step=prove-determinism item=3/16'
node "$creator" create-release \
    "$work/Policy" \
    "$work/Release-Key/release-private.pem" \
    "$work/Fixture/Release-Input.json" \
    "$work/Fixture/Sources" \
    "$work/Second" >/dev/null || exit $?
node "$fixture_tool" compare "$work/First" "$work/Second" >/dev/null || exit $?

echo 'native release envelope step=verify-valid item=4/16'
node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/First" >/dev/null || exit $?
if find "$work/First" -type f -name '*private*' | grep . >/dev/null; then exit 1; fi

echo 'native release envelope step=reject-artifact-tamper item=5/16'
node "$fixture_tool" copy "$work/First" "$work/Tamper-Artifact" || exit $?
printf x >>"$work/Tamper-Artifact/Artifacts/approval.txt"
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/Tamper-Artifact" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-manifest-signature-tamper item=6/16'
node "$fixture_tool" copy "$work/First" "$work/Tamper-Manifest" || exit $?
printf x >>"$work/Tamper-Manifest/Release-Manifest.sig"
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/Tamper-Manifest" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-root-signature-tamper item=7/16'
node "$fixture_tool" copy "$work/First" "$work/Tamper-Root" || exit $?
printf x >>"$work/Tamper-Root/Root-Policy.sig"
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/Tamper-Root" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-wrong-root item=8/16'
if node "$verifier" verify "$work/Other-Root-Key/root-public.pem" "$work/First" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-undeclared-file item=9/16'
node "$fixture_tool" copy "$work/First" "$work/Extra" || exit $?
printf '%s\n' undeclared >"$work/Extra/undeclared.txt"
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/Extra" >/dev/null 2>&1; then exit 1; fi
rm -- "$work/Extra/undeclared.txt" || exit 1
mkdir -- "$work/Extra/Artifacts/undeclared-directory" || exit 1
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/Extra" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-sequence-rollback item=10/16'
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/First" 2 >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-unsafe-path item=11/16'
node "$fixture_tool" mutate-input unsafe-path \
    "$work/Fixture/Release-Input.json" "$work/Unsafe-Input.json" || exit $?
if node "$creator" create-release "$work/Policy" \
    "$work/Release-Key/release-private.pem" "$work/Unsafe-Input.json" \
    "$work/Fixture/Sources" "$work/Unsafe-Out" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-incomplete-profile item=12/16'
node "$fixture_tool" mutate-input missing-profile \
    "$work/Fixture/Release-Input.json" "$work/Missing-Input.json" || exit $?
if node "$creator" create-release "$work/Policy" \
    "$work/Release-Key/release-private.pem" "$work/Missing-Input.json" \
    "$work/Fixture/Sources" "$work/Missing-Out" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-key-and-source-substitution item=13/16'
if node "$creator" create-release "$work/Policy" \
    "$work/Other-Release-Key/release-private.pem" "$work/Fixture/Release-Input.json" \
    "$work/Fixture/Sources" "$work/Wrong-Key-Out" >/dev/null 2>&1; then exit 1; fi
printf x >>"$work/Fixture/Sources/approval-all-approval.txt"
if node "$creator" create-release "$work/Policy" \
    "$work/Release-Key/release-private.pem" "$work/Fixture/Release-Input.json" \
    "$work/Fixture/Sources" "$work/Changed-Out" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=protected-key-roundtrip item=14/16'
node "$fixture_tool" create "$work/Protected-Fixture" >/dev/null || exit $?
printf '%s\n%s\n' "$test_passphrase" "$test_passphrase" | \
    node "$creator" generate-key root "$work/Protected-Root-Key" \
    --key-passphrase >/dev/null || exit $?
printf '%s\n%s\n' "$test_passphrase" "$test_passphrase" | \
    node "$creator" generate-key release "$work/Protected-Release-Key" \
    --key-passphrase >/dev/null || exit $?
[[ -f "$work/Protected-Root-Key/root-private.wvkey" ]] || exit 1
[[ -f "$work/Protected-Release-Key/release-private.wvkey" ]] || exit 1
printf '%s\n' "$test_passphrase" | node "$creator" create-root \
    "$work/Protected-Fixture/Root-Input.json" \
    "$work/Protected-Root-Key/root-private.wvkey" \
    "$work/Protected-Release-Key/release-public.pem" \
    "$work/Protected-Policy" --key-passphrase >/dev/null || exit $?
printf '%s\n' "$test_passphrase" | node "$creator" create-release \
    "$work/Protected-Policy" \
    "$work/Protected-Release-Key/release-private.wvkey" \
    "$work/Protected-Fixture/Release-Input.json" "$work/Protected-Fixture/Sources" \
    "$work/Protected-First" --key-passphrase >/dev/null || exit $?
node "$verifier" verify "$work/Protected-Root-Key/root-public.pem" \
    "$work/Protected-First" >/dev/null || exit $?

echo 'native release envelope step=reject-protected-key-credential-errors item=15/16'
if printf '%s\n' windvale-test-wrong-passphrase | node "$creator" create-release \
    "$work/Protected-Policy" "$work/Protected-Release-Key/release-private.wvkey" \
    "$work/Protected-Fixture/Release-Input.json" "$work/Protected-Fixture/Sources" \
    "$work/Protected-Wrong-Out" --key-passphrase >/dev/null 2>&1; then exit 1; fi
if node "$creator" create-release "$work/Protected-Policy" \
    "$work/Protected-Release-Key/release-private.wvkey" \
    "$work/Protected-Fixture/Release-Input.json" "$work/Protected-Fixture/Sources" \
    "$work/Protected-Missing-Out" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-protected-key-tamper item=16/16'
cp -- "$work/Protected-Root-Key/root-private.wvkey" \
    "$work/Protected-Tampered-Key/root-private.wvkey" || exit $?
printf x >>"$work/Protected-Tampered-Key/root-private.wvkey"
if printf '%s\n' "$test_passphrase" | node "$creator" create-root \
    "$work/Protected-Fixture/Root-Input.json" \
    "$work/Protected-Tampered-Key/root-private.wvkey" \
    "$work/Protected-Release-Key/release-public.pem" \
    "$work/Protected-Tamper-Out" --key-passphrase >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope status=Passed cases=16 signatures=4 artifacts=11 protected-private-keys=2'
