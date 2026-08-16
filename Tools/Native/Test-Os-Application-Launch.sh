#!/usr/bin/env bash
set -uo pipefail
[[ $# -eq 0 ]] || exit 64
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-os-application-launch.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-os-application-launch.*) rm -f -- "$work"/*; rmdir -- "$work" ;;
        *) return 1 ;;
    esac
}
trap cleanup EXIT
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Application-Launch-Policy.wvproj" "$work/Policy.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Application-Start-Request.wvproj" "$work/Request.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Application-Launch.wvproj" "$work/Test.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Application-Start-Request.wvproj" "$work/Request-Test.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Application-Start-User-Copy.wvproj" "$work/Copy-Policy.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Application-Start-User-Copy.wvproj" "$work/Copy-Test.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Service-Launch-Policy.wvproj" "$work/Service-Policy.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Service-Launch.wvproj" "$work/Service-Test.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Operating-System/Windvale-Os-Application-Machine-Construction-Policy.wvproj" "$work/Machine-Policy.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$repository_root/Projects/Tests/Windvale-Native-Test-Os-Application-Machine-Construction.wvproj" "$work/Machine-Test.wvb" >/dev/null || exit $?
"$script_directory/Run-Wvb.sh" "$work/Test.wvb" >"$work/Run.out" 2>"$work/Run.err" || exit $?
[[ $(<"$work/Run.out") == 'Result: 42' ]] || exit 1
[[ ! -s $work/Run.err ]] || exit 1
"$script_directory/Run-Wvb.sh" "$work/Request-Test.wvb" >"$work/Request-Run.out" 2>"$work/Request-Run.err" || exit $?
[[ $(<"$work/Request-Run.out") == 'Result: 44' ]] || exit 1
[[ ! -s $work/Request-Run.err ]] || exit 1
"$script_directory/Run-Wvb.sh" "$work/Copy-Test.wvb" >"$work/Copy-Run.out" 2>"$work/Copy-Run.err" || exit $?
[[ $(<"$work/Copy-Run.out") == 'Result: 46' ]] || exit 1
[[ ! -s $work/Copy-Run.err ]] || exit 1
"$script_directory/Run-Wvb.sh" "$work/Service-Test.wvb" >"$work/Service-Run.out" 2>"$work/Service-Run.err" || exit $?
[[ $(<"$work/Service-Run.out") == 'Result: 45' ]] || exit 1
[[ ! -s $work/Service-Run.err ]] || exit 1
"$script_directory/Run-Wvb.sh" "$work/Machine-Test.wvb" >"$work/Machine-Run.out" 2>"$work/Machine-Run.err" || exit $?
[[ $(<"$work/Machine-Run.out") == 'Result: 43' ]] || exit 1
[[ ! -s $work/Machine-Run.err ]] || exit 1
echo 'native os application launch status=Passed projects=7 behavior=5 cases=42'
