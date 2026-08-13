#!/usr/bin/env bash
set -uo pipefail

if [[ $# -lt 1 || $# -gt 2 ]]; then
    echo 'Usage: ./Tools/Native/Build-Wvb.sh <project.wvproj> [output.wvb]' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Front-Door"
if ! (cd -- "$artifact_root" && sha256sum --check --strict --quiet SHA256SUMS); then
    echo 'The native-front-door artifact inventory is invalid.' >&2
    exit 1
fi

project_input=$1
project_directory=$(CDPATH= cd -- "$(dirname -- "$project_input")" && pwd -P) || exit 1
project_path="$project_directory/$(basename -- "$project_input")"
if [[ $project_path != *.wvproj ]]; then
    echo 'The native build input must use the .wvproj extension.' >&2
    exit 64
fi

workspace_path="$repository_root/Windvale.wvws"
if [[ ! -f $workspace_path ]]; then
    echo 'The native workspace marker is missing.' >&2
    exit 1
fi
if [[ -L $repository_root ]] || [[ -n $(find "$repository_root" -type l -print -quit) ]]; then
    echo 'The native workspace must not contain symbolic links.' >&2
    exit 1
fi

if [[ $# -eq 2 ]]; then
    output_input=$2
    output_directory=$(CDPATH= cd -- "$(dirname -- "$output_input")" && pwd -P) || exit 1
    output_path="$output_directory/$(basename -- "$output_input")"
else
    output_path="${project_path%.wvproj}.wvb"
fi
if [[ $output_path != *.wvb ]]; then
    echo 'The native build output must use the .wvb extension.' >&2
    exit 64
fi

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-build.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-build.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

candidate_path="$temporary_directory/Candidate.wvb"
"$artifact_root/linux-x64/wvbuild.elf" \
    --workspace "$workspace_path" --project "$project_path" "$candidate_path"
result=$?
if [[ $result -eq 0 ]]; then
    "$artifact_root/linux-x64/wvpublish.elf" "$candidate_path" "$output_path"
    result=$?
fi
exit "$result"
