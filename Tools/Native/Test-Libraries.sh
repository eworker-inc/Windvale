#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Libraries.sh' >&2
    exit 64
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
)
for project in "${library_projects[@]}"; do
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Libraries/$project.wvproj" \
        "$temporary_directory/$project.wvb" >/dev/null || exit $?
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

echo 'native libraries status=Passed projects=17 conformance-builds=7 negative=2 cases=26'
