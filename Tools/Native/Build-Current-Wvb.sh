#!/usr/bin/env bash
set -uo pipefail

if [[ $# -lt 1 || $# -gt 2 ]]; then
    echo 'Usage: ./Tools/Native/Build-Current-Wvb.sh <project.wvproj> [output.wvb]' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
compiler_root="$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate"
build_driver="$compiler_root/linux-x64/wvbuild.elf"

verify_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3 description=$4
    [[ -f $path && ! -L $path ]] || {
        echo "Missing $description: $path" >&2
        return 1
    }
    local actual_bytes actual_sha256
    actual_bytes=$(wc -c < "$path") || return 1
    actual_sha256=$(sha256sum -- "$path" | awk '{ print $1 }') || return 1
    [[ $actual_bytes -eq $expected_bytes && $actual_sha256 == "$expected_sha256" ]] || {
        echo "The $description identity is invalid." >&2
        return 1
    }
}

verify_file "$build_driver" 30380032 \
    b4fdc30a7242e03f7166491bf6b415aa3b5dce8ff0e16444f6ccb24c5bcb03a0 \
    'current Linux native build driver' || exit 1

project_input=$1
project_directory=$(CDPATH= cd -- "$(dirname -- "$project_input")" && pwd -P) || exit 1
project_path="$project_directory/$(basename -- "$project_input")"
if [[ $project_path != *.wvproj ]]; then
    echo 'The current native build input must use the .wvproj extension.' >&2
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
    echo 'The current native build output must use the .wvb extension.' >&2
    exit 64
fi

"$build_driver" \
    --workspace "$workspace_path" --project "$project_path" "$output_path"
