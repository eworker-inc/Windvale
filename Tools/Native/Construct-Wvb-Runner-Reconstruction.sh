#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Wvb-Runner-Reconstruction.sh <existing-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
candidate_root="$repository_root/Artifacts/Native-Wvb-Runner-Candidate"
if [[ $output_root == "$candidate_root" ]]; then
    echo 'The WVB-runner reconstruction must use a separate output directory.' >&2
    exit 64
fi
if [[ -L $1 ]]; then
    echo 'The WVB-runner reconstruction output directory must not be a symbolic link.' >&2
    exit 64
fi

source_project="$repository_root/Projects/Tools/Windvale-Wvb-Runner.wvproj"
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wvb-runner-reconstruction.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wvb-runner-reconstruction.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT HUP INT TERM

wvb="$output_root/Wvb-Runner.wvb"
object_prefix="$temporary_directory/Object"
object_manifest="$temporary_directory/Object.wvop"
image_prefix="$temporary_directory/Image"
image_manifest="$temporary_directory/Image.wvli"
canonical_prefix="$temporary_directory/Canonical"
canonical_manifest="$temporary_directory/Canonical.wvli"
windows_application="$output_root/windows-x64-wvrun.exe"
linux_application="$output_root/linux-x64-wvrun.elf"

report_failure() {
    local prefix=$1
    [[ ! -f "$temporary_directory/$prefix.out" ]] || cat -- "$temporary_directory/$prefix.out" >&2
    [[ ! -f "$temporary_directory/$prefix.err" ]] || cat -- "$temporary_directory/$prefix.err" >&2
    exit 1
}

verify_file() {
    local file=$1 expected_bytes=$2 expected_sha256=$3 label=$4
    [[ -f $file ]] || { echo "Missing $label: $file" >&2; return 1; }
    [[ $(wc -c < "$file") -eq $expected_bytes ]] || {
        echo "The $label byte length is invalid." >&2
        return 1
    }
    printf '%s  %s\n' "$expected_sha256" "$file" |
        sha256sum --check --strict --quiet || {
            echo "The $label digest is invalid." >&2
            return 1
        }
}

node "$repository_root/Tools/Native/Build-Current-Split-Project-Wvb.mjs" \
    "$source_project" "$wvb" || exit 1
verify_file "$wvb" 482767 \
    fc4724c7756f22eb52dd6ed4da9737a865e14ea4d52df1de69fc10236970ff4f \
    'WVB-runner module' || exit 1

"$repository_root/Tools/Native/Stage-Compiler-Wvb.sh" \
    "$wvb" "$object_prefix" "$object_manifest" \
    >"$temporary_directory/Stage.out" 2>"$temporary_directory/Stage.err" ||
    report_failure Stage
grep -Fqx \
    'native x64 staging status=Complete object-bytes=5899132 chunks=11 manifest-bytes=156' \
    "$temporary_directory/Stage.out" || report_failure Stage

"$repository_root/Tools/Native/Link-Staged-Compiler-Wvo.sh" \
    "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" \
    >"$temporary_directory/Link.out" 2>"$temporary_directory/Link.err" ||
    report_failure Link
grep -Fqx \
    'segmented compiler image staging status=Complete image-bytes=5889164 entry-offset=150541 chunks=7 manifest-bytes=112' \
    "$temporary_directory/Link.out" || report_failure Link

"$repository_root/Tools/Native/Transport-Compiler-Image.sh" \
    "$image_prefix" "$image_manifest" "$canonical_prefix" "$canonical_manifest" \
    >"$temporary_directory/Transport.out" 2>"$temporary_directory/Transport.err" ||
    report_failure Transport
grep -Fqx \
    'compiler image transport status=Complete image-bytes=5889164 entry-offset=150541 chunks=2 manifest-bytes=52' \
    "$temporary_directory/Transport.out" || report_failure Transport

"$repository_root/Tools/Native/Package-Hosted-Wvb.sh" image 5 \
    "$wvb" "$canonical_prefix" 2 150541 "$windows_application" windows \
    >"$temporary_directory/Windows-Package.out" \
    2>"$temporary_directory/Windows-Package.err" || report_failure Windows-Package
verify_file "$windows_application" 5907456 \
    2721b80158cf4825919be5a6b5c58cfa40d417dc802d5bf27b2584b822ad817b \
    'Windows WVB-runner application' || exit 1

"$repository_root/Tools/Native/Package-Hosted-Wvb.sh" image 5 \
    "$wvb" "$canonical_prefix" 2 150541 "$linux_application" linux \
    >"$temporary_directory/Linux-Package.out" \
    2>"$temporary_directory/Linux-Package.err" || report_failure Linux-Package
verify_file "$linux_application" 5906432 \
    611cfbf9fd95e9b29df4a38e3ac392dc9eea87b760b81ff572bad8af6f235eae \
    'Linux WVB-runner application' || exit 1

echo 'native WVB runner reconstruction status=Complete artifacts=3'
