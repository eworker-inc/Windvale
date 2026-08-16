#!/usr/bin/env bash
set -uo pipefail
[[ $# -eq 0 ]] || exit 64
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-os-provider-launch.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-os-provider-launch.*) rm -f -- "$work"/*; rmdir -- "$work" ;;
        *) return 1 ;;
    esac
}
trap cleanup EXIT

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Operating-System/Windvale-Os-Provider-Launch-Transaction-Policy.wvproj" \
    "$work/Policy.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Provider-Launch-Transaction.wvproj" \
    "$work/Transaction-Test.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Provider-Launch-Lifecycle.wvproj" \
    "$work/Lifecycle-Test.wvb" >/dev/null || exit $?

"$script_directory/Run-Wvb.sh" "$work/Transaction-Test.wvb" \
    >"$work/Transaction.out" 2>"$work/Transaction.err" || exit $?
[[ $(<"$work/Transaction.out") == 'Result: 48' ]] || exit 1
[[ ! -s $work/Transaction.err ]] || exit 1
"$script_directory/Run-Wvb.sh" "$work/Lifecycle-Test.wvb" \
    >"$work/Lifecycle.out" 2>"$work/Lifecycle.err" || exit $?
[[ $(<"$work/Lifecycle.out") == 'Result: 49' ]] || exit 1
[[ ! -s $work/Lifecycle.err ]] || exit 1

echo 'native os provider launch transaction status=Passed projects=3 behavior=13 cases=18'
