#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Wv-Linker-Reconstruction.sh <existing-separate-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
candidate_root=$(CDPATH= cd -- "$repository_root/Artifacts/Native-Wv-Linker-Candidate" && pwd -P) || exit 1
if [[ $output_root == "$candidate_root" ]]; then
    echo 'The Wv-Linker reconstruction must use a separate output directory.' >&2
    exit 64
fi

verify_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3 label=$4
    local actual_bytes digest_line actual_sha256
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

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wv-linker-reconstruction.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wv-linker-reconstruction.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

wvb="$output_root/Wv-Linker.wvb"
wvo="$output_root/Wv-Linker.wvo"
fragment="$output_root/Wv-Linker.bin"
windows_application="$output_root/Wv-Linker.exe"
linux_application="$output_root/Wv-Linker.elf"
object_prefix="$temporary_directory/Object"
object_manifest="$temporary_directory/Object.wvop"
image_prefix="$temporary_directory/Image"
image_manifest="$temporary_directory/Image.wvli"
canonical_prefix="$temporary_directory/Canonical"
canonical_manifest="$temporary_directory/Canonical.wvli"

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Linker/Windvale-Wv-Linker.wvproj" "$wvb" \
    >"$temporary_directory/Build.out" 2>"$temporary_directory/Build.err" || exit $?
verify_file "$wvb" 135740 \
    02f727a8ce2d6826c8414cada0933c7d5a54893ea061621d08147984c3d6f874 \
    'Wv-Linker WVB' || exit 1

"$script_directory/Lower-Wvb-To-Wvo.sh" "$wvb" "$wvo" \
    >"$temporary_directory/Lower.out" 2>"$temporary_directory/Lower.err" || exit $?
verify_file "$wvo" 1786271 \
    0141219773241e8780e2520f30ab8377914bf89a72f57da091871ac40d68a287 \
    'Wv-Linker WVO' || exit 1

"$script_directory/Stage-Compiler-Wvb.sh" \
    "$wvb" "$object_prefix" "$object_manifest" \
    >"$temporary_directory/Stage.out" 2>"$temporary_directory/Stage.err" || exit $?
"$script_directory/Link-Staged-Compiler-Wvo.sh" \
    "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" \
    >"$temporary_directory/Link.out" 2>"$temporary_directory/Link.err" || exit $?
"$script_directory/Transport-Compiler-Image.sh" \
    "$image_prefix" "$image_manifest" "$canonical_prefix" "$canonical_manifest" \
    >"$temporary_directory/Transport.out" 2>"$temporary_directory/Transport.err" || exit $?

transport_line=$(sed -n '/^compiler image transport status=Complete /p' "$temporary_directory/Transport.out")
native_entry=$(printf '%s\n' "$transport_line" | sed -n 's/^.* entry-offset=\([0-9][0-9]*\) chunks=.*$/\1/p')
fragment_count=$(printf '%s\n' "$transport_line" | sed -n 's/^.* chunks=\([1-8]\) manifest-bytes=.*$/\1/p')
if [[ $native_entry != 884630 || $fragment_count != 1 ]]; then
    echo 'The transported Wv-Linker entry or fragment count is invalid.' >&2
    exit 1
fi
cp -- "$canonical_prefix.chunk-0" "$fragment" || exit 1
verify_file "$fragment" 1777781 \
    d30e0c4dce7159bf98c546a0200e8b541797612ab67d6f21e3d8ee876af27480 \
    'Wv-Linker linked fragment' || exit 1

"$script_directory/Package-Hosted-Wvb.sh" image 4 \
    "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" "$windows_application" windows \
    >"$temporary_directory/Windows.out" 2>"$temporary_directory/Windows.err" || exit $?
verify_file "$windows_application" 1796608 \
    08744f3cacf71280ea757dcdf6509ee3770d5536b08e5b3984a438cb6123fb78 \
    'Windows Wv-Linker application' || exit 1

"$script_directory/Package-Hosted-Wvb.sh" image 4 \
    "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" "$linux_application" linux \
    >"$temporary_directory/Linux.out" 2>"$temporary_directory/Linux.err" || exit $?
verify_file "$linux_application" 1798144 \
    8a220bfd6c7ef684897583e728419ecd6d383c8e8cf40094edbcfb695e3d6d7a \
    'Linux Wv-Linker application' || exit 1
[[ -x $linux_application ]] || {
    echo 'The Linux Wv-Linker application is not executable.' >&2
    exit 1
}

echo 'native Wv-Linker reconstruction status=Complete artifacts=5'
