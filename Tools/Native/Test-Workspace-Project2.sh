#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
build_driver="$repository_root/Artifacts/Native-Front-Door/linux-x64/wvbuild.elf"
workspace="$repository_root/Windvale.wvws"
fixtures="$repository_root/Tests/Fixtures/Project"
temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-workspace-project2.XXXXXXXX") || exit 1
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-workspace-project2.*) rm -rf -- "$test_directory" ;;
        *) echo "Refusing to remove unexpected temporary path: $test_directory" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

candidate="$test_directory/Candidate.wvb"
"$script_directory/Build-Wvb.sh" \
    "$fixtures/Workspace-Project2-Build.wvproj" "$candidate" \
    >"$test_directory/Valid.out" 2>"$test_directory/Valid.err" || exit 1
"$script_directory/Run-Wvb.sh" "$candidate" --report-steps \
    >"$test_directory/Run.out" 2>"$test_directory/Run.err" || exit 1
grep -Fx 'Result: 42' "$test_directory/Run.out" >/dev/null || exit 1

reject() {
    local case_workspace=$1
    local case_project=$2
    local code=$3
    set +e
    "$build_driver" --workspace "$case_workspace" --project "$case_project" "$candidate" \
        >"$test_directory/Reject.out" 2>"$test_directory/Reject.err"
    local result=$?
    set -e
    [[ $result -eq 1 ]] || return 1
    grep -F "code=$code" "$test_directory/Reject.err" >/dev/null
}

set -e
reject "$workspace" "$fixtures/Legacy-Project1.wvproj" WVP1001
reject "$workspace" "$fixtures/Parent-Escape-Project2.wvproj" WVP1006
reject "$workspace" "$fixtures/Absolute-Path-Project2.wvproj" WVP1006
reject "$workspace" "$fixtures/Duplicate-Path-Project2.wvproj" WVP1007
reject "$fixtures/Invalid-Header.wvws" "$fixtures/Workspace-Project2-Build.wvproj" WVW1001
reject "$fixtures/Trailing-Data.wvws" "$fixtures/Workspace-Project2-Build.wvproj" WVW1002
reject "$fixtures/Nested/Windvale.wvws" "$fixtures/Workspace-Project2-Build.wvproj" WVW1003

echo 'native workspace/project test status=Complete cases=8'
