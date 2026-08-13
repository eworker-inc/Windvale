#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-os-kernel-target.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-os-kernel-target.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

module="$temporary_directory/Hello-World.wvb"
object="$temporary_directory/01-kernel.wvo"
existing="$temporary_directory/Existing.wvo"
unsupported="$temporary_directory/Unsupported.wvo"
unsupported_module="$repository_root/Artifacts/Native-Os-Probe-Memory-Object-Producer-Candidate/Os-Probe-Memory-Object-Producer.wvb"
malformed="$temporary_directory/Malformed.wvb"
malformed_output="$temporary_directory/Malformed.wvo"
target="$repository_root/Artifacts/Native-Os-Kernel-Target-Candidate/linux-x64-os-kernel-target.elf"

verify_identity() {
    local path=$1
    local bytes=$2
    local digest=$3
    [[ -f $path && $(wc -c < "$path") -eq $bytes ]] &&
        printf '%s  %s\n' "$digest" "$path" |
            sha256sum --check --strict --quiet
}

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Operating-System/Windvale-Os-Kernel-Markers.wvproj" "$module" \
    >/dev/null 2>&1 || exit 1
verify_identity "$module" 1484 \
    7a0ef0dedba2a72177239c54fd670be82968e7c5156855bf36be7412da6d656c || exit 1
"$script_directory/Lower-Os-Kernel-Wvb.sh" "$module" "$object" \
    >/dev/null 2>&1 || exit 1
verify_identity "$object" 12134 \
    bf13c1b103c297e87f4aa14f5bf7eba57ef2a30caa21b4c67dba34abc0a7f7a8 || exit 1
"$script_directory/Verify-Wvo.sh" "$object" >/dev/null 2>&1 || exit 1

printf '%s\n' preserved > "$existing"
existing_sha256=$(sha256sum -- "$existing") || exit 1
existing_sha256=${existing_sha256%% *}
if "$script_directory/Lower-Os-Kernel-Wvb.sh" "$module" "$existing" \
    >/dev/null 2>&1; then
    exit 1
fi
printf '%s  %s\n' "$existing_sha256" "$existing" |
    sha256sum --check --strict --quiet || exit 1

"$script_directory/Verify-Wvb.sh" "$unsupported_module" >/dev/null 2>&1 || exit 1
if "$script_directory/Lower-Os-Kernel-Wvb.sh" \
    "$unsupported_module" "$unsupported" >/dev/null 2>&1; then
    exit 1
fi
[[ ! -e $unsupported ]] || exit 1

direct_rejection() {
    local fixture=$1
    base64 --decode "$repository_root/$fixture" > "$malformed" || return 1
    if "$target" "$malformed" "$malformed_output" >/dev/null 2>&1; then
        return 1
    fi
    [[ ! -e $malformed_output ]] || return 1
    rm -f -- "$malformed"
}

direct_rejection 'Tests/Native/Malformed-Wvb/Truncated.wvb.b64' || exit 1
direct_rejection 'Tests/Native/Malformed-Wvb/Trailing.wvb.b64' || exit 1
direct_rejection 'Tests/Native/Malformed-Wvb/Bad-Utf8.wvb.b64' || exit 1
direct_rejection 'Tests/Native/Malformed-Wvb/Typed-Declared-Maximum-Stack.wvb.b64' || exit 1

echo 'Tests: 7, Passed: 7, Failed: 0'
