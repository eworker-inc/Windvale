#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Package-Format.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-package-format.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-package-format.*)
            rm -f -- "$temporary_directory"/*
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

canonical_project="$repository_root/Projects/Tests/Windvale-Native-Test-Canonical-Package-Text.wvproj"
canonical_wvb="$temporary_directory/Canonical.wvb"
canonical_windows_application="$temporary_directory/Canonical.exe"
canonical_linux_application="$temporary_directory/Canonical.elf"
project="$repository_root/Projects/Tests/Windvale-Native-Test-Package-Manifest.wvproj"
first="$temporary_directory/First.wvb"
second="$temporary_directory/Second.wvb"
windows_application="$temporary_directory/Package-Format.exe"
linux_application="$temporary_directory/Package-Format.elf"
lock_project="$repository_root/Projects/Tests/Windvale-Native-Test-Package-Lock.wvproj"
lock_wvb="$temporary_directory/Lock.wvb"
lock_windows_application="$temporary_directory/Lock.exe"
lock_linux_application="$temporary_directory/Lock.elf"
consistency_project="$repository_root/Projects/Tests/Windvale-Native-Test-Package-Consistency.wvproj"
consistency_wvb="$temporary_directory/Consistency.wvb"
consistency_windows_application="$temporary_directory/Consistency.exe"
consistency_linux_application="$temporary_directory/Consistency.elf"
manifest="$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack"
lock="$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock"
admission_project="$repository_root/Projects/Tests/Windvale-Native-Test-Package-Resource-Admission.wvproj"
admission_wvb="$temporary_directory/Admission.wvb"
admission_windows_application="$temporary_directory/Admission.exe"
admission_linux_application="$temporary_directory/Admission.elf"

"$script_directory/Build-Wvb.sh" "$canonical_project" "$canonical_wvb" >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$canonical_wvb" "$canonical_windows_application" windows >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$canonical_wvb" "$canonical_linux_application" linux >/dev/null || exit $?
"$canonical_linux_application" >/dev/null
[[ $? -eq 42 ]] || exit 1

"$script_directory/Build-Wvb.sh" "$project" "$first" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" "$project" "$second" >/dev/null || exit $?
cmp --silent "$first" "$second" || exit 1

"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$first" "$windows_application" windows >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$first" "$linux_application" linux >/dev/null || exit $?
"$linux_application" >/dev/null
[[ $? -eq 42 ]] || exit 1

"$script_directory/Build-Wvb.sh" "$lock_project" "$lock_wvb" >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$lock_wvb" "$lock_windows_application" windows >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$lock_wvb" "$lock_linux_application" linux >/dev/null || exit $?
"$lock_linux_application" >/dev/null
[[ $? -eq 42 ]] || exit 1

"$script_directory/Build-Wvb.sh" "$consistency_project" "$consistency_wvb" >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$consistency_wvb" "$consistency_windows_application" windows >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$consistency_wvb" "$consistency_linux_application" linux >/dev/null || exit $?
"$consistency_linux_application" "$manifest" "$lock" >/dev/null
[[ $? -eq 42 ]] || exit 1

"$script_directory/Build-Wvb.sh" "$admission_project" "$admission_wvb" >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$admission_wvb" "$admission_windows_application" windows >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$admission_wvb" "$admission_linux_application" linux >/dev/null || exit $?
"$admission_linux_application" "$repository_root" >/dev/null
[[ $? -eq 42 ]] || exit 1

echo 'native package format status=Passed result=42 modules=5 builds=6 groups=58 cross-host-images=10'
