#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Language-1.0-Front-Door.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
profile_root="$repository_root/Documents/Project/Language-1.0-Localization-Workloads/01-Source-Profile-Admission/Reference-Artifacts"
source_lock="$profile_root/Source-Inputs.wvlock"
source_profile="$profile_root/En-Source-Profile.wvsp"
source_lock_hash=4c5840af896924292a2ad3f3d5d986956211745a8e4a9bb60f0b45f10cecf9c3
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-language-1-front-door.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-language-1-front-door.*)
            rm -f -- "$work"/*
            rmdir -- "$work"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $work" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

echo 'START language 1 front door phase=frozen-fixtures item=1/4'
node "$script_directory/Verify-Language-1.0-Migration-Fixtures.mjs" || exit $?
echo 'PASS  language 1 front door phase=frozen-fixtures item=1/4'

echo 'START language 1 front door phase=descriptor item=2/4'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Source-Descriptor.wvproj" \
    "$work/Descriptor-A.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Source-Descriptor.wvproj" \
    "$work/Descriptor-B.wvb" >/dev/null || exit $?
cmp -s -- "$work/Descriptor-A.wvb" "$work/Descriptor-B.wvb" || exit 1
"$script_directory/Run-Wvb.sh" "$work/Descriptor-A.wvb" \
    >"$work/Run.out" 2>"$work/Run.err" || exit $?
[[ ! -s $work/Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected.out"
cmp -s -- "$work/Expected.out" "$work/Run.out" || exit 1
echo 'PASS  language 1 front door phase=descriptor item=2/4'

echo 'START language 1 front door phase=value-front-end item=3/4'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Language-1-Value-Front-End.wvproj" \
    "$work/Value-Front-End.wvb" >/dev/null || exit $?
"$script_directory/Run-Wvb.sh" "$work/Value-Front-End.wvb" \
    >"$work/Value-Front-End.out" 2>"$work/Value-Front-End.err" || exit $?
[[ ! -s $work/Value-Front-End.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Value-Front-End.out"
cmp -s -- "$work/Expected-Value-Front-End.out" "$work/Value-Front-End.out" || exit 1
echo 'PASS  language 1 front door phase=value-front-end item=3/4'

echo 'START language 1 front door phase=compiler-slice item=4/4'
segmented_report=$("$script_directory/Build-Cached-Segmented-Project.sh" \
    "$repository_root/Projects/Examples/Windvale-Compiler.wvproj" \
    "$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate/linux-x64/wvbuild.elf" \
    "$work/Compiler.wvb" "$work/Compiler-Image" "$work/Compiler.wvli") || exit $?
compiler_entry=$(printf '%s\n' "$segmented_report" | sed -n \
    's/^native segmented project cache status=[A-Za-z]* key=[0-9a-f]* entry-offset=\([0-9][0-9]*\) fragments=[1-8]$/\1/p')
compiler_fragments=$(printf '%s\n' "$segmented_report" | sed -n \
    's/^native segmented project cache status=[A-Za-z]* key=[0-9a-f]* entry-offset=[0-9][0-9]* fragments=\([1-8]\)$/\1/p')
[[ $compiler_entry =~ ^(0|[1-9][0-9]*)$ && $compiler_fragments =~ ^[1-8]$ ]] || exit 1
"$script_directory/Build-Cached-Hosted-Application.sh" 1 \
    "$work/Compiler.wvb" "$work/Compiler-Image" "$compiler_fragments" \
    "$compiler_entry" "$work/Compiler.elf" linux >/dev/null || exit $?
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Minimum-Program.wv" \
    "$work/Minimum-A.wvb" >"$work/Compile-A.out" 2>"$work/Compile-A.err" || exit $?
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Minimum-Program.wv" \
    "$work/Minimum-B.wvb" >"$work/Compile-B.out" 2>"$work/Compile-B.err" || exit $?
[[ ! -s $work/Compile-A.err && ! -s $work/Compile-B.err ]] || exit 1
cmp -s -- "$work/Compile-A.out" "$work/Compile-B.out" || exit 1
cmp -s -- "$work/Minimum-A.wvb" "$work/Minimum-B.wvb" || exit 1
"$script_directory/Run-Wvb.sh" "$work/Minimum-A.wvb" \
    >"$work/Minimum.out" 2>"$work/Minimum.err" || exit $?
[[ ! -s $work/Minimum.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Minimum.out"
cmp -s -- "$work/Expected-Minimum.out" "$work/Minimum.out" || exit 1
expect_rejection() {
    local source=$1 output=$2
    expect_rejection_with_digest "$source" "$output" "$source_lock_hash" "$source_profile"
}
expect_rejection_with_digest() {
    local source=$1 output=$2 digest=$3 profile=$4
    [[ ! -e $output ]] || return 1
    if "$work/Compiler.elf" \
        --source-input-lock "$source_lock" "$digest" \
        --source-profile "$profile" "$source" "$output" \
        >"$output.out" 2>"$output.err"; then
        return 1
    fi
    [[ ! -e $output ]]
}
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Unsupported-Source-Profile.wv" \
    "$work/Unsupported.wvb" || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Missing-Edition-Profile.wv" \
    "$work/Missing-Profile.wvb" || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Descriptorless-Edition-Header.wv" \
    "$work/Descriptorless.wvb" || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Seed-Only-Void.wv" \
    "$work/Seed-Only-Void.wvb" || exit 1
if "$work/Compiler.elf" \
    "$repository_root/Tests/Fixtures/Language-1.0/Minimum-Program.wv" \
    "$work/Ambient.wvb" >"$work/Ambient.out" 2>"$work/Ambient.err"; then
    exit 1
fi
[[ ! -e $work/Ambient.wvb ]] || exit 1
expect_rejection_with_digest \
    "$repository_root/Tests/Fixtures/Language-1.0/Minimum-Program.wv" \
    "$work/Wrong-Digest.wvb" \
    4c5840af896924292a2ad3f3d5d986956211745a8e4a9bb60f0b45f10cecf9c0 \
    "$source_profile" || exit 1
cp -- "$source_profile" "$work/Corrupt.wvsp" || exit 1
printf x >>"$work/Corrupt.wvsp" || exit 1
expect_rejection_with_digest \
    "$repository_root/Tests/Fixtures/Language-1.0/Minimum-Program.wv" \
    "$work/Corrupt.wvb" "$source_lock_hash" "$work/Corrupt.wvsp" || exit 1
[[ $(wc -c < "$work/Minimum-A.wvb") -eq 221 ]] || exit 1
printf '%s  %s\n' \
    '25a18cf13d791db1e85fd6b237f89f21d4a0c7b9460b0a72db2da5e5deb205ae' \
    "$work/Minimum-A.wvb" | sha256sum --check --status || exit 1
echo 'PASS  language 1 front door phase=compiler-slice item=4/4'
echo 'native language 1 front door status=Passed cases=13 frozen-inputs=250 source-fixtures=72 descriptor-cases=33 profile-cases=4 value-front-end-cases=23 compiler-cases=8 compiler-result=42 compiler-wvb-bytes=221'
