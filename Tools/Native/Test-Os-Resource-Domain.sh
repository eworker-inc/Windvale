#!/usr/bin/env bash
set -uo pipefail
[[ $# -eq 0 ]] || exit 64
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-os-resource-domain.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-os-resource-domain.*) rm -f -- "$work"/*; rmdir -- "$work" ;;
        *) return 1 ;;
    esac
}
trap cleanup EXIT
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Resource-Domain-Policy.wvproj" "$work/Policy.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Resource-Domain.wvproj" "$work/Test.wvb" >/dev/null || exit $?
"$script_directory/Run-Wvb.sh" "$work/Test.wvb" >"$work/Run.out" 2>"$work/Run.err" || exit $?
[[ $(<"$work/Run.out") == 'Result: 42' ]] || exit 1
[[ ! -s $work/Run.err ]] || exit 1
echo 'native os resource domain status=Passed projects=1 behavior=1 cases=2'
