#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Console-Packager-Reconstruction.sh <existing-separate-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
ordinary_candidate=$(CDPATH= cd -- "$repository_root/Artifacts/Native-Console-Packager-Candidate" && pwd -P) || exit 1
segmented_candidate=$(CDPATH= cd -- "$repository_root/Artifacts/Native-Console-Segmented-Packager-Candidate" && pwd -P) || exit 1
if [[ $output_root == "$ordinary_candidate" || $output_root == "$segmented_candidate" ]]; then
    echo 'The console-packager reconstruction must not overwrite a live candidate directory.' >&2
    exit 64
fi

ordinary_output="$output_root/Native-Console-Packager-Candidate"
segmented_output="$output_root/Native-Console-Segmented-Packager-Candidate"
if [[ $ordinary_output == "$ordinary_candidate" || $segmented_output == "$segmented_candidate" ]]; then
    echo 'The console-packager reconstruction must not overwrite a live candidate directory.' >&2
    exit 64
fi
mkdir -p -- "$ordinary_output" "$segmented_output" || exit 1
ordinary_output=$(CDPATH= cd -- "$ordinary_output" && pwd -P) || exit 1
segmented_output=$(CDPATH= cd -- "$segmented_output" && pwd -P) || exit 1
if [[ $ordinary_output == "$ordinary_candidate" || $segmented_output == "$segmented_candidate" ]]; then
    echo 'The console-packager reconstruction must not overwrite a live candidate directory.' >&2
    exit 64
fi

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-console-packager-reconstruction.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-console-packager-reconstruction.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

ordinary_wvb="$ordinary_output/Console-Packager.wvb"
ordinary_windows="$ordinary_output/Console-Packager.exe"
ordinary_linux="$ordinary_output/Console-Packager.elf"
segmented_wvb="$segmented_output/Console-Segmented-Packager.wvb"
segmented_windows="$segmented_output/Console-Segmented-Packager.exe"
segmented_linux="$segmented_output/Console-Segmented-Packager.elf"

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Linker/Windvale-Console-Application-Packager.wvproj" \
    "$ordinary_wvb" \
    >"$temporary_directory/Build-Ordinary.txt" 2>"$temporary_directory/Build-Ordinary.err" || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Linker/Windvale-Console-Application-Segmented-Packager.wvproj" \
    "$segmented_wvb" \
    >"$temporary_directory/Build-Segmented.txt" 2>"$temporary_directory/Build-Segmented.err" || exit $?

construct_pair() {
    local name=$1
    local wvb=$2
    local windows=$3
    local linux=$4
    local work_directory="$temporary_directory/$name"
    local object_prefix="$work_directory/Object"
    local object_manifest="$work_directory/Object.wvop"
    local image_prefix="$work_directory/Image"
    local image_manifest="$work_directory/Image.wvli"
    local canonical_prefix="$work_directory/Canonical"
    local canonical_manifest="$work_directory/Canonical.wvli"
    local transport_line
    local native_entry
    local fragment_count

    mkdir -- "$work_directory" || return 1
    "$script_directory/Stage-Compiler-Wvb.sh" \
        "$wvb" "$object_prefix" "$object_manifest" \
        >"$work_directory/Stage.txt" 2>"$work_directory/Stage.err" || return $?
    "$script_directory/Link-Staged-Compiler-Wvo.sh" \
        "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" \
        >"$work_directory/Link.txt" 2>"$work_directory/Link.err" || return $?
    "$script_directory/Transport-Compiler-Image.sh" \
        "$image_prefix" "$image_manifest" "$canonical_prefix" "$canonical_manifest" \
        >"$work_directory/Transport.txt" 2>"$work_directory/Transport.err" || return $?

    transport_line=$(sed -n '/^compiler image transport status=Complete /p' "$work_directory/Transport.txt")
    native_entry=$(printf '%s\n' "$transport_line" | sed -n 's/^.* entry-offset=\([0-9][0-9]*\) chunks=.*$/\1/p')
    fragment_count=$(printf '%s\n' "$transport_line" | sed -n 's/^.* chunks=\([1-8]\) manifest-bytes=.*$/\1/p')
    case "$native_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    case "$fragment_count" in
        [1-8]) ;;
        *) return 1 ;;
    esac

    "$script_directory/Package-Hosted-Wvb.sh" image 5 \
        "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" \
        "$windows" windows \
        >"$work_directory/Windows.txt" 2>"$work_directory/Windows.err" || return $?
    "$script_directory/Package-Hosted-Wvb.sh" image 5 \
        "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" \
        "$linux" linux \
        >"$work_directory/Linux.txt" 2>"$work_directory/Linux.err"
}

construct_pair Ordinary "$ordinary_wvb" "$ordinary_windows" "$ordinary_linux" || exit $?
construct_pair Segmented "$segmented_wvb" "$segmented_windows" "$segmented_linux" || exit $?

verify_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    local label=$4
    local actual_bytes
    local digest_line
    local actual_sha256
    [[ -f $path ]] || return 1
    actual_bytes=$(wc -c < "$path") || return 1
    if [[ $actual_bytes -ne $expected_bytes ]]; then
        echo "The $label identity is invalid." >&2
        return 1
    fi
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    if [[ $actual_sha256 != "$expected_sha256" ]]; then
        echo "The $label identity is invalid." >&2
        return 1
    fi
}

verify_file "$ordinary_wvb" 60797 \
    f4c75495321736bbce22582213133e7cc09157a8439dc198d9848ec95683e89c \
    'ordinary console-packager WVB' || exit 1
verify_file "$ordinary_windows" 708608 \
    0dddbe6cfd38c37e3fd5332567b3323480a5548a6fbeb41b6b50aed0e57ac3d2 \
    'Windows ordinary console-packager application' || exit 1
verify_file "$ordinary_linux" 708608 \
    d399c935e906ab42d7572e337226577055396cb6204766106e21790e22ea43af \
    'Linux ordinary console-packager application' || exit 1
verify_file "$segmented_wvb" 70033 \
    c4941f396f76467cb6455472f7f4711c21a6f65c12c09a9b5f4135987628f20e \
    'segmented console-packager WVB' || exit 1
verify_file "$segmented_windows" 805376 \
    954c4b2aaba56149c21e16e19ca6f16434069513e1d1b3034423dab457635412 \
    'Windows segmented console-packager application' || exit 1
verify_file "$segmented_linux" 806912 \
    8916fb509f81e29dabca7ed0202c0ad250f129e78b70b701630dbfcd55a1d30d \
    'Linux segmented console-packager application' || exit 1

for application in "$ordinary_linux" "$segmented_linux"; do
    if [[ ! -x $application ]]; then
        echo "The Linux application is not executable: $application" >&2
        exit 1
    fi
done

echo 'native console packager reconstruction status=Complete families=2 artifacts=6'
