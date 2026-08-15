#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Wvdb-Approval-Records.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
records=$repository_root/Distribution/Applications/Wvdb-Query
verifier=$repository_root/Tools/Release/Verify-Wvdb-Approval-Records.mjs
approval=Windvale-Wvdb-Query.wvapproval
windows=Windvale-Wvdb-Query.windows-x64.wvlaunch
linux=Windvale-Wvdb-Query.linux-x64.wvlaunch
inspector_records=$repository_root/Distribution/Applications/Wvb-Inspector
inspector_approval=Windvale-Wvb-Inspector.wvapproval
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-wvdb-approval.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-wvdb-approval.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

for directory in Copy Extra Capability Writable Target Approval-Identity Truncated; do
    mkdir -- "$work/$directory" || exit 1
    cp -- "$records/$approval" "$work/$directory/$approval" || exit 1
    cp -- "$records/$windows" "$work/$directory/$windows" || exit 1
    cp -- "$records/$linux" "$work/$directory/$linux" || exit 1
done
mkdir -- "$work/Inspector-Capability" || exit 1
cp -- "$inspector_records/$inspector_approval" \
    "$work/Inspector-Capability/$inspector_approval" || exit 1

echo 'native application approval step=verify-wvdb-source item=1/10'
node "$verifier" verify "$records" >/dev/null || exit $?

echo 'native application approval step=verify-wvdb-copy item=2/10'
node "$verifier" verify "$work/Copy" >/dev/null || exit $?

echo 'native application approval step=verify-inspector-source item=3/10'
node "$verifier" verify-inspector "$inspector_records" >/dev/null || exit $?

echo 'native application approval step=reject-inspector-capability-substitution item=4/10'
sed -i 's/file\.read_bytes/file.write_bytes/' \
    "$work/Inspector-Capability/$inspector_approval" || exit 1
if node "$verifier" verify-inspector "$work/Inspector-Capability" >/dev/null 2>&1; then exit 1; fi

echo 'native application approval step=reject-extra-approval item=5/10'
printf '%s\n' 'approve 5 network.connect ambient-network' >>"$work/Extra/$approval"
if node "$verifier" verify "$work/Extra" >/dev/null 2>&1; then exit 1; fi

echo 'native application approval step=reject-wvdb-capability-substitution item=6/10'
sed -i 's/console\.write_line/console.write/' "$work/Capability/$approval" || exit 1
if node "$verifier" verify "$work/Capability" >/dev/null 2>&1; then exit 1; fi

echo 'native application approval step=reject-writable-provider item=7/10'
sed -i 's/fixed-read-only-object/mutable-directory-object/' "$work/Writable/$windows" || exit 1
if node "$verifier" verify "$work/Writable" >/dev/null 2>&1; then exit 1; fi

echo 'native application approval step=reject-target-substitution item=8/10'
cp -- "$work/Target/$linux" "$work/Target/$windows" || exit 1
if node "$verifier" verify "$work/Target" >/dev/null 2>&1; then exit 1; fi

echo 'native application approval step=reject-approval-identity-substitution item=9/10'
sed -i 's/3c4a968745cde9d5073c67c6c453443d54c74e779b509c2f00131b4d47e8ef71/0000000000000000000000000000000000000000000000000000000000000000/' \
    "$work/Approval-Identity/$linux" || exit 1
if node "$verifier" verify "$work/Approval-Identity" >/dev/null 2>&1; then exit 1; fi

echo 'native application approval step=reject-truncated-record item=10/10'
printf '%s\n' 'windvale-launch-record 1' >"$work/Truncated/$windows"
if node "$verifier" verify "$work/Truncated" >/dev/null 2>&1; then exit 1; fi

echo 'native application approval status=Passed cases=10 applications=2 records=4 capabilities=10 targets=2'
