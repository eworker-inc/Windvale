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
verify() {
    local path=$1 bytes=$2 digest=$3
    [[ -f $path ]] || return 1
    [[ $(wc -c < "$path") -eq $bytes ]] || return 1
    printf '%s  %s\n' "$digest" "$path" | sha256sum --check --status
}
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
"$script_directory/Assemble-Wva.sh" "$repository_root/Operating-System/Kernel/X64-Application-Start-User-Copy.wva" "$work/Start-Copy.wvo" >/dev/null || exit $?
verify "$work/Start-Copy.wvo" 799 74978b1f6124517b44205cba52aaf6c161cf5d00e39ff9ab3ad883d527c87ddb || exit $?
"$script_directory/Assemble-Wva.sh" "$repository_root/Tests/Native/X64-Application-Start-User-Copy-Self-Test.wva" "$work/Start-Copy-Test.wvo" >/dev/null || exit $?
verify "$work/Start-Copy-Test.wvo" 1432 4a7b3fb803e8cea12a2c828ca1947f8ca90d554ad44c0eb7bbfa8a73c7dd691d || exit $?
"$script_directory/Link-Wvo.sh" 0 Main "$work/Start-Copy-Test.bin" "$work/Start-Copy-Test.wvo" "$work/Start-Copy.wvo" >/dev/null || exit $?
verify "$work/Start-Copy-Test.bin" 4288 19411b99859049d7453bd17c3d473e0141122213b39d9c9f4be5356c6b495cc1 || exit $?
"$script_directory/Package-Console.sh" windows-x64-console-v1 "$work/Start-Copy-Test.bin" 0 "$work/Start-Copy-Test.exe" >/dev/null || exit $?
verify "$work/Start-Copy-Test.exe" 6144 cf4e8f6b531a2770c318e445e646ca776b6f8e167e7d569a92b3a8e8fcbda904 || exit $?
"$script_directory/Package-Console.sh" linux-x64-console-v1 "$work/Start-Copy-Test.bin" 0 "$work/Start-Copy-Test.elf" >/dev/null || exit $?
verify "$work/Start-Copy-Test.elf" 12400 2cb4b5cedef3d82483a13f60e8be3ed6df9f63c3566abd894aa4da42ff5fbaaa || exit $?
"$work/Start-Copy-Test.elf" >/dev/null
[[ $? -eq 47 ]] || exit 1
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
echo 'native os application launch status=Passed projects=7 native-leaves=1 behavior=6 cases=52 local-result=47 cross-host-images=Verified'
