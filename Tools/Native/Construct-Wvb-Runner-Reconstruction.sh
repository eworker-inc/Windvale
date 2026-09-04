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

echo 'native WVB runner reconstruction step=build status=Started'
node "$repository_root/Tools/Native/Build-Current-Split-Project-Wvb.mjs" \
    "$source_project" "$wvb" || exit 1
verify_file "$wvb" 1040878 \
    4e50301efe5e2260608eb994f21ece89e83ad102aac28cebb705d35d06e3d86b \
    'WVB-runner module' || exit 1

echo 'native WVB runner reconstruction step=stage status=Started'
"$repository_root/Tools/Native/Stage-Compiler-Wvb.sh" \
    "$wvb" "$object_prefix" "$object_manifest" \
    >"$temporary_directory/Stage.out" 2>"$temporary_directory/Stage.err" ||
    report_failure Stage
grep -Fqx \
    'native x64 staging status=Complete object-bytes=10547710 chunks=15 manifest-bytes=204' \
    "$temporary_directory/Stage.out" || report_failure Stage

echo 'native WVB runner reconstruction step=link status=Started'
"$repository_root/Tools/Native/Link-Staged-Compiler-Wvo.sh" \
    "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" \
    >"$temporary_directory/Link.out" 2>"$temporary_directory/Link.err" ||
    report_failure Link
grep -Fqx \
    'segmented compiler image staging status=Complete image-bytes=10529580 entry-offset=150541 chunks=11 manifest-bytes=160' \
    "$temporary_directory/Link.out" || report_failure Link

echo 'native WVB runner reconstruction step=transport status=Started'
"$repository_root/Tools/Native/Transport-Compiler-Image.sh" \
    "$image_prefix" "$image_manifest" "$canonical_prefix" "$canonical_manifest" \
    >"$temporary_directory/Transport.out" 2>"$temporary_directory/Transport.err" ||
    report_failure Transport
grep -Fqx \
    'compiler image transport status=Complete image-bytes=10529580 entry-offset=150541 chunks=3 manifest-bytes=64' \
    "$temporary_directory/Transport.out" || report_failure Transport

echo 'native WVB runner reconstruction step=package-windows status=Started'
"$repository_root/Tools/Native/Package-Hosted-Wvb.sh" image 5 \
    "$wvb" "$canonical_prefix" 3 150541 "$windows_application" windows \
    >"$temporary_directory/Windows-Package.out" \
    2>"$temporary_directory/Windows-Package.err" || report_failure Windows-Package
verify_file "$windows_application" 10547712 \
    8942e7c0a17182ff15ed79eaf63f7aeb8a8ab7cd4cde5015cd489612c3494972 \
    'Windows WVB-runner application' || exit 1

echo 'native WVB runner reconstruction step=package-linux status=Started'
"$repository_root/Tools/Native/Package-Hosted-Wvb.sh" image 5 \
    "$wvb" "$canonical_prefix" 3 150541 "$linux_application" linux \
    >"$temporary_directory/Linux-Package.out" \
    2>"$temporary_directory/Linux-Package.err" || report_failure Linux-Package
verify_file "$linux_application" 10547200 \
    4b9f7ea9eb30e4aa9b713f17d6065a413b4585a27eb8c62e021e95375e27436e \
    'Linux WVB-runner application' || exit 1

echo 'native WVB runner reconstruction status=Complete artifacts=3'
