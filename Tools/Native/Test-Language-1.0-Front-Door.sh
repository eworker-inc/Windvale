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
source_lock_hash=9e2ca572552ed52ed496142d18539f2f55fed2bbdfb1ec602f283b5d72386f3e
bootstrap_emitter_wvb="$repository_root/Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/Wvb/wvemit.wvb"
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-language-1-front-door.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-language-1-front-door.*)
            for malformed in \
                Fixed-Integer-Malformed Rune-Malformed Floating-Malformed \
                Unit-Never-Malformed Multi-Field-Variant-Malformed; do
                if [[ -d $work/$malformed ]]; then
                    rm -f -- "$work/$malformed"/*
                    rmdir -- "$work/$malformed"
                fi
            done
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

echo 'START language 1 front door phase=frozen-fixtures item=1/11'
node "$script_directory/Verify-Language-1.0-Migration-Fixtures.mjs" || exit $?
echo 'PASS  language 1 front door phase=frozen-fixtures item=1/11'

echo 'START language 1 front door phase=descriptor item=2/11'
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
echo 'PASS  language 1 front door phase=descriptor item=2/11'

echo 'START language 1 front door phase=value-front-end item=3/11'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Language-1-Value-Front-End.wvproj" \
    "$work/Value-Front-End.wvb" >/dev/null || exit $?
"$script_directory/Run-Wvb.sh" "$work/Value-Front-End.wvb" \
    >"$work/Value-Front-End.out" 2>"$work/Value-Front-End.err" || exit $?
[[ ! -s $work/Value-Front-End.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Value-Front-End.out"
cmp -s -- "$work/Expected-Value-Front-End.out" "$work/Value-Front-End.out" || exit 1
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Language-1-Generic-Declarations.wvproj" \
    "$work/Generic-Declarations.wvb" >/dev/null || exit $?
"$script_directory/Run-Wvb.sh" "$work/Generic-Declarations.wvb" \
    >"$work/Generic-Declarations.out" 2>"$work/Generic-Declarations.err" || exit $?
[[ ! -s $work/Generic-Declarations.err ]] || exit 1
cmp -s -- "$work/Expected-Value-Front-End.out" \
    "$work/Generic-Declarations.out" || exit 1
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Language-1-Generic-Calls.wvproj" \
    "$work/Generic-Calls.wvb" >/dev/null || exit $?
"$script_directory/Run-Wvb.sh" "$work/Generic-Calls.wvb" \
    >"$work/Generic-Calls.out" 2>"$work/Generic-Calls.err" || exit $?
[[ ! -s $work/Generic-Calls.err ]] || exit 1
cmp -s -- "$work/Expected-Value-Front-End.out" \
    "$work/Generic-Calls.out" || exit 1
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Language-1-Generic-Resolution.wvproj" \
    "$work/Generic-Resolution.wvb" >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 1 \
    "$work/Generic-Resolution.wvb" "$work/Generic-Resolution.elf" \
    >"$work/Generic-Resolution-Package.out" \
    2>"$work/Generic-Resolution-Package.err" || exit $?
[[ ! -s $work/Generic-Resolution-Package.err ]] || exit 1
"$work/Generic-Resolution.elf" \
    >"$work/Generic-Resolution.out" 2>"$work/Generic-Resolution.err"
generic_resolution_result=$?
[[ $generic_resolution_result -eq 42 ]] || exit 1
[[ ! -s $work/Generic-Resolution.out && \
    ! -s $work/Generic-Resolution.err ]] || exit 1
echo 'START language 1 front door step=generic-type-catalog'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Language-1-Generic-Type-Catalog.wvproj" \
    "$work/Generic-Type-Catalog.wvb" >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 1 \
    "$work/Generic-Type-Catalog.wvb" "$work/Generic-Type-Catalog.elf" \
    >"$work/Generic-Type-Catalog-Package.out" \
    2>"$work/Generic-Type-Catalog-Package.err" || exit $?
[[ ! -s $work/Generic-Type-Catalog-Package.err ]] || exit 1
"$work/Generic-Type-Catalog.elf" \
    >"$work/Generic-Type-Catalog.out" \
    2>"$work/Generic-Type-Catalog.err"
generic_type_catalog_result=$?
[[ $generic_type_catalog_result -eq 42 ]] || exit 1
[[ ! -s $work/Generic-Type-Catalog.out && \
    ! -s $work/Generic-Type-Catalog.err ]] || exit 1
generic_type_catalog_wvb_bytes=$(wc -c < "$work/Generic-Type-Catalog.wvb")
printf 'PASS  language 1 front door step=generic-type-catalog wvb-bytes=%s\n' \
    "$generic_type_catalog_wvb_bytes"
echo 'PASS  language 1 front door phase=value-front-end item=3/11'

echo 'START language 1 front door phase=compiler-slice item=4/11'
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 \
    "$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate/Wvb/Windvale-Compiler.wvb" \
    "$work/Bootstrap-Compiler.elf" --development-cache || exit $?
"$script_directory/Compile-Compiler-Source-Set.sh" \
    "$work/Bootstrap-Compiler.elf" \
    "$repository_root" "$work/Compiler.wvb" || exit $?
node "$script_directory/Compile-Project-2-With-Compiler.mjs" \
    "$work/Bootstrap-Compiler.elf" \
    "$repository_root/Projects/Tools/Windvale-Compiler-Admission-Driver.wvproj" \
    "$work/Admitter.wvb" || exit $?
node "$script_directory/Compile-Project-2-With-Compiler.mjs" \
    "$work/Bootstrap-Compiler.elf" \
    "$repository_root/Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj" \
    "$work/Analyzer.wvb" || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 \
    "$work/Admitter.wvb" "$work/Admitter.elf" --development-cache || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 \
    "$work/Analyzer.wvb" "$work/Analyzer.elf" --development-cache || exit $?
node "$script_directory/Write-Split-Compiler-Producer-Identity.mjs" \
    analyzer "$work/Analyzer.elf" "$work/Analyzer.identity" || exit $?
[[ $(wc -c < "$bootstrap_emitter_wvb") -eq 746557 ]] || exit 1
printf '%s  %s\n' \
    a0fe54283ed51e1940bae837eb11bfb2d72f16dd91d7eb7022e51730eb0c5805 \
    "$bootstrap_emitter_wvb" | sha256sum --check --strict --quiet || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 \
    "$bootstrap_emitter_wvb" "$work/Bootstrap-Emitter.elf" \
    --development-cache || exit $?
node "$script_directory/Write-Split-Compiler-Producer-Identity.mjs" \
    emitter "$work/Bootstrap-Emitter.elf" \
    "$work/Bootstrap-Emitter.identity" || exit $?
node "$script_directory/Build-Cached-Split-Project-Wvb.mjs" \
    "$repository_root/Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj" \
    "$work/Emitter.wvb" \
    "$work/Analyzer.elf" "$work/Analyzer.identity" \
    "$work/Bootstrap-Emitter.elf" "$work/Bootstrap-Emitter.identity" || exit $?
[[ $(wc -c < "$work/Emitter.wvb") -eq 841145 ]] || exit 1
printf '%s  %s\n' \
    b925e215796f82d67191833be60c6d6421427989e2dcd8e5cdcb3562142f36a0 \
    "$work/Emitter.wvb" | sha256sum --check --strict --quiet || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 \
    "$work/Emitter.wvb" "$work/Emitter.elf" --development-cache || exit $?
node "$script_directory/Write-Split-Compiler-Producer-Identity.mjs" \
    emitter "$work/Emitter.elf" "$work/Emitter.identity" || exit $?
echo 'START language 1 front door step=generic-wir-split'
node "$script_directory/Build-Cached-Split-Project-Wvb.mjs" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Language-1-Generic-Wir.wvproj" \
    "$work/Generic-Wir-A.wvb" \
    "$work/Analyzer.elf" "$work/Analyzer.identity" \
    "$work/Emitter.elf" "$work/Emitter.identity" || exit $?
node "$script_directory/Build-Cached-Split-Project-Wvb.mjs" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Language-1-Generic-Wir.wvproj" \
    "$work/Generic-Wir-B.wvb" \
    "$work/Analyzer.elf" "$work/Analyzer.identity" \
    "$work/Emitter.elf" "$work/Emitter.identity" || exit $?
cmp -s -- "$work/Generic-Wir-A.wvb" "$work/Generic-Wir-B.wvb" || exit 1
[[ $(wc -c < "$work/Generic-Wir-A.wvb") -eq 1065737 ]] || exit 1
printf '%s  %s\n' \
    c8aa63e688ee53ed5ee72cc75db4b3852f0b6431a501a4f6230d680b6a4dcefc \
    "$work/Generic-Wir-A.wvb" | sha256sum --check --strict --quiet || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 1 \
    "$work/Generic-Wir-A.wvb" "$work/Generic-Wir.elf" \
    --development-cache >"$work/Generic-Wir-Package.out" \
    2>"$work/Generic-Wir-Package.err" || exit $?
[[ ! -s $work/Generic-Wir-Package.err ]] || exit 1
"$work/Generic-Wir.elf" >"$work/Generic-Wir.out" \
    2>"$work/Generic-Wir.err"
generic_wir_result=$?
[[ $generic_wir_result -eq 42 ]] || exit 1
[[ ! -s $work/Generic-Wir.out && ! -s $work/Generic-Wir.err ]] || exit 1
generic_wir_wvb_bytes=$(wc -c < "$work/Generic-Wir-A.wvb")
printf 'PASS  language 1 front door step=generic-wir-split wvb-bytes=%s result=42\n' \
    "$generic_wir_wvb_bytes"
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Minimum-Program.wv" \
    "$work/Minimum-A.wvb" >"$work/Compile-A.out" 2>"$work/Compile-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
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
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Unit-Control.wv" \
    "$work/Unit-A.wvb" >"$work/Unit-A.out" 2>"$work/Unit-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Unit-Control.wv" \
    "$work/Unit-B.wvb" >"$work/Unit-B.out" 2>"$work/Unit-B.err" || exit $?
[[ ! -s $work/Unit-A.err && ! -s $work/Unit-B.err ]] || exit 1
cmp -s -- "$work/Unit-A.out" "$work/Unit-B.out" || exit 1
cmp -s -- "$work/Unit-A.wvb" "$work/Unit-B.wvb" || exit 1
cat -- "$work/Unit-A.out"
printf 'INFO  language 1 unit wvb-bytes=%s\n' "$(wc -c < "$work/Unit-A.wvb")"
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Record-Update.wv" \
    "$work/Record-Update-A.wvb" \
    >"$work/Record-Update-A.out" 2>"$work/Record-Update-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Record-Update.wv" \
    "$work/Record-Update-B.wvb" \
    >"$work/Record-Update-B.out" 2>"$work/Record-Update-B.err" || exit $?
[[ ! -s $work/Record-Update-A.err && ! -s $work/Record-Update-B.err ]] || exit 1
cmp -s -- "$work/Record-Update-A.out" "$work/Record-Update-B.out" || exit 1
cmp -s -- "$work/Record-Update-A.wvb" "$work/Record-Update-B.wvb" || exit 1
cat -- "$work/Record-Update-A.out"
printf 'INFO  language 1 record-update wvb-bytes=%s\n' \
    "$(wc -c < "$work/Record-Update-A.wvb")"
"$script_directory/Run-Wvb.sh" "$work/Record-Update-A.wvb" \
    >"$work/Record-Update.out" 2>"$work/Record-Update.err" || exit $?
[[ ! -s $work/Record-Update.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Record-Update.out"
cmp -s -- "$work/Expected-Record-Update.out" \
    "$work/Record-Update.out" || exit 1
expect_rejection() {
    local source=$1 output=$2
    expect_rejection_with_digest "$source" "$output" "$source_lock_hash" "$source_profile"
}
expect_rejection_with_digest() {
    local source=$1 output=$2 digest=$3 profile=$4
    [[ ! -e $output ]] || return 1
    if node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
        --source-input-lock "$source_lock" "$digest" \
        --source-profile "$profile" "$source" "$output" \
        >"$output.out" 2>"$output.err"; then
        return 1
    fi
    [[ ! -e $output ]]
}
expect_foundation_generic_rejection() {
    local source=$1 output=$2
    [[ ! -e $output ]] || return 1
    if node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
        --source-input-lock "$source_lock" "$source_lock_hash" \
        --source-profile "$source_profile" \
        "$source" \
        "$repository_root/Libraries/Foundation/Values/Result.wv" \
        "$output" >"$output.out" 2>"$output.err"; then
        return 1
    fi
    [[ ! -e $output ]]
}
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Value-Control.wv" \
    "$work/Value-If-A.wvb" >"$work/Value-If-A.out" 2>"$work/Value-If-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Value-Control.wv" \
    "$work/Value-If-B.wvb" >"$work/Value-If-B.out" 2>"$work/Value-If-B.err" || exit $?
[[ ! -s $work/Value-If-A.err && ! -s $work/Value-If-B.err ]] || exit 1
cmp -s -- "$work/Value-If-A.out" "$work/Value-If-B.out" || exit 1
cmp -s -- "$work/Value-If-A.wvb" "$work/Value-If-B.wvb" || exit 1
"$script_directory/Run-Wvb.sh" "$work/Value-If-A.wvb" \
    >"$work/Value-If.out" 2>"$work/Value-If.err" || exit $?
[[ ! -s $work/Value-If.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Value-If.out"
cmp -s -- "$work/Expected-Value-If.out" "$work/Value-If.out" || exit 1
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Value-If-Lazy.wv" \
    "$work/Value-If-Lazy.wvb" >"$work/Value-If-Lazy.out" \
    2>"$work/Value-If-Lazy.err" || exit $?
[[ ! -s $work/Value-If-Lazy.err ]] || exit 1
"$script_directory/Run-Wvb.sh" "$work/Value-If-Lazy.wvb" \
    >"$work/Value-If-Lazy-Run.out" 2>"$work/Value-If-Lazy-Run.err" || exit $?
[[ ! -s $work/Value-If-Lazy-Run.err ]] || exit 1
cmp -s -- "$work/Expected-Value-If.out" "$work/Value-If-Lazy-Run.out" || exit 1
for name in Missing-Else Trailing-Semicolon Type-Mismatch Invalid-Condition; do
    expect_rejection \
        "$repository_root/Tests/Fixtures/Language-1.0/Value-If-$name.wv" \
        "$work/Value-If-$name.wvb" || exit 1
done
if node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    "$repository_root/Tests/Fixtures/Source-Wvb/Invalid-Value-If.wv" \
    "$work/Seed-Value-If.wvb" \
    >"$work/Seed-Value-If.out" 2>"$work/Seed-Value-If.err"; then
    exit 1
fi
[[ ! -e $work/Seed-Value-If.wvb ]] || exit 1
value_if_wvb_bytes=$(wc -c < "$work/Value-If-A.wvb")
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Value-Match.wv" \
    "$work/Value-Match-A.wvb" >"$work/Value-Match-A.out" \
    2>"$work/Value-Match-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Value-Match.wv" \
    "$work/Value-Match-B.wvb" >"$work/Value-Match-B.out" \
    2>"$work/Value-Match-B.err" || exit $?
[[ ! -s $work/Value-Match-A.err && ! -s $work/Value-Match-B.err ]] || exit 1
cmp -s -- "$work/Value-Match-A.out" "$work/Value-Match-B.out" || exit 1
cmp -s -- "$work/Value-Match-A.wvb" "$work/Value-Match-B.wvb" || exit 1
"$script_directory/Run-Wvb.sh" "$work/Value-Match-A.wvb" \
    >"$work/Value-Match.out" 2>"$work/Value-Match.err" || exit $?
[[ ! -s $work/Value-Match.err ]] || exit 1
cmp -s -- "$work/Expected-Value-If.out" "$work/Value-Match.out" || exit 1
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Value-Match-Lazy.wv" \
    "$work/Value-Match-Lazy.wvb" >"$work/Value-Match-Lazy.out" \
    2>"$work/Value-Match-Lazy.err" || exit $?
[[ ! -s $work/Value-Match-Lazy.err ]] || exit 1
"$script_directory/Run-Wvb.sh" "$work/Value-Match-Lazy.wvb" \
    >"$work/Value-Match-Lazy-Run.out" \
    2>"$work/Value-Match-Lazy-Run.err" || exit $?
[[ ! -s $work/Value-Match-Lazy-Run.err ]] || exit 1
cmp -s -- "$work/Expected-Value-If.out" \
    "$work/Value-Match-Lazy-Run.out" || exit 1
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Value-Match-Never.wv" \
    "$work/Value-Match-Never-A.wvb" \
    >"$work/Value-Match-Never-A.out" \
    2>"$work/Value-Match-Never-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Value-Match-Never.wv" \
    "$work/Value-Match-Never-B.wvb" \
    >"$work/Value-Match-Never-B.out" \
    2>"$work/Value-Match-Never-B.err" || exit $?
[[ ! -s $work/Value-Match-Never-A.err && \
    ! -s $work/Value-Match-Never-B.err ]] || exit 1
cmp -s -- "$work/Value-Match-Never-A.out" \
    "$work/Value-Match-Never-B.out" || exit 1
cmp -s -- "$work/Value-Match-Never-A.wvb" \
    "$work/Value-Match-Never-B.wvb" || exit 1
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Value-Match-Variant.wv" \
    "$work/Value-Match-Variant-A.wvb" \
    >"$work/Value-Match-Variant-A.out" \
    2>"$work/Value-Match-Variant-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Value-Match-Variant.wv" \
    "$work/Value-Match-Variant-B.wvb" \
    >"$work/Value-Match-Variant-B.out" \
    2>"$work/Value-Match-Variant-B.err" || exit $?
[[ ! -s $work/Value-Match-Variant-A.err && \
    ! -s $work/Value-Match-Variant-B.err ]] || exit 1
cmp -s -- "$work/Value-Match-Variant-A.out" \
    "$work/Value-Match-Variant-B.out" || exit 1
cmp -s -- "$work/Value-Match-Variant-A.wvb" \
    "$work/Value-Match-Variant-B.wvb" || exit 1
for name in Missing-Case Trailing-Semicolon Type-Mismatch; do
    expect_rejection \
        "$repository_root/Tests/Fixtures/Language-1.0/Value-Match-$name.wv" \
        "$work/Value-Match-$name.wvb" || exit 1
done
if node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    "$repository_root/Tests/Fixtures/Source-Wvb/Invalid-Value-Match.wv" \
    "$work/Seed-Value-Match.wvb" \
    >"$work/Seed-Value-Match.out" 2>"$work/Seed-Value-Match.err"; then
    exit 1
fi
[[ ! -e $work/Seed-Value-Match.wvb ]] || exit 1
value_match_wvb_bytes=$(wc -c < "$work/Value-Match-A.wvb")
value_match_never_wvb_bytes=$(wc -c < "$work/Value-Match-Never-A.wvb")
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Unit-Return-Value.wv" \
    "$work/Unit-Return-Value.wvb" || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Unit-Return-From-I32.wv" \
    "$work/Unit-Return-From-I32.wvb" || exit 1
if node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    "$repository_root/Tests/Fixtures/Source-Wvb/Invalid-Unit-Literal.wv" \
    "$work/Seed-Unit.wvb" >"$work/Seed-Unit.out" 2>"$work/Seed-Unit.err"; then
    exit 1
fi
[[ ! -e $work/Seed-Unit.wvb ]] || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Record-Update-Wrong-Base.wv" \
    "$work/Record-Update-Wrong-Base.wvb" || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Record-Update-Duplicate-Field.wv" \
    "$work/Record-Update-Duplicate-Field.wvb" || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Record-Update-Unknown-Field.wv" \
    "$work/Record-Update-Unknown-Field.wvb" || exit 1
if node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    "$repository_root/Tests/Fixtures/Source-Wvb/Invalid-Record-Update.wv" \
    "$work/Seed-Record-Update.wvb" \
    >"$work/Seed-Record-Update.out" 2>"$work/Seed-Record-Update.err"; then
    exit 1
fi
[[ ! -e $work/Seed-Record-Update.wvb ]] || exit 1
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
if node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
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
echo 'PASS  language 1 front door phase=compiler-slice item=4/11'

echo 'START language 1 front door phase=fixed-integers item=5/11'
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Integer-Program.wv" \
    "$work/Fixed-Integer-A.wvb" \
    >"$work/Fixed-Integer-A.out" 2>"$work/Fixed-Integer-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Integer-Program.wv" \
    "$work/Fixed-Integer-B.wvb" \
    >"$work/Fixed-Integer-B.out" 2>"$work/Fixed-Integer-B.err" || exit $?
[[ ! -s $work/Fixed-Integer-A.err && ! -s $work/Fixed-Integer-B.err ]] || exit 1
cmp -s -- "$work/Fixed-Integer-A.out" "$work/Fixed-Integer-B.out" || exit 1
cmp -s -- "$work/Fixed-Integer-A.wvb" "$work/Fixed-Integer-B.wvb" || exit 1

for name in Overflow Divide-By-Zero Invalid-Shift; do
    node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
        --source-input-lock "$source_lock" "$source_lock_hash" \
        --source-profile "$source_profile" \
        "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Integer-$name.wv" \
        "$work/Fixed-Integer-$name.wvb" \
        >"$work/Fixed-Integer-$name.out" 2>"$work/Fixed-Integer-$name.err" || exit $?
done
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Integer-Literal-Out-Of-Range.wv" \
    "$work/Fixed-Integer-Literal-Out-Of-Range.wvb" || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Integer-Type-Mismatch.wv" \
    "$work/Fixed-Integer-Type-Mismatch.wvb" || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Integer-Signed-Bitwise.wv" \
    "$work/Fixed-Integer-Signed-Bitwise.wvb" || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Integer-Constant-Overflow.wv" \
    "$work/Fixed-Integer-Constant-Overflow.wvb" || exit 1

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj" \
    "$work/Verifier.wvb" >"$work/Verifier-Build.out" \
    2>"$work/Verifier-Build.err" || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 2 \
    "$work/Verifier.wvb" "$work/Verifier.elf" linux \
    >"$work/Verifier-Package.out" 2>"$work/Verifier-Package.err" || exit $?
for name in A Overflow Divide-By-Zero Invalid-Shift; do
    "$work/Verifier.elf" "$work/Fixed-Integer-$name.wvb" \
        >"$work/Verify-$name.out" 2>"$work/Verify-$name.err" || exit $?
    grep -Fq 'wvb status=Valid profile=compiler-aligned' \
        "$work/Verify-$name.out" || exit 1
done
node "$script_directory/Verify-Language-1.0-Fixed-Integers.mjs" \
    "$work/Verifier.elf" "$work/Fixed-Integer-A.wvb" \
    "$work/Fixed-Integer-Malformed" || exit $?

"$script_directory/Run-Wvb.sh" "$work/Fixed-Integer-A.wvb" \
    >"$work/Fixed-Integer-Run.out" 2>"$work/Fixed-Integer-Run.err" || exit $?
[[ ! -s $work/Fixed-Integer-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Fixed-Integer.out"
cmp -s -- "$work/Expected-Fixed-Integer.out" \
    "$work/Fixed-Integer-Run.out" || exit 1
expect_runtime_failure() {
    local input=$1 status=$2
    if "$script_directory/Run-Wvb.sh" "$input" \
        >"$work/Runtime-$status.out" 2>"$work/Runtime-$status.err"; then
        return 1
    fi
    grep -Fq "wvb run status=Failed code=$status " \
        "$work/Runtime-$status.err"
}
expect_runtime_failure "$work/Fixed-Integer-Overflow.wvb" 3007 || exit 1
expect_runtime_failure "$work/Fixed-Integer-Divide-By-Zero.wvb" 3032 || exit 1
expect_runtime_failure "$work/Fixed-Integer-Invalid-Shift.wvb" 3033 || exit 1

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Wvb-Fixed-Integer-Runtime.wvproj" \
    "$work/Fixed-Integer-Runtime.wvb" >/dev/null || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" \
    "$work/Fixed-Integer-Runtime.wvb" "$work/Fixed-Integer-Runtime.wvo" \
    >/dev/null || exit $?
"$script_directory/Check-Wvo.sh" "$work/Fixed-Integer-Runtime.wvo" \
    >/dev/null || exit $?
"$script_directory/Link-Wvo.sh" 1048576 Main \
    "$work/Fixed-Integer-Runtime.bin" "$work/Fixed-Integer-Runtime.wvo" \
    >"$work/Fixed-Integer-Runtime.wvmap" || exit $?
runtime_address=$(sed -n \
    's/^entry name=Main address=\([0-9][0-9]*\)$/\1/p' \
    "$work/Fixed-Integer-Runtime.wvmap")
[[ $runtime_address =~ ^[0-9]+$ && $runtime_address -ge 1048576 ]] || exit 1
runtime_entry=$((runtime_address - 1048576))
"$script_directory/Package-Console.sh" linux-x64-console-v1 \
    "$work/Fixed-Integer-Runtime.bin" "$runtime_entry" \
    "$work/Fixed-Integer-Runtime.elf" >/dev/null || exit $?
"$work/Fixed-Integer-Runtime.elf"
runtime_result=$?
[[ $runtime_result -eq 42 ]] || exit 1
printf 'INFO  language 1 fixed-integer wvb-bytes=%s\n' \
    "$(wc -c < "$work/Fixed-Integer-A.wvb")"
echo 'PASS  language 1 front door phase=fixed-integers item=5/11'

echo 'START language 1 front door phase=runes item=6/11'
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Rune-Program.wv" \
    "$work/Rune-A.wvb" \
    >"$work/Rune-A.out" 2>"$work/Rune-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Rune-Program.wv" \
    "$work/Rune-B.wvb" \
    >"$work/Rune-B.out" 2>"$work/Rune-B.err" || exit $?
[[ ! -s $work/Rune-A.err && ! -s $work/Rune-B.err ]] || exit 1
cmp -s -- "$work/Rune-A.out" "$work/Rune-B.out" || exit 1
cmp -s -- "$work/Rune-A.wvb" "$work/Rune-B.wvb" || exit 1

for name in Empty Multiple Surrogate Out-Of-Range Invalid-Escape Unterminated Type-Mismatch Invalid-Operator; do
    expect_rejection \
        "$repository_root/Tests/Fixtures/Language-1.0/Rune-$name.wv" \
        "$work/Rune-$name.wvb" || exit 1
done

"$work/Verifier.elf" "$work/Rune-A.wvb" \
    >"$work/Verify-Rune.out" 2>"$work/Verify-Rune.err" || exit $?
grep -Fq 'wvb status=Valid profile=compiler-aligned' \
    "$work/Verify-Rune.out" || exit 1
node "$script_directory/Verify-Language-1.0-Runes.mjs" \
    "$work/Verifier.elf" "$work/Rune-A.wvb" \
    "$work/Rune-Malformed" || exit $?

"$script_directory/Run-Wvb.sh" "$work/Rune-A.wvb" \
    >"$work/Rune-Run.out" 2>"$work/Rune-Run.err" || exit $?
[[ ! -s $work/Rune-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Rune.out"
cmp -s -- "$work/Expected-Rune.out" "$work/Rune-Run.out" || exit 1

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Wvb-Rune-Runtime.wvproj" \
    "$work/Rune-Runtime.wvb" >/dev/null || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" \
    "$work/Rune-Runtime.wvb" "$work/Rune-Runtime.wvo" \
    >/dev/null || exit $?
"$script_directory/Check-Wvo.sh" "$work/Rune-Runtime.wvo" \
    >/dev/null || exit $?
"$script_directory/Link-Wvo.sh" 1048576 Main \
    "$work/Rune-Runtime.bin" "$work/Rune-Runtime.wvo" \
    >"$work/Rune-Runtime.wvmap" || exit $?
runtime_address=$(sed -n \
    's/^entry name=Main address=\([0-9][0-9]*\)$/\1/p' \
    "$work/Rune-Runtime.wvmap")
[[ $runtime_address =~ ^[0-9]+$ && $runtime_address -ge 1048576 ]] || exit 1
runtime_entry=$((runtime_address - 1048576))
"$script_directory/Package-Console.sh" linux-x64-console-v1 \
    "$work/Rune-Runtime.bin" "$runtime_entry" \
    "$work/Rune-Runtime.elf" >/dev/null || exit $?
"$work/Rune-Runtime.elf"
runtime_result=$?
[[ $runtime_result -eq 42 ]] || exit 1
rune_wvb_bytes=$(wc -c < "$work/Rune-A.wvb")
printf 'INFO  language 1 rune wvb-bytes=%s\n' "$rune_wvb_bytes"
echo 'PASS  language 1 front door phase=runes item=6/11'

echo 'START language 1 front door phase=floating item=7/11'
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Floating-Program.wv" \
    "$work/Floating-A.wvb" \
    >"$work/Floating-A.out" 2>"$work/Floating-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Floating-Program.wv" \
    "$work/Floating-B.wvb" \
    >"$work/Floating-B.out" 2>"$work/Floating-B.err" || exit $?
[[ ! -s $work/Floating-A.err && ! -s $work/Floating-B.err ]] || exit 1
cmp -s -- "$work/Floating-A.out" "$work/Floating-B.out" || exit 1
cmp -s -- "$work/Floating-A.wvb" "$work/Floating-B.wvb" || exit 1

for name in Decimal-Literal Missing-Suffix Type-Mismatch Invalid-Operator; do
    expect_rejection \
        "$repository_root/Tests/Fixtures/Language-1.0/Floating-$name.wv" \
        "$work/Floating-$name.wvb" || exit 1
done

"$work/Verifier.elf" "$work/Floating-A.wvb" \
    >"$work/Verify-Floating.out" 2>"$work/Verify-Floating.err" || exit $?
grep -Fq 'wvb status=Valid profile=compiler-aligned' \
    "$work/Verify-Floating.out" || exit 1
node "$script_directory/Verify-Language-1.0-Floating.mjs" \
    "$work/Verifier.elf" "$work/Floating-A.wvb" \
    "$work/Floating-Malformed" || exit $?

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Wvb-Runner.wvproj" \
    "$work/Floating-Runner.wvb" >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 5 \
    "$work/Floating-Runner.wvb" "$work/Floating-Runner.elf" linux \
    >/dev/null || exit $?
"$work/Floating-Runner.elf" "$work/Floating-A.wvb" \
    >"$work/Floating-Run.out" 2>"$work/Floating-Run.err" || exit $?
[[ ! -s $work/Floating-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Floating.out"
cmp -s -- "$work/Expected-Floating.out" "$work/Floating-Run.out" || exit 1

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Wvb-Floating-Runtime.wvproj" \
    "$work/Floating-Runtime.wvb" >/dev/null || exit $?
"$script_directory/Lower-Wvb-To-Wvo.sh" \
    "$work/Floating-Runtime.wvb" "$work/Floating-Runtime.wvo" \
    >/dev/null || exit $?
"$script_directory/Check-Wvo.sh" "$work/Floating-Runtime.wvo" \
    >/dev/null || exit $?
"$script_directory/Link-Wvo.sh" 1048576 Main \
    "$work/Floating-Runtime.bin" "$work/Floating-Runtime.wvo" \
    >"$work/Floating-Runtime.wvmap" || exit $?
runtime_address=$(sed -n \
    's/^entry name=Main address=\([0-9][0-9]*\)$/\1/p' \
    "$work/Floating-Runtime.wvmap")
[[ $runtime_address =~ ^[0-9]+$ && $runtime_address -ge 1048576 ]] || exit 1
runtime_entry=$((runtime_address - 1048576))
"$script_directory/Package-Console.sh" linux-x64-console-v1 \
    "$work/Floating-Runtime.bin" "$runtime_entry" \
    "$work/Floating-Runtime.elf" >/dev/null || exit $?
"$work/Floating-Runtime.elf"
runtime_result=$?
[[ $runtime_result -eq 42 ]] || exit 1
floating_wvb_bytes=$(wc -c < "$work/Floating-A.wvb")
printf 'INFO  language 1 floating wvb-bytes=%s\n' "$floating_wvb_bytes"
echo 'PASS  language 1 front door phase=floating item=7/11'

echo 'START language 1 front door phase=unit-never item=8/11'
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Never-Control.wv" \
    "$work/Never-A.wvb" \
    >"$work/Never-A.out" 2>"$work/Never-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Never-Control.wv" \
    "$work/Never-B.wvb" \
    >"$work/Never-B.out" 2>"$work/Never-B.err" || exit $?
[[ ! -s $work/Never-A.err && ! -s $work/Never-B.err ]] || exit 1
cmp -s -- "$work/Never-A.out" "$work/Never-B.out" || exit 1
cmp -s -- "$work/Never-A.wvb" "$work/Never-B.wvb" || exit 1

for name in Fallthrough Return Parameter Unreachable; do
    expect_rejection \
        "$repository_root/Tests/Fixtures/Language-1.0/Never-$name.wv" \
        "$work/Never-$name.wvb" || exit 1
done

node "$script_directory/Verify-Language-1.0-Unit-Never.mjs" \
    "$work/Verifier.elf" "$work/Unit-A.wvb" "$work/Never-A.wvb" \
    "$work/Unit-Never-Malformed" || exit $?
"$work/Floating-Runner.elf" "$work/Unit-A.wvb" \
    >"$work/Unit-Run.out" 2>"$work/Unit-Run.err" || exit $?
"$work/Floating-Runner.elf" "$work/Never-A.wvb" \
    >"$work/Never-Run.out" 2>"$work/Never-Run.err" || exit $?
[[ ! -s $work/Unit-Run.err && ! -s $work/Never-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Unit-Never.out"
cmp -s -- "$work/Expected-Unit-Never.out" "$work/Unit-Run.out" || exit 1
cmp -s -- "$work/Expected-Unit-Never.out" "$work/Never-Run.out" || exit 1
unit_wvb_bytes=$(wc -c < "$work/Unit-A.wvb")
never_wvb_bytes=$(wc -c < "$work/Never-A.wvb")
printf 'INFO  language 1 unit-never unit-wvb-bytes=%s never-wvb-bytes=%s\n' \
    "$unit_wvb_bytes" "$never_wvb_bytes"
echo 'PASS  language 1 front door phase=unit-never item=8/11'

echo 'START language 1 front door phase=multi-field-variants item=9/11'
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Multi-Field-Variant.wv" \
    "$work/Multi-Field-Variant-A.wvb" \
    >"$work/Multi-Field-Variant-A.out" \
    2>"$work/Multi-Field-Variant-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Multi-Field-Variant.wv" \
    "$work/Multi-Field-Variant-B.wvb" \
    >"$work/Multi-Field-Variant-B.out" \
    2>"$work/Multi-Field-Variant-B.err" || exit $?
[[ ! -s $work/Multi-Field-Variant-A.err && \
    ! -s $work/Multi-Field-Variant-B.err ]] || exit 1
cmp -s -- "$work/Multi-Field-Variant-A.out" \
    "$work/Multi-Field-Variant-B.out" || exit 1
cmp -s -- "$work/Multi-Field-Variant-A.wvb" \
    "$work/Multi-Field-Variant-B.wvb" || exit 1

for name in \
    Duplicate-Declaration Empty-Payload Missing-Field Duplicate-Field \
    Unknown-Field Type-Mismatch Pattern-Missing-Field \
    Pattern-Duplicate-Field Pattern-Unknown-Field; do
    expect_rejection \
        "$repository_root/Tests/Fixtures/Language-1.0/Multi-Field-Variant-$name.wv" \
        "$work/Multi-Field-Variant-$name.wvb" || exit 1
done

node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Named-Variant-Field.wv" \
    "$work/Named-Variant-Field.wvb" \
    >"$work/Named-Variant-Field.out" \
    2>"$work/Named-Variant-Field.err" || exit $?
[[ ! -s $work/Named-Variant-Field.err ]] || exit 1

node "$script_directory/Verify-Language-1.0-Multi-Field-Variants.mjs" \
    "$work/Verifier.elf" "$work/Multi-Field-Variant-A.wvb" \
    "$work/Named-Variant-Field.wvb" \
    "$work/Multi-Field-Variant-Malformed" || exit $?
"$work/Floating-Runner.elf" "$work/Multi-Field-Variant-A.wvb" \
    >"$work/Multi-Field-Variant-Run.out" \
    2>"$work/Multi-Field-Variant-Run.err" || exit $?
"$work/Floating-Runner.elf" "$work/Named-Variant-Field.wvb" \
    >"$work/Named-Variant-Field-Run.out" \
    2>"$work/Named-Variant-Field-Run.err" || exit $?
[[ ! -s $work/Multi-Field-Variant-Run.err && \
    ! -s $work/Named-Variant-Field-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Variant.out"
cmp -s -- "$work/Expected-Variant.out" \
    "$work/Multi-Field-Variant-Run.out" || exit 1
"$work/Floating-Runner.elf" "$work/Value-Match-Variant-A.wvb" \
    >"$work/Value-Match-Variant-Run.out" \
    2>"$work/Value-Match-Variant-Run.err" || exit $?
[[ ! -s $work/Value-Match-Variant-Run.err ]] || exit 1
cmp -s -- "$work/Expected-Variant.out" \
    "$work/Value-Match-Variant-Run.out" || exit 1
"$work/Floating-Runner.elf" "$work/Value-Match-Never-A.wvb" \
    >"$work/Value-Match-Never-Run.out" \
    2>"$work/Value-Match-Never-Run.err" || exit $?
[[ ! -s $work/Value-Match-Never-Run.err ]] || exit 1
cmp -s -- "$work/Expected-Variant.out" \
    "$work/Value-Match-Never-Run.out" || exit 1
cmp -s -- "$work/Expected-Variant.out" \
    "$work/Named-Variant-Field-Run.out" || exit 1
if "$work/Floating-Runner.elf" \
    "$work/Multi-Field-Variant-Malformed/runtime-case-mismatch.wvb" \
    >"$work/Multi-Field-Variant-Mismatch.out" \
    2>"$work/Multi-Field-Variant-Mismatch.err"; then
    exit 1
fi
grep -F 'wvb run status=Failed code=3017 ' \
    "$work/Multi-Field-Variant-Mismatch.err" >/dev/null || exit 1
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Wvb-Variant-Runtime-Pressure.wvproj" \
    "$work/Variant-Runtime-Pressure.wvb" >/dev/null || exit $?
"$work/Floating-Runner.elf" "$work/Variant-Runtime-Pressure.wvb" \
    >"$work/Variant-Runtime-Pressure.out" \
    2>"$work/Variant-Runtime-Pressure.err" || exit $?
[[ ! -s $work/Variant-Runtime-Pressure.err ]] || exit 1
cmp -s -- "$work/Expected-Variant.out" \
    "$work/Variant-Runtime-Pressure.out" || exit 1
multi_field_variant_wvb_bytes=$(wc -c < "$work/Multi-Field-Variant-A.wvb")
printf 'INFO  language 1 multi-field-variants wvb-bytes=%s\n' \
    "$multi_field_variant_wvb_bytes"
echo 'PASS  language 1 front door phase=multi-field-variants item=9/11'

echo 'START language 1 front door phase=typed-failure item=10/11'
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Result-Try.wv" \
    "$work/Result-Try-A.wvb" \
    >"$work/Result-Try-A.out" 2>"$work/Result-Try-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Result-Try.wv" \
    "$work/Result-Try-B.wvb" \
    >"$work/Result-Try-B.out" 2>"$work/Result-Try-B.err" || exit $?
[[ ! -s $work/Result-Try-A.err && ! -s $work/Result-Try-B.err ]] || exit 1
cmp -s -- "$work/Result-Try-A.out" "$work/Result-Try-B.out" || exit 1
cmp -s -- "$work/Result-Try-A.wvb" "$work/Result-Try-B.wvb" || exit 1

for name in Lookalike Wrong-Value-Field Extra-Case Scalar; do
    expect_rejection \
        "$repository_root/Tests/Fixtures/Language-1.0/Result-Try-$name.wv" \
        "$work/Result-Try-$name.wvb" || exit 1
done

"$work/Verifier.elf" "$work/Result-Try-A.wvb" \
    >"$work/Verify-Result-Try.out" \
    2>"$work/Verify-Result-Try.err" || exit $?
grep -Fq 'wvb status=Valid profile=compiler-aligned' \
    "$work/Verify-Result-Try.out" || exit 1
"$work/Floating-Runner.elf" "$work/Result-Try-A.wvb" \
    >"$work/Result-Try-Run.out" \
    2>"$work/Result-Try-Run.err" || exit $?
[[ ! -s $work/Result-Try-Run.err ]] || exit 1
cmp -s -- "$work/Expected-Variant.out" "$work/Result-Try-Run.out" || exit 1
result_try_wvb_bytes=$(wc -c < "$work/Result-Try-A.wvb")
printf 'INFO  language 1 typed-failure wvb-bytes=%s\n' \
    "$result_try_wvb_bytes"
echo 'PASS  language 1 front door phase=typed-failure item=10/11'

echo 'START language 1 front door phase=foundation-generics item=11/11'
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Foundation-Generic-Result.wv" \
    "$repository_root/Libraries/Foundation/Values/Option.wv" \
    "$repository_root/Libraries/Foundation/Values/Result.wv" \
    "$work/Foundation-Generic-A.wvb" \
    >"$work/Foundation-Generic-A.out" \
    2>"$work/Foundation-Generic-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Foundation-Generic-Result.wv" \
    "$repository_root/Libraries/Foundation/Values/Option.wv" \
    "$repository_root/Libraries/Foundation/Values/Result.wv" \
    "$work/Foundation-Generic-B.wvb" \
    >"$work/Foundation-Generic-B.out" \
    2>"$work/Foundation-Generic-B.err" || exit $?
[[ ! -s $work/Foundation-Generic-A.err && \
    ! -s $work/Foundation-Generic-B.err ]] || exit 1
cmp -s -- "$work/Foundation-Generic-A.out" \
    "$work/Foundation-Generic-B.out" || exit 1
cmp -s -- "$work/Foundation-Generic-A.wvb" \
    "$work/Foundation-Generic-B.wvb" || exit 1

for name in Result-Wrong-Arity Result-Extra-Argument Result-Bare Try-Wrong-Error; do
    expect_foundation_generic_rejection \
        "$repository_root/Tests/Fixtures/Language-1.0/Foundation-Generic-$name.wv" \
        "$work/Foundation-Generic-$name.wvb" || exit 1
done

"$work/Verifier.elf" "$work/Foundation-Generic-A.wvb" \
    >"$work/Verify-Foundation-Generic.out" \
    2>"$work/Verify-Foundation-Generic.err" || exit $?
grep -Fq 'wvb status=Valid profile=compiler-aligned' \
    "$work/Verify-Foundation-Generic.out" || exit 1
"$work/Floating-Runner.elf" "$work/Foundation-Generic-A.wvb" \
    >"$work/Foundation-Generic-Run.out" \
    2>"$work/Foundation-Generic-Run.err" || exit $?
[[ ! -s $work/Foundation-Generic-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Foundation-Generic.out"
cmp -s -- "$work/Expected-Foundation-Generic.out" \
    "$work/Foundation-Generic-Run.out" || exit 1
foundation_generic_wvb_bytes=$(wc -c < "$work/Foundation-Generic-A.wvb")
printf 'INFO  language 1 foundation-generics wvb-bytes=%s\n' \
    "$foundation_generic_wvb_bytes"

echo 'START language 1 front door step=generic-specializations'
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Multiple-Specializations.wv" \
    "$work/Generic-Specializations-A.wvb" \
    >"$work/Generic-Specializations-A.out" \
    2>"$work/Generic-Specializations-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Multiple-Specializations.wv" \
    "$work/Generic-Specializations-B.wvb" \
    >"$work/Generic-Specializations-B.out" \
    2>"$work/Generic-Specializations-B.err" || exit $?
[[ ! -s $work/Generic-Specializations-A.err && \
    ! -s $work/Generic-Specializations-B.err ]] || exit 1
cmp -s -- "$work/Generic-Specializations-A.out" \
    "$work/Generic-Specializations-B.out" || exit 1
cmp -s -- "$work/Generic-Specializations-A.wvb" \
    "$work/Generic-Specializations-B.wvb" || exit 1
"$work/Verifier.elf" "$work/Generic-Specializations-A.wvb" \
    >"$work/Verify-Generic-Specializations.out" \
    2>"$work/Verify-Generic-Specializations.err" || exit $?
grep -Fq 'wvb status=Valid profile=compiler-aligned' \
    "$work/Verify-Generic-Specializations.out" || exit 1
"$work/Floating-Runner.elf" "$work/Generic-Specializations-A.wvb" \
    >"$work/Generic-Specializations-Run.out" \
    2>"$work/Generic-Specializations-Run.err" || exit $?
[[ ! -s $work/Generic-Specializations-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Generic-Specializations.out"
cmp -s -- "$work/Expected-Generic-Specializations.out" \
    "$work/Generic-Specializations-Run.out" || exit 1
generic_specializations_wvb_bytes=$(wc -c < \
    "$work/Generic-Specializations-A.wvb")
printf 'PASS  language 1 front door step=generic-specializations wvb-bytes=%s\n' \
    "$generic_specializations_wvb_bytes"
echo 'PASS  language 1 front door phase=foundation-generics item=11/11'
printf 'native language 1 front door status=Passed cases=160 frozen-inputs=251 source-fixtures=72 descriptor-cases=33 profile-cases=4 value-front-end-cases=39 generic-front-end-cases=4 generic-resolution-cases=1 generic-type-catalog-cases=1 generic-specialization-cases=4 generic-wir-cases=4 compiler-cases=36 fixed-integer-cases=22 rune-cases=20 floating-cases=27 unit-never-cases=21 multi-field-variant-cases=25 typed-failure-cases=5 foundation-generic-cases=5 compiler-result=42 compiler-wvb-bytes=221 generic-wir-wvb-bytes=%s generic-type-catalog-wvb-bytes=%s value-if-wvb-bytes=%s value-match-wvb-bytes=%s value-match-never-wvb-bytes=%s unit-wvb-bytes=%s never-wvb-bytes=%s record-update-wvb-bytes=1116 fixed-integer-wvb-bytes=5335 rune-wvb-bytes=%s floating-wvb-bytes=%s multi-field-variant-wvb-bytes=%s typed-failure-wvb-bytes=%s foundation-generic-wvb-bytes=%s generic-specializations-wvb-bytes=%s\n' "$generic_wir_wvb_bytes" "$generic_type_catalog_wvb_bytes" "$value_if_wvb_bytes" "$value_match_wvb_bytes" "$value_match_never_wvb_bytes" "$unit_wvb_bytes" "$never_wvb_bytes" "$rune_wvb_bytes" "$floating_wvb_bytes" "$multi_field_variant_wvb_bytes" "$result_try_wvb_bytes" "$foundation_generic_wvb_bytes" "$generic_specializations_wvb_bytes"
