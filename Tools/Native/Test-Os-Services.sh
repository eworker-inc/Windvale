#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Os-Services.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-os-services.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-os-services.*)
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

service_projects=(
    Windvale-Os-Resource-Service-Core
    Windvale-Os-Resource-Service-Bridge
    Windvale-Os-Resource-Store-Service
    Windvale-Os-Directory-Service-Core
    Windvale-Os-Directory-Service-Bridge
    Windvale-Os-Directory-Snapshot
    Windvale-Os-Directory-Snapshot-Service
    Windvale-Os-Directory-Snapshot-Bridge
)
for project in "${service_projects[@]}"; do
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Operating-System/$project.wvproj" \
        "$temporary_directory/$project.wvb" >/dev/null || {
            status=$?
            echo "Native OS service project failed: $project" >&2
            exit "$status"
        }
done

behavior_projects=(
    Windvale-Native-Test-Os-Resource-Service
    Windvale-Native-Test-Os-Directory-Service
)
for project in "${behavior_projects[@]}"; do
    output="$temporary_directory/$project.wvb"
    run_output="$temporary_directory/$project.out"
    run_error="$temporary_directory/$project.err"
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Tests/$project.wvproj" \
        "$output" >/dev/null || exit $?
    "$script_directory/Run-Wvb.sh" "$output" >"$run_output" 2>"$run_error" || exit $?
    [[ $(<"$run_output") == 'Result: 42' ]] || exit 1
    [[ ! -s $run_error ]] || exit 1
done

echo 'native os services status=Passed projects=8 behavior=2 cases=10'
