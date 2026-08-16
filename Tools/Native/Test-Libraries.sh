#!/usr/bin/env bash
set -uo pipefail

development_target=''
if [[ $# -ne 0 ]]; then
    if [[ $# -ne 2 || $1 != '--development-target' || -z $2 ]]; then
        echo 'Usage: ./Tools/Native/Test-Libraries.sh [--development-target <target>]' >&2
        exit 64
    fi
    development_target=$2
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-libraries.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-libraries.*)
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

if [[ -n $development_target ]]; then
    target_plan=$repository_root/Tests/Native/Library-Development-Targets.txt
    [[ -f $target_plan ]] || {
        echo 'Missing library development-target manifest.' >&2
        exit 1
    }
    IFS= read -r target_header <"$target_plan"
    [[ $target_header == 'windvale-library-development-targets 1' ]] || {
        echo 'Invalid library development-target manifest.' >&2
        exit 1
    }
    development_projects=0
    development_conformance=0
    development_negative=0
    development_cases=0
    while IFS='|' read -r target kind project; do
        [[ $target == "$development_target" ]] || continue
        development_cases=$((development_cases + 1))
        output=$temporary_directory/development-$development_cases.wvb
        case "$kind" in
            project)
                development_projects=$((development_projects + 1))
                "$script_directory/Build-Wvb.sh" \
                    "$repository_root/$project" "$output" >/dev/null || {
                        status=$?
                        echo "Native library development project failed: $project" >&2
                        exit "$status"
                    }
                ;;
            conformance)
                development_conformance=$((development_conformance + 1))
                "$script_directory/Build-Wvb.sh" \
                    "$repository_root/$project" "$output" >/dev/null || {
                        status=$?
                        echo "Native library development project failed: $project" >&2
                        exit "$status"
                    }
                ;;
            negative)
                development_negative=$((development_negative + 1))
                if "$script_directory/Build-Wvb.sh" \
                    "$repository_root/$project" "$output" \
                    >"$temporary_directory/development-$development_cases.out" \
                    2>"$temporary_directory/development-$development_cases.err"; then
                    exit 1
                fi
                [[ ! -e $output ]] || exit 1
                ;;
            *)
                echo "Invalid library development-target kind: $kind" >&2
                exit 1
                ;;
        esac
    done < <(tail -n +2 "$target_plan")
    if [[ $development_cases -eq 0 ]]; then
        echo "Unknown library development target: $development_target" >&2
        exit 64
    fi
    echo "native libraries development status=Passed target=$development_target projects=$development_projects conformance-builds=$development_conformance negative=$development_negative cases=$development_cases"
    exit 0
fi

library_projects=(
    Windvale-Library-Resource-Store
    Windvale-Library-Database-Storage-Geometry
    Windvale-Library-Database-Storage-Page
    Windvale-Library-Database-Durable-Superblock
    Windvale-Library-Database-Durable-Page
    Windvale-Library-Database-Durable-Commit-Record
    Windvale-Library-Database-Commit-Publication
    Windvale-Library-Wvdb-Reader
    Windvale-Library-Hosted-Resource-Store
    Windvale-Library-Read-Only-Directory
    Windvale-Library-Random-Access-Storage
    Windvale-Library-Random-Access-Database-Page
    Windvale-Library-Native-Hosted-Snapshot-Page
    Windvale-Library-Read-Only-Wvdb
    Windvale-Library-Model-Protocol
    Windvale-Library-Scripted-Model-Provider
)
for project in "${library_projects[@]}"; do
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Libraries/$project.wvproj" \
        "$temporary_directory/$project.wvb" >/dev/null || {
            status=$?
            echo "Native library project failed: $project" >&2
            exit "$status"
        }
done

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Tests/Fixtures/Libraries/Directory-Import-Smoke.wvproj" \
    "$temporary_directory/Directory-Import-Smoke.wvb" >/dev/null || exit $?

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Tests/Fixtures/Libraries/Random-Access-Page-Import-Smoke.wvproj" \
    "$temporary_directory/Random-Access-Page-Import-Smoke.wvb" >/dev/null || exit $?

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Tests/Fixtures/Libraries/Random-Access-Storage-Import-Smoke.wvproj" \
    "$temporary_directory/Random-Access-Storage-Import-Smoke.wvb" >/dev/null || exit $?

conformance_projects=(
    Windvale-Native-Test-Database-Geometry
    Windvale-Native-Test-Database-Storage-Page
    Windvale-Native-Test-Database-Storage-Page-Accept
    Windvale-Native-Test-Database-Durable-Superblock
    Windvale-Native-Test-Database-Durable-Commit
    Windvale-Native-Test-Native-Hosted-Snapshot-Page
    Windvale-Native-Test-Database-Reader
    Windvale-Native-Test-Model-Protocol
)
for project in "${conformance_projects[@]}"; do
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Tests/$project.wvproj" \
        "$temporary_directory/$project.wvb" >/dev/null || exit $?
done

negative_projects=(
    Capability-Import-No-Root-Declaration
    Capability-Profile-Rejection
)
for project in "${negative_projects[@]}"; do
    if "$script_directory/Build-Wvb.sh" \
        "$repository_root/Tests/Fixtures/Libraries/$project.wvproj" \
        "$temporary_directory/$project.wvb" \
        >"$temporary_directory/$project.out" \
        2>"$temporary_directory/$project.err"; then
        exit 1
    fi
    [[ ! -e "$temporary_directory/$project.wvb" ]] || exit 1
done

echo 'native libraries status=Passed projects=19 conformance-builds=8 negative=2 cases=29'
