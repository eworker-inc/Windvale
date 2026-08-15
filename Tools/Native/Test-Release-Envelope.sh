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
    Unsafe-Out Missing-Out Wrong-Key-Out Changed-Out; do
    mkdir -- "$work/$directory" || exit 1
done

echo 'native release envelope step=create-key-policy item=1/13'
node "$fixture_tool" create "$work/Fixture" || exit $?
node "$creator" generate-key root "$work/Root-Key" >/dev/null || exit $?
node "$creator" generate-key release "$work/Release-Key" >/dev/null || exit $?
node "$creator" generate-key root "$work/Other-Root-Key" >/dev/null || exit $?
node "$creator" generate-key release "$work/Other-Release-Key" >/dev/null || exit $?
[[ $(stat -c '%a' -- "$work/Root-Key/root-private.pem") == 600 ]] || exit 1
[[ $(stat -c '%a' -- "$work/Release-Key/release-private.pem") == 600 ]] || exit 1
node "$creator" create-root \
    "$work/Fixture/Root-Input.json" \
    "$work/Root-Key/root-private.pem" \
    "$work/Release-Key/release-public.pem" \
    "$work/Policy" >/dev/null || exit $?

echo 'native release envelope step=create-first item=2/13'
node "$creator" create-release \
    "$work/Policy" \
    "$work/Release-Key/release-private.pem" \
    "$work/Fixture/Release-Input.json" \
    "$work/Fixture/Sources" \
    "$work/First" >/dev/null || exit $?

echo 'native release envelope step=prove-determinism item=3/13'
node "$creator" create-release \
    "$work/Policy" \
    "$work/Release-Key/release-private.pem" \
    "$work/Fixture/Release-Input.json" \
    "$work/Fixture/Sources" \
    "$work/Second" >/dev/null || exit $?
node "$fixture_tool" compare "$work/First" "$work/Second" >/dev/null || exit $?

echo 'native release envelope step=verify-valid item=4/13'
node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/First" >/dev/null || exit $?
if find "$work/First" -type f -name '*private*' | grep . >/dev/null; then exit 1; fi

echo 'native release envelope step=reject-artifact-tamper item=5/13'
node "$fixture_tool" copy "$work/First" "$work/Tamper-Artifact" || exit $?
printf x >>"$work/Tamper-Artifact/Artifacts/approval.txt"
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/Tamper-Artifact" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-manifest-signature-tamper item=6/13'
node "$fixture_tool" copy "$work/First" "$work/Tamper-Manifest" || exit $?
printf x >>"$work/Tamper-Manifest/Release-Manifest.sig"
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/Tamper-Manifest" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-root-signature-tamper item=7/13'
node "$fixture_tool" copy "$work/First" "$work/Tamper-Root" || exit $?
printf x >>"$work/Tamper-Root/Root-Policy.sig"
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/Tamper-Root" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-wrong-root item=8/13'
if node "$verifier" verify "$work/Other-Root-Key/root-public.pem" "$work/First" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-undeclared-file item=9/13'
node "$fixture_tool" copy "$work/First" "$work/Extra" || exit $?
printf '%s\n' undeclared >"$work/Extra/undeclared.txt"
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/Extra" >/dev/null 2>&1; then exit 1; fi
rm -- "$work/Extra/undeclared.txt" || exit 1
mkdir -- "$work/Extra/Artifacts/undeclared-directory" || exit 1
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/Extra" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-sequence-rollback item=10/13'
if node "$verifier" verify "$work/Root-Key/root-public.pem" "$work/First" 2 >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-unsafe-path item=11/13'
node "$fixture_tool" mutate-input unsafe-path \
    "$work/Fixture/Release-Input.json" "$work/Unsafe-Input.json" || exit $?
if node "$creator" create-release "$work/Policy" \
    "$work/Release-Key/release-private.pem" "$work/Unsafe-Input.json" \
    "$work/Fixture/Sources" "$work/Unsafe-Out" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-incomplete-profile item=12/13'
node "$fixture_tool" mutate-input missing-profile \
    "$work/Fixture/Release-Input.json" "$work/Missing-Input.json" || exit $?
if node "$creator" create-release "$work/Policy" \
    "$work/Release-Key/release-private.pem" "$work/Missing-Input.json" \
    "$work/Fixture/Sources" "$work/Missing-Out" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope step=reject-key-and-source-substitution item=13/13'
if node "$creator" create-release "$work/Policy" \
    "$work/Other-Release-Key/release-private.pem" "$work/Fixture/Release-Input.json" \
    "$work/Fixture/Sources" "$work/Wrong-Key-Out" >/dev/null 2>&1; then exit 1; fi
printf x >>"$work/Fixture/Sources/approval-all-approval.txt"
if node "$creator" create-release "$work/Policy" \
    "$work/Release-Key/release-private.pem" "$work/Fixture/Release-Input.json" \
    "$work/Fixture/Sources" "$work/Changed-Out" >/dev/null 2>&1; then exit 1; fi

echo 'native release envelope status=Passed cases=13 signatures=2 artifacts=11 private-keys=External'
