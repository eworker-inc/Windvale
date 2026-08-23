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
bootstrap_analyzer_wvb="$repository_root/Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/Wvb/wvanalyze.wvb"
bootstrap_emitter_wvb="$repository_root/Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/Wvb/wvemit.wvb"
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-language-1-front-door.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-language-1-front-door.*)
            for malformed in \
                Memory-Budget-Entry-Malformed Fixed-Integer-Malformed \
                Rune-Malformed Floating-Malformed \
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

expect_analysis_failure() {
    local fixture=$1
    local prefix=$2
    local expected_status=$3
    local analysis_exit
    local -a analysis_lines
    if "$work/Analyzer.elf" "$fixture" \
        "$work/$prefix.wvss" "$work/$prefix.wvca" \
        "$work/$prefix.wvlb" "$work/$prefix.wvir" \
        >"$work/$prefix.out" 2>"$work/$prefix.err"; then
        return 1
    else
        analysis_exit=$?
    fi
    [[ $analysis_exit -eq 1 && ! -s $work/$prefix.out ]] || return 1
    [[ ! -e $work/$prefix.wvss && ! -e $work/$prefix.wvca && \
        ! -e $work/$prefix.wvlb && ! -e $work/$prefix.wvir ]] || return 1
    mapfile -t analysis_lines <"$work/$prefix.err"
    [[ ${#analysis_lines[@]} -eq 1 ]] || return 1
    [[ ${analysis_lines[0]} == \
        "source analysis status=Sourceˉwir symbol-status=Valid binding-status=Valid wir-status=$expected_status" ]]
}

expect_profiled_analysis_failure() {
    local fixture=$1
    local prefix=$2
    local expected_status=$3
    local analysis_exit
    local -a analysis_lines
    "$work/Admitter.elf" \
        --source-input-lock "$source_lock" "$source_lock_hash" \
        --source-profile "$source_profile" \
        "$fixture" "$work/$prefix.wvss" \
        >"$work/$prefix-admission.out" 2>"$work/$prefix-admission.err" || return 1
    [[ ! -s $work/$prefix-admission.err ]] || return 1
    if "$work/Analyzer.elf" --admitted-source-set \
        "$work/$prefix.wvss" "$work/$prefix.wvss" \
        "$work/$prefix.wvca" "$work/$prefix.wvlb" "$work/$prefix.wvir" \
        >"$work/$prefix.out" 2>"$work/$prefix.err"; then
        return 1
    else
        analysis_exit=$?
    fi
    [[ $analysis_exit -eq 1 && ! -s $work/$prefix.out ]] || return 1
    [[ ! -e $work/$prefix.wvca && ! -e $work/$prefix.wvlb && \
        ! -e $work/$prefix.wvir ]] || return 1
    mapfile -t analysis_lines <"$work/$prefix.err"
    [[ ${#analysis_lines[@]} -eq 1 ]] || return 1
    [[ ${analysis_lines[0]} == \
        "source analysis status=Sourceˉwir symbol-status=Valid binding-status=Valid wir-status=$expected_status" ]]
}

expect_profiled_symbol_failure() {
    local fixture=$1
    local prefix=$2
    local expected_status=$3
    local analysis_exit
    local -a analysis_lines
    "$work/Admitter.elf" \
        --source-input-lock "$source_lock" "$source_lock_hash" \
        --source-profile "$source_profile" \
        "$fixture" "$work/$prefix.wvss" \
        >"$work/$prefix-admission.out" 2>"$work/$prefix-admission.err" || return 1
    [[ ! -s $work/$prefix-admission.err ]] || return 1
    if "$work/Analyzer.elf" --admitted-source-set \
        "$work/$prefix.wvss" "$work/$prefix.wvss" \
        "$work/$prefix.wvca" "$work/$prefix.wvlb" "$work/$prefix.wvir" \
        >"$work/$prefix.out" 2>"$work/$prefix.err"; then
        return 1
    else
        analysis_exit=$?
    fi
    [[ $analysis_exit -eq 1 && ! -s $work/$prefix.out ]] || return 1
    [[ ! -e $work/$prefix.wvca && ! -e $work/$prefix.wvlb && \
        ! -e $work/$prefix.wvir ]] || return 1
    mapfile -t analysis_lines <"$work/$prefix.err"
    [[ ${#analysis_lines[@]} -eq 1 ]] || return 1
    [[ ${analysis_lines[0]} == \
        "source analysis status=Sourceˉsymbols symbol-status=$expected_status binding-status=Sourceˉsymbols wir-status=Sourceˉbindings" ]]
}

expect_profiled_analysis_failure_with_dependencies() {
    local fixture=$1
    local prefix=$2
    local expected_status=$3
    local first_dependency=$4
    local second_dependency=${5-}
    local analysis_exit
    local -a analysis_lines admission_arguments
    admission_arguments=(
        --source-input-lock "$source_lock" "$source_lock_hash"
        --source-profile "$source_profile"
        "$fixture" "$first_dependency"
    )
    if [[ -n $second_dependency ]]; then
        admission_arguments+=("$second_dependency")
    fi
    admission_arguments+=("$work/$prefix.wvss")
    "$work/Admitter.elf" "${admission_arguments[@]}" \
        >"$work/$prefix-admission.out" 2>"$work/$prefix-admission.err" || return 1
    [[ ! -s $work/$prefix-admission.err ]] || return 1
    if "$work/Analyzer.elf" --admitted-source-set \
        "$work/$prefix.wvss" "$work/$prefix.wvss" \
        "$work/$prefix.wvca" "$work/$prefix.wvlb" "$work/$prefix.wvir" \
        >"$work/$prefix.out" 2>"$work/$prefix.err"; then
        return 1
    else
        analysis_exit=$?
    fi
    [[ $analysis_exit -eq 1 && ! -s $work/$prefix.out ]] || return 1
    [[ ! -e $work/$prefix.wvca && ! -e $work/$prefix.wvlb && \
        ! -e $work/$prefix.wvir ]] || return 1
    mapfile -t analysis_lines <"$work/$prefix.err"
    [[ ${#analysis_lines[@]} -eq 1 ]] || return 1
    [[ ${analysis_lines[0]} == \
        "source analysis status=Sourceˉwir symbol-status=Valid binding-status=Valid wir-status=$expected_status" ]]
}

expect_profiled_symbol_failure_with_dependency() {
    local fixture=$1
    local prefix=$2
    local expected_status=$3
    local dependency=$4
    local analysis_exit
    local -a analysis_lines
    "$work/Admitter.elf" \
        --source-input-lock "$source_lock" "$source_lock_hash" \
        --source-profile "$source_profile" \
        "$fixture" "$dependency" "$work/$prefix.wvss" \
        >"$work/$prefix-admission.out" 2>"$work/$prefix-admission.err" || return 1
    [[ ! -s $work/$prefix-admission.err ]] || return 1
    if "$work/Analyzer.elf" --admitted-source-set \
        "$work/$prefix.wvss" "$work/$prefix.wvss" \
        "$work/$prefix.wvca" "$work/$prefix.wvlb" "$work/$prefix.wvir" \
        >"$work/$prefix.out" 2>"$work/$prefix.err"; then
        return 1
    else
        analysis_exit=$?
    fi
    [[ $analysis_exit -eq 1 && ! -s $work/$prefix.out ]] || return 1
    [[ ! -e $work/$prefix.wvca && ! -e $work/$prefix.wvlb && \
        ! -e $work/$prefix.wvir ]] || return 1
    mapfile -t analysis_lines <"$work/$prefix.err"
    [[ ${#analysis_lines[@]} -eq 1 ]] || return 1
    [[ ${analysis_lines[0]} == \
        "source analysis status=Sourceˉsymbols symbol-status=$expected_status binding-status=Sourceˉsymbols wir-status=Sourceˉbindings" ]]
}

expect_profiled_emission_failure() {
    local fixture=$1
    local prefix=$2
    local expected_line=$3
    local emission_exit
    local -a emission_lines
    "$work/Admitter.elf" \
        --source-input-lock "$source_lock" "$source_lock_hash" \
        --source-profile "$source_profile" \
        "$fixture" "$work/$prefix.wvss" \
        >"$work/$prefix-admission.out" 2>"$work/$prefix-admission.err" || return 1
    [[ ! -s $work/$prefix-admission.err ]] || return 1
    "$work/Analyzer.elf" --admitted-source-set \
        "$work/$prefix.wvss" "$work/$prefix.wvss" \
        "$work/$prefix.wvca" "$work/$prefix.wvlb" "$work/$prefix.wvir" \
        >"$work/$prefix-analysis.out" 2>"$work/$prefix-analysis.err" || return 1
    [[ ! -s $work/$prefix-analysis.err ]] || return 1
    if "$work/Emitter.elf" \
        "$work/$prefix.wvss" "$work/$prefix.wvca" \
        "$work/$prefix.wvlb" "$work/$prefix.wvir" "$work/$prefix.wvb" \
        >"$work/$prefix-emission.out" 2>"$work/$prefix-emission.err"; then
        return 1
    else
        emission_exit=$?
    fi
    [[ $emission_exit -eq 1 && ! -s $work/$prefix-emission.out && \
        ! -e $work/$prefix.wvb ]] || return 1
    mapfile -t emission_lines <"$work/$prefix-emission.err"
    [[ ${#emission_lines[@]} -eq 1 && \
        ${emission_lines[0]} == "$expected_line" ]]
}

echo 'START language 1 front door phase=frozen-fixtures item=1/13'
node "$script_directory/Verify-Language-1.0-Migration-Fixtures.mjs" || exit $?
echo 'PASS  language 1 front door phase=frozen-fixtures item=1/13'

echo 'START language 1 front door phase=descriptor item=2/13'
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
echo 'PASS  language 1 front door phase=descriptor item=2/13'

echo 'START language 1 front door phase=value-front-end item=3/13'
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
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 1 \
    "$work/Generic-Calls.wvb" "$work/Generic-Calls.elf" \
    --development-cache \
    >"$work/Generic-Calls-Package.out" \
    2>"$work/Generic-Calls-Package.err" || exit $?
[[ ! -s $work/Generic-Calls-Package.err ]] || exit 1
"$work/Generic-Calls.elf" \
    >"$work/Generic-Calls.out" 2>"$work/Generic-Calls.err"
generic_calls_result=$?
[[ $generic_calls_result -eq 42 ]] || exit 1
[[ ! -s $work/Generic-Calls.out && ! -s $work/Generic-Calls.err ]] || exit 1
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
echo 'PASS  language 1 front door phase=value-front-end item=3/13'

echo 'START language 1 front door phase=compiler-slice item=4/13'
[[ $(wc -c < "$bootstrap_analyzer_wvb") -eq 992412 ]] || exit 1
printf '%s  %s\n' \
    26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120 \
    "$bootstrap_analyzer_wvb" | sha256sum --check --strict --quiet || exit $?
[[ $(wc -c < "$bootstrap_emitter_wvb") -eq 895787 ]] || exit 1
printf '%s  %s\n' \
    ea8ade4774236a84208242a6e17d271077b9a4a94fb40c47ec487d43a97b2b94 \
    "$bootstrap_emitter_wvb" | sha256sum --check --strict --quiet || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 7 \
    "$bootstrap_analyzer_wvb" "$work/Bootstrap-Analyzer.elf" \
    --development-cache || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 7 \
    "$bootstrap_emitter_wvb" "$work/Bootstrap-Emitter.elf" \
    --development-cache || exit $?
node "$script_directory/Write-Split-Compiler-Producer-Identity.mjs" \
    analyzer "$work/Bootstrap-Analyzer.elf" \
    "$work/Bootstrap-Analyzer.identity" || exit $?
node "$script_directory/Write-Split-Compiler-Producer-Identity.mjs" \
    emitter "$work/Bootstrap-Emitter.elf" \
    "$work/Bootstrap-Emitter.identity" || exit $?
node "$script_directory/Build-Cached-Split-Project-Wvb.mjs" \
    "$repository_root/Projects/Tools/Windvale-Compiler-Admission-Driver.wvproj" \
    "$work/Admitter.wvb" \
    "$work/Bootstrap-Analyzer.elf" "$work/Bootstrap-Analyzer.identity" \
    "$work/Bootstrap-Emitter.elf" "$work/Bootstrap-Emitter.identity" || exit $?
node "$script_directory/Build-Cached-Split-Project-Wvb.mjs" \
    "$repository_root/Projects/Tools/Windvale-Compiler-Analysis-Driver.wvproj" \
    "$work/Analyzer.wvb" \
    "$work/Bootstrap-Analyzer.elf" "$work/Bootstrap-Analyzer.identity" \
    "$work/Bootstrap-Emitter.elf" "$work/Bootstrap-Emitter.identity" || exit $?
printf 'INFO  language 1 admitter wvb-bytes=%s sha256=%s\n' \
    "$(wc -c < "$work/Admitter.wvb")" \
    "$(sha256sum -- "$work/Admitter.wvb" | cut -d' ' -f1)"
[[ $(wc -c < "$work/Admitter.wvb") -eq 82924 ]] || exit 1
printf '%s  %s\n' \
    7a7da249ff51647e2c279a9d06c05897f071683991aca0748ad6f40e02887512 \
    "$work/Admitter.wvb" | sha256sum --check --strict --quiet || exit $?
printf 'INFO  language 1 analyzer wvb-bytes=%s sha256=%s\n' \
    "$(wc -c < "$work/Analyzer.wvb")" \
    "$(sha256sum -- "$work/Analyzer.wvb" | cut -d' ' -f1)"
[[ $(wc -c < "$work/Analyzer.wvb") -eq 1132570 ]] || exit 1
printf '%s  %s\n' \
    e3eef9e462f47cb88d4de174eb1e714106b346137538d9e6b396361b834d8471 \
    "$work/Analyzer.wvb" | sha256sum --check --strict --quiet || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 \
    "$work/Admitter.wvb" "$work/Admitter.elf" --development-cache || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 7 \
    "$work/Analyzer.wvb" "$work/Analyzer.elf" --development-cache || exit $?
node "$script_directory/Write-Split-Compiler-Producer-Identity.mjs" \
    analyzer "$work/Analyzer.elf" "$work/Analyzer.identity" || exit $?
echo 'START language 1 front door step=generic-nominal-main-pipeline'
"$work/Analyzer.elf" \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Main-Pipeline.wv" \
    "$work/Generic-Nominal-Main.wvss" \
    "$work/Generic-Nominal-Main.wvca" \
    "$work/Generic-Nominal-Main.wvlb" \
    "$work/Generic-Nominal-Main.wvir" \
    >"$work/Generic-Nominal-Main.out" \
    2>"$work/Generic-Nominal-Main.err" || exit $?
[[ ! -s $work/Generic-Nominal-Main.err ]] || exit 1
cat -- "$work/Generic-Nominal-Main.out"
echo 'INFO  language 1 front door step=generic-nominal-main-pipeline analysis=Published'
echo 'START language 1 front door step=generic-nominal-function-body'
"$work/Analyzer.elf" \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Function-Body.wv" \
    "$work/Generic-Nominal-Function-Body.wvss" \
    "$work/Generic-Nominal-Function-Body.wvca" \
    "$work/Generic-Nominal-Function-Body.wvlb" \
    "$work/Generic-Nominal-Function-Body.wvir" \
    >"$work/Generic-Nominal-Function-Body.out" \
    2>"$work/Generic-Nominal-Function-Body.err" || exit $?
[[ ! -s $work/Generic-Nominal-Function-Body.err ]] || exit 1
cat -- "$work/Generic-Nominal-Function-Body.out"
echo 'INFO  language 1 front door step=generic-nominal-function-body analysis=Published'
echo 'START language 1 front door step=generic-nominal-declaration-dependency'
"$work/Analyzer.elf" \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Declaration-Dependency.wv" \
    "$work/Generic-Nominal-Declaration-Dependency.wvss" \
    "$work/Generic-Nominal-Declaration-Dependency.wvca" \
    "$work/Generic-Nominal-Declaration-Dependency.wvlb" \
    "$work/Generic-Nominal-Declaration-Dependency.wvir" \
    >"$work/Generic-Nominal-Declaration-Dependency.out" \
    2>"$work/Generic-Nominal-Declaration-Dependency.err" || exit $?
[[ ! -s $work/Generic-Nominal-Declaration-Dependency.err ]] || exit 1
cat -- "$work/Generic-Nominal-Declaration-Dependency.out"
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Declaration-Cycle.wv" \
    Generic-Nominal-Declaration-Cycle Genericˉresolution || exit 1
echo 'INFO  language 1 front door step=generic-nominal-declaration-dependency analysis=Published cycle=Rejected'
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Missing-Field.wv" \
    Generic-Nominal-Missing-Field Missingˉrecordˉfield || exit 1
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Duplicate-Field.wv" \
    Generic-Nominal-Duplicate-Field Duplicateˉrecordˉfield || exit 1
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Field-Type-Mismatch.wv" \
    Generic-Nominal-Field-Type-Mismatch Typeˉmismatch || exit 1
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Unknown-Field.wv" \
    Generic-Nominal-Unknown-Field Unknownˉfield || exit 1
echo 'PASS  language 1 front door step=generic-nominal-record-rejections cases=4'
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Function-Body-Type-Mismatch.wv" \
    Generic-Nominal-Function-Body-Type-Mismatch Typeˉmismatch || exit 1
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Function-Body-Unknown-Field.wv" \
    Generic-Nominal-Function-Body-Unknown-Field Unknownˉfield || exit 1
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Function-Body-Inference-Mismatch.wv" \
    Generic-Nominal-Function-Body-Inference-Mismatch Genericˉresolution || exit 1
echo 'PASS  language 1 front door step=generic-nominal-function-body-rejections cases=3'
node "$script_directory/Build-Cached-Split-Project-Wvb.mjs" \
    "$repository_root/Projects/Tools/Windvale-Compiler-Emission-Driver.wvproj" \
    "$work/Emitter.wvb" \
    "$work/Analyzer.elf" "$work/Analyzer.identity" \
    "$work/Bootstrap-Emitter.elf" "$work/Bootstrap-Emitter.identity" || exit $?
printf 'INFO  language 1 emitter wvb-bytes=%s sha256=%s\n' \
    "$(wc -c < "$work/Emitter.wvb")" \
    "$(sha256sum -- "$work/Emitter.wvb" | cut -d' ' -f1)"
[[ $(wc -c < "$work/Emitter.wvb") -eq 1055285 ]] || exit 1
printf '%s  %s\n' \
    bd87930696685475920bdc73dcf72dde01ae0eb5dae94579e28b9a79d018d606 \
    "$work/Emitter.wvb" | sha256sum --check --strict --quiet || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 7 \
    "$work/Emitter.wvb" "$work/Emitter.elf" --development-cache || exit $?
node "$script_directory/Write-Split-Compiler-Producer-Identity.mjs" \
    emitter "$work/Emitter.elf" "$work/Emitter.identity" || exit $?
echo 'START language 1 front door step=enum-backing'
"$work/Admitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Enum-Backing-All.wv" \
    "$work/Enum-Backing-All.wvss" \
    >"$work/Enum-Backing-All-Admission.out" \
    2>"$work/Enum-Backing-All-Admission.err" || exit $?
[[ ! -s $work/Enum-Backing-All-Admission.err ]] || exit 1
"$work/Analyzer.elf" --admitted-source-set \
    "$work/Enum-Backing-All.wvss" "$work/Enum-Backing-All.wvss" \
    "$work/Enum-Backing-All.wvca" "$work/Enum-Backing-All.wvlb" \
    "$work/Enum-Backing-All.wvir" \
    >"$work/Enum-Backing-All-Analysis.out" \
    2>"$work/Enum-Backing-All-Analysis.err" || exit $?
[[ ! -s $work/Enum-Backing-All-Analysis.err ]] || exit 1
grep -q '^source analysis status=Published ' \
    "$work/Enum-Backing-All-Analysis.out" || exit 1
"$work/Emitter.elf" \
    "$work/Enum-Backing-All.wvss" "$work/Enum-Backing-All.wvca" \
    "$work/Enum-Backing-All.wvlb" "$work/Enum-Backing-All.wvir" \
    "$work/Enum-Backing-All.wvb" \
    >"$work/Enum-Backing-All-Emission.out" \
    2>"$work/Enum-Backing-All-Emission.err" || exit $?
[[ ! -s $work/Enum-Backing-All-Emission.err ]] || exit 1
"$script_directory/Run-Wvb.sh" "$work/Enum-Backing-All.wvb" \
    >"$work/Enum-Backing-All-Run.out" \
    2>"$work/Enum-Backing-All-Run.err" || exit $?
[[ ! -s $work/Enum-Backing-All-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Enum-Backing-All.out"
cmp -s -- "$work/Expected-Enum-Backing-All.out" \
    "$work/Enum-Backing-All-Run.out" || exit 1
enum_dead_type_wvb_bytes=$(wc -c < "$work/Enum-Backing-All.wvb")
[[ $enum_dead_type_wvb_bytes -eq 217 ]] || exit 1
node "$script_directory/Run-Split-Compiler.mjs" \
    "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Enum-U8-Used-Main.wv" \
    "$work/Enum-U8-Used-Main-A.wvb" \
    >"$work/Enum-U8-Used-Main-A.out" \
    2>"$work/Enum-U8-Used-Main-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" \
    "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Enum-U8-Used-Main.wv" \
    "$work/Enum-U8-Used-Main-B.wvb" \
    >"$work/Enum-U8-Used-Main-B.out" \
    2>"$work/Enum-U8-Used-Main-B.err" || exit $?
[[ ! -s $work/Enum-U8-Used-Main-A.err ]] || exit 1
[[ ! -s $work/Enum-U8-Used-Main-B.err ]] || exit 1
cmp -s -- "$work/Enum-U8-Used-Main-A.wvb" \
    "$work/Enum-U8-Used-Main-B.wvb" || exit 1
enum_u8_wvb_bytes=$(wc -c < "$work/Enum-U8-Used-Main-A.wvb")
[[ $enum_u8_wvb_bytes -eq 415 ]] || exit 1
printf '%s  %s\n' \
    961ba417955a523b9fc21e0b71df7a8d99613252b7450700dd4381aa94e825ed \
    "$work/Enum-U8-Used-Main-A.wvb" |
    sha256sum --check --strict --quiet || exit $?
expect_profiled_symbol_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Enum-Backing-Duplicate-Signed.wv" \
    Enum-Backing-Duplicate-Signed Duplicateˉenumˉvalue || exit 1
expect_profiled_symbol_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Enum-Backing-Mismatched-Suffix.wv" \
    Enum-Backing-Mismatched-Suffix Invalidˉenumˉvalue || exit 1
expect_profiled_symbol_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Enum-Backing-Out-Of-Range.wv" \
    Enum-Backing-Out-Of-Range Invalidˉenumˉvalue || exit 1
expect_profiled_symbol_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Enum-Backing-Unsigned-Negative.wv" \
    Enum-Backing-Unsigned-Negative Invalidˉenumˉvalue || exit 1
expect_profiled_symbol_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Enum-Missing-Backing.wv" \
    Enum-Missing-Backing Missingˉenumˉbacking || exit 1
node "$script_directory/Run-Split-Compiler.mjs" \
    "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Enum-I32-Negative-Main.wv" \
    "$work/Enum-I32-Negative-Main.wvb" \
    >"$work/Enum-I32-Negative-Main.out" \
    2>"$work/Enum-I32-Negative-Main.err" || exit $?
[[ ! -s $work/Enum-I32-Negative-Main.err ]] || exit 1
"$script_directory/Run-Wvb.sh" "$work/Enum-I32-Negative-Main.wvb" \
    >"$work/Enum-I32-Negative-Main-Run.out" \
    2>"$work/Enum-I32-Negative-Main-Run.err" || exit $?
[[ ! -s $work/Enum-I32-Negative-Main-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Enum-I32.out"
cmp -s -- "$work/Expected-Enum-I32.out" \
    "$work/Enum-I32-Negative-Main-Run.out" || exit 1
enum_i32_wvb_bytes=$(wc -c < "$work/Enum-I32-Negative-Main.wvb")
[[ $enum_i32_wvb_bytes -eq 427 ]] || exit 1
echo 'PASS  language 1 front door step=enum-backing cases=9 analysis=all-fixed-widths wvb=i32-only execution=42'
echo 'START language 1 front door step=borrow-call-semantics'
"$work/Analyzer.elf" \
    "$repository_root/Tests/Fixtures/Language-1.0/Borrow-Call-Main-Pipeline.wv" \
    "$work/Borrow-Call.wvss" "$work/Borrow-Call.wvca" \
    "$work/Borrow-Call.wvlb" "$work/Borrow-Call.wvir" \
    >"$work/Borrow-Call-Analysis.out" \
    2>"$work/Borrow-Call-Analysis.err" || exit $?
[[ ! -s $work/Borrow-Call-Analysis.err ]] || exit 1
"$work/Emitter.elf" \
    "$work/Borrow-Call.wvss" "$work/Borrow-Call.wvca" \
    "$work/Borrow-Call.wvlb" "$work/Borrow-Call.wvir" \
    "$work/Borrow-Call.wvb" \
    >"$work/Borrow-Call-Emission.out" \
    2>"$work/Borrow-Call-Emission.err" || exit $?
[[ ! -s $work/Borrow-Call-Emission.err ]] || exit 1
"$script_directory/Verify-Wvb.sh" "$work/Borrow-Call.wvb" || exit $?
node "$script_directory/Run-WebAssembly-Scalar-Wvb.mjs" \
    "$repository_root/Artifacts/WebAssembly-Playground/Wvb-Scalar-Interpreter.wasm" \
    "$work/Borrow-Call.wvb" 42 \
    >"$work/Borrow-Call-Run.out" 2>"$work/Borrow-Call-Run.err" || exit $?
[[ ! -s $work/Borrow-Call-Run.err ]] || exit 1
grep -Fq 'webassembly scalar status=Valid result=42' \
    "$work/Borrow-Call-Run.out" || exit 1
echo 'PASS  language 1 front door step=borrow-call-semantics item=execution result=42 engine=webassembly'
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Borrow-Missing-Explicit.wv" \
    Borrow-Missing-Explicit Invalidˉborrow || exit 1
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Borrow-Immutable-To-Mutable.wv" \
    Borrow-Immutable-To-Mutable Invalidˉborrow || exit 1
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Borrow-Mutable-From-Let.wv" \
    Borrow-Mutable-From-Let Invalidˉborrow || exit 1
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Borrow-Escape-Local.wv" \
    Borrow-Escape-Local Invalidˉborrow || exit 1
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Borrow-Return.wv" \
    Borrow-Return Invalidˉborrow || exit 1
expect_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Borrow-Owned-Read-Through.wv" \
    Borrow-Owned-Read-Through Invalidˉborrow || exit 1
echo 'PASS  language 1 front door step=borrow-call-semantics item=direct-rejections cases=6'
"$work/Admitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Borrow-Sequence-Read-Through.wv" \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" \
    "$work/Borrow-Sequence-Admitted.wvss" \
    >"$work/Borrow-Sequence-Admission.out" \
    2>"$work/Borrow-Sequence-Admission.err" || exit $?
[[ ! -s $work/Borrow-Sequence-Admission.err ]] || exit 1
"$work/Analyzer.elf" --admitted-source-set \
    "$work/Borrow-Sequence-Admitted.wvss" \
    "$work/Borrow-Sequence.wvss" "$work/Borrow-Sequence.wvca" \
    "$work/Borrow-Sequence.wvlb" "$work/Borrow-Sequence.wvir" \
    >"$work/Borrow-Sequence.out" 2>"$work/Borrow-Sequence.err" || exit $?
[[ ! -s $work/Borrow-Sequence.err ]] || exit 1
grep -Fq 'source analysis status=Published' "$work/Borrow-Sequence.out" || exit 1
echo 'PASS  language 1 front door step=borrow-call-semantics item=sequence ownership=Shared'
"$work/Admitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Borrow-Vector-Owned-Read-Through.wv" \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" \
    "$work/Borrow-Vector-Admitted.wvss" \
    >"$work/Borrow-Vector-Admission.out" \
    2>"$work/Borrow-Vector-Admission.err" || exit $?
[[ ! -s $work/Borrow-Vector-Admission.err ]] || exit 1
if "$work/Analyzer.elf" --admitted-source-set \
    "$work/Borrow-Vector-Admitted.wvss" \
    "$work/Borrow-Vector.wvss" "$work/Borrow-Vector.wvca" \
    "$work/Borrow-Vector.wvlb" "$work/Borrow-Vector.wvir" \
    >"$work/Borrow-Vector.out" 2>"$work/Borrow-Vector.err"; then
    exit 1
else
    borrow_vector_exit=$?
fi
[[ $borrow_vector_exit -eq 1 && ! -s $work/Borrow-Vector.out ]] || exit 1
[[ ! -e $work/Borrow-Vector.wvir ]] || exit 1
grep -Fq 'wir-status=Invalidˉborrow' "$work/Borrow-Vector.err" || exit 1
echo 'PASS  language 1 front door step=borrow-call-semantics item=vector ownership=Owned rejection=Invalid-borrow'
"$work/Admitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Memory-Budget-Type-Identity.wv" \
    "$repository_root/Libraries/Foundation/Memory/Memory.wv" \
    "$work/Memory-Budget-Admitted.wvss" \
    >"$work/Memory-Budget-Admission.out" \
    2>"$work/Memory-Budget-Admission.err" || exit $?
[[ ! -s $work/Memory-Budget-Admission.err ]] || exit 1
"$work/Analyzer.elf" --admitted-source-set \
    "$work/Memory-Budget-Admitted.wvss" \
    "$work/Memory-Budget.wvss" "$work/Memory-Budget.wvca" \
    "$work/Memory-Budget.wvlb" "$work/Memory-Budget.wvir" \
    >"$work/Memory-Budget-Analysis.out" \
    2>"$work/Memory-Budget-Analysis.err" || exit $?
[[ ! -s $work/Memory-Budget-Analysis.err ]] || exit 1
grep -Fq 'source analysis status=Published' \
    "$work/Memory-Budget-Analysis.out" || exit 1
if "$work/Emitter.elf" \
    "$work/Memory-Budget.wvss" "$work/Memory-Budget.wvca" \
    "$work/Memory-Budget.wvlb" "$work/Memory-Budget.wvir" \
    "$work/Memory-Budget.wvb" \
    >"$work/Memory-Budget-Emission.out" \
    2>"$work/Memory-Budget-Emission.err"; then
    exit 1
else
    memory_budget_emission_exit=$?
fi
[[ $memory_budget_emission_exit -eq 1 && \
    ! -s $work/Memory-Budget-Emission.out && \
    ! -e $work/Memory-Budget.wvb ]] || exit 1
mapfile -t memory_budget_emission_lines <"$work/Memory-Budget-Emission.err"
[[ ${#memory_budget_emission_lines[@]} -eq 1 && \
    ${memory_budget_emission_lines[0]} == \
    'source emission status=Valid analysis-status=Valid wvb-status=Unsupportedˉshape function=1 operation=4 source-line=0' ]] || exit 1
echo 'PASS  language 1 front door step=borrow-call-semantics item=memory-budget identity=Owned-WVIR wvb=Unsupported-shape'
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Memory-Budget-Owned-Read-Through.wv" \
    Memory-Budget-Owned-Read-Through Invalidˉborrow \
    "$repository_root/Libraries/Foundation/Memory/Memory.wv" || exit $?
expect_profiled_symbol_failure_with_dependency \
    "$repository_root/Tests/Fixtures/Language-1.0/Memory-Budget-Unqualified.wv" \
    Memory-Budget-Unqualified Unknownˉtype \
    "$repository_root/Libraries/Foundation/Memory/Memory.wv" || exit $?
expect_profiled_symbol_failure_with_dependency \
    "$repository_root/Tests/Fixtures/Language-1.0/Memory-Budget-Lookalike-Module.wv" \
    Memory-Budget-Lookalike-Module Unknownˉtype \
    "$repository_root/Tests/Fixtures/Language-1.0/Foundation-Memory-Lookalike.wv" || exit $?
echo 'PASS  language 1 front door step=borrow-call-semantics item=memory-rejections cases=3'
echo 'START language 1 front door step=memory-budget-entry'
"$work/Admitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Memory-Budget-Entry-Main.wv" \
    "$repository_root/Libraries/Foundation/Memory/Memory.wv" \
    "$work/Memory-Budget-Entry-Admitted.wvss" \
    >"$work/Memory-Budget-Entry-Admission.out" \
    2>"$work/Memory-Budget-Entry-Admission.err" || exit $?
[[ ! -s $work/Memory-Budget-Entry-Admission.err ]] || exit 1
"$work/Analyzer.elf" --admitted-source-set \
    "$work/Memory-Budget-Entry-Admitted.wvss" \
    "$work/Memory-Budget-Entry.wvss" "$work/Memory-Budget-Entry.wvca" \
    "$work/Memory-Budget-Entry.wvlb" "$work/Memory-Budget-Entry.wvir" \
    >"$work/Memory-Budget-Entry-Analysis.out" \
    2>"$work/Memory-Budget-Entry-Analysis.err" || exit $?
[[ ! -s $work/Memory-Budget-Entry-Analysis.err ]] || exit 1
grep -Fq 'source analysis status=Published' \
    "$work/Memory-Budget-Entry-Analysis.out" || exit 1
for suffix in A B; do
    "$work/Emitter.elf" \
        "$work/Memory-Budget-Entry.wvss" "$work/Memory-Budget-Entry.wvca" \
        "$work/Memory-Budget-Entry.wvlb" "$work/Memory-Budget-Entry.wvir" \
        "$work/Memory-Budget-Entry-$suffix.wvb" \
        >"$work/Memory-Budget-Entry-$suffix.out" \
        2>"$work/Memory-Budget-Entry-$suffix.err" || exit $?
    [[ ! -s $work/Memory-Budget-Entry-$suffix.err ]] || exit 1
done
cmp -s -- "$work/Memory-Budget-Entry-A.wvb" \
    "$work/Memory-Budget-Entry-B.wvb" || exit 1
memory_budget_entry_wvb_bytes=$(wc -c < "$work/Memory-Budget-Entry-A.wvb")
[[ $memory_budget_entry_wvb_bytes -eq 242 ]] || exit 1
printf 'PASS  language 1 front door step=memory-budget-entry item=compile format=WVB-1.21 deterministic=1 wvb-bytes=%s\n' \
    "$memory_budget_entry_wvb_bytes"
echo 'PASS  language 1 front door step=borrow-call-semantics cases=14 execution=42 vector=Owned sequence=Shared memory-budget=Owned-WVIR'
echo 'START language 1 front door step=generic-nominal-variant'
"$work/Admitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Variant.wv" \
    "$work/Generic-Nominal-Variant.wvss" \
    >"$work/Generic-Nominal-Variant-Admission.out" \
    2>"$work/Generic-Nominal-Variant-Admission.err" || exit $?
[[ ! -s $work/Generic-Nominal-Variant-Admission.err ]] || exit 1
"$work/Analyzer.elf" --admitted-source-set \
    "$work/Generic-Nominal-Variant.wvss" \
    "$work/Generic-Nominal-Variant.wvss" \
    "$work/Generic-Nominal-Variant.wvca" \
    "$work/Generic-Nominal-Variant.wvlb" \
    "$work/Generic-Nominal-Variant.wvir" \
    >"$work/Generic-Nominal-Variant-Analysis.out" \
    2>"$work/Generic-Nominal-Variant-Analysis.err" || exit $?
[[ ! -s $work/Generic-Nominal-Variant-Analysis.err ]] || exit 1
"$work/Emitter.elf" \
    "$work/Generic-Nominal-Variant.wvss" \
    "$work/Generic-Nominal-Variant.wvca" \
    "$work/Generic-Nominal-Variant.wvlb" \
    "$work/Generic-Nominal-Variant.wvir" \
    "$work/Generic-Nominal-Variant.wvb" \
    >"$work/Generic-Nominal-Variant-Emission.out" \
    2>"$work/Generic-Nominal-Variant-Emission.err" || exit $?
[[ ! -s $work/Generic-Nominal-Variant-Emission.err ]] || exit 1
expect_profiled_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Variant-Type-Mismatch.wv" \
    Generic-Nominal-Variant-Type-Mismatch Typeˉmismatch || exit 1
expect_profiled_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Variant-Missing-Field.wv" \
    Generic-Nominal-Variant-Missing-Field Missingˉvariantˉfield || exit 1
expect_profiled_analysis_failure \
    "$repository_root/Tests/Fixtures/Language-1.0/Generic-Nominal-Variant-Pattern-Type-Mismatch.wv" \
    Generic-Nominal-Variant-Pattern-Type-Mismatch Typeˉmismatch || exit 1
generic_nominal_variant_wvb_bytes=$(wc -c < \
    "$work/Generic-Nominal-Variant.wvb")
printf 'INFO  language 1 front door step=generic-nominal-variant analysis=Published wvb-bytes=%s rejections=3\n' \
    "$generic_nominal_variant_wvb_bytes"
"$work/Emitter.elf" \
    "$work/Generic-Nominal-Main.wvss" \
    "$work/Generic-Nominal-Main.wvca" \
    "$work/Generic-Nominal-Main.wvlb" \
    "$work/Generic-Nominal-Main.wvir" \
    "$work/Generic-Nominal-Main.wvb" \
    >"$work/Generic-Nominal-Main-Emission.out" \
    2>"$work/Generic-Nominal-Main-Emission.err" || exit $?
[[ ! -s $work/Generic-Nominal-Main-Emission.err ]] || exit 1
cat -- "$work/Generic-Nominal-Main-Emission.out"
"$script_directory/Verify-Wvb.sh" \
    "$work/Generic-Nominal-Main.wvb" || exit $?
node "$script_directory/Verify-Generic-Nominal-Main-Pipeline.mjs" \
    "$work/Generic-Nominal-Main.wvss" \
    "$work/Generic-Nominal-Main.wvca" \
    "$work/Generic-Nominal-Main.wvlb" \
    "$work/Generic-Nominal-Main.wvir" \
    "$work/Generic-Nominal-Main.wvb" || exit $?
"$script_directory/Run-Wvb.sh" "$work/Generic-Nominal-Main.wvb" \
    >"$work/Generic-Nominal-Main-Run.out" \
    2>"$work/Generic-Nominal-Main-Run.err" || exit $?
[[ ! -s $work/Generic-Nominal-Main-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Generic-Nominal-Main.out"
cmp -s -- "$work/Expected-Generic-Nominal-Main.out" \
    "$work/Generic-Nominal-Main-Run.out" || exit 1
echo 'PASS  language 1 front door step=generic-nominal-main-pipeline cases=26 verification=compiler-aligned execution=42'
"$work/Emitter.elf" \
    "$work/Generic-Nominal-Function-Body.wvss" \
    "$work/Generic-Nominal-Function-Body.wvca" \
    "$work/Generic-Nominal-Function-Body.wvlb" \
    "$work/Generic-Nominal-Function-Body.wvir" \
    "$work/Generic-Nominal-Function-Body.wvb" \
    >"$work/Generic-Nominal-Function-Body-Emission.out" \
    2>"$work/Generic-Nominal-Function-Body-Emission.err" || exit $?
[[ ! -s $work/Generic-Nominal-Function-Body-Emission.err ]] || exit 1
cat -- "$work/Generic-Nominal-Function-Body-Emission.out"
"$script_directory/Verify-Wvb.sh" \
    "$work/Generic-Nominal-Function-Body.wvb" || exit $?
node "$script_directory/Verify-Generic-Nominal-Function-Body.mjs" \
    "$work/Generic-Nominal-Function-Body.wvss" \
    "$work/Generic-Nominal-Function-Body.wvca" \
    "$work/Generic-Nominal-Function-Body.wvlb" \
    "$work/Generic-Nominal-Function-Body.wvir" \
    "$work/Generic-Nominal-Function-Body.wvb" || exit $?
"$script_directory/Run-Wvb.sh" "$work/Generic-Nominal-Function-Body.wvb" \
    >"$work/Generic-Nominal-Function-Body-Run.out" \
    2>"$work/Generic-Nominal-Function-Body-Run.err" || exit $?
[[ ! -s $work/Generic-Nominal-Function-Body-Run.err ]] || exit 1
cmp -s -- "$work/Expected-Generic-Nominal-Main.out" \
    "$work/Generic-Nominal-Function-Body-Run.out" || exit 1
echo 'PASS  language 1 front door step=generic-nominal-function-body cases=33 verification=compiler-aligned execution=42'
"$work/Emitter.elf" \
    "$work/Generic-Nominal-Declaration-Dependency.wvss" \
    "$work/Generic-Nominal-Declaration-Dependency.wvca" \
    "$work/Generic-Nominal-Declaration-Dependency.wvlb" \
    "$work/Generic-Nominal-Declaration-Dependency.wvir" \
    "$work/Generic-Nominal-Declaration-Dependency.wvb" \
    >"$work/Generic-Nominal-Declaration-Dependency-Emission.out" \
    2>"$work/Generic-Nominal-Declaration-Dependency-Emission.err" || exit $?
[[ ! -s $work/Generic-Nominal-Declaration-Dependency-Emission.err ]] || exit 1
cat -- "$work/Generic-Nominal-Declaration-Dependency-Emission.out"
"$script_directory/Verify-Wvb.sh" \
    "$work/Generic-Nominal-Declaration-Dependency.wvb" || exit $?
node "$script_directory/Verify-Generic-Nominal-Declaration-Dependency.mjs" \
    "$work/Generic-Nominal-Declaration-Dependency.wvss" \
    "$work/Generic-Nominal-Declaration-Dependency.wvca" \
    "$work/Generic-Nominal-Declaration-Dependency.wvlb" \
    "$work/Generic-Nominal-Declaration-Dependency.wvir" \
    "$work/Generic-Nominal-Declaration-Dependency.wvb" || exit $?
"$script_directory/Run-Wvb.sh" \
    "$work/Generic-Nominal-Declaration-Dependency.wvb" \
    >"$work/Generic-Nominal-Declaration-Dependency-Run.out" \
    2>"$work/Generic-Nominal-Declaration-Dependency-Run.err" || exit $?
[[ ! -s $work/Generic-Nominal-Declaration-Dependency-Run.err ]] || exit 1
cmp -s -- "$work/Expected-Generic-Nominal-Main.out" \
    "$work/Generic-Nominal-Declaration-Dependency-Run.out" || exit 1
echo 'PASS  language 1 front door step=generic-nominal-declaration-dependency cases=33 verification=compiler-aligned execution=42 cycle=Rejected'
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
[[ $(wc -c < "$work/Generic-Wir-A.wvb") -eq 1295691 ]] || exit 1
printf '%s  %s\n' \
    6afc2f4574158d5b151c7d4c0ec85eca132e26f88187f8d5fda8b2c866be9e6b \
    "$work/Generic-Wir-A.wvb" | sha256sum --check --strict --quiet || exit $?
generic_wir_wvb_bytes=$(wc -c < "$work/Generic-Wir-A.wvb")
printf 'INFO  language 1 front door step=generic-wir-split wvb-bytes=%s verification=pending-current-native\n' \
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
echo 'PASS  language 1 front door phase=compiler-slice item=4/13'

echo 'START language 1 front door phase=fixed-integers item=5/13'
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
node "$script_directory/Verify-Language-1.0-Memory-Budget-Entry.mjs" \
    "$work/Verifier.elf" "$work/Memory-Budget-Entry-A.wvb" \
    "$work/Memory-Budget-Entry-Malformed" || exit $?
echo 'PASS  language 1 front door step=memory-budget-entry item=verification valid=1 malformed=9'
"$work/Verifier.elf" "$work/Enum-I32-Negative-Main.wvb" \
    >"$work/Verify-Enum-I32.out" \
    2>"$work/Verify-Enum-I32.err" || exit $?
[[ ! -s $work/Verify-Enum-I32.err ]] || exit 1
grep -Fq 'wvb status=Valid profile=compiler-aligned' \
    "$work/Verify-Enum-I32.out" || exit 1
echo 'PASS  language 1 front door step=enum-backing-verifier cases=1 verification=current-native'
if ! "$work/Verifier.elf" "$work/Generic-Wir-A.wvb" \
    >"$work/Verify-Generic-Wir.out" \
    2>"$work/Verify-Generic-Wir.err"; then
    cat -- "$work/Verify-Generic-Wir.out" >&2
    cat -- "$work/Verify-Generic-Wir.err" >&2
    exit 1
fi
if [[ -s $work/Verify-Generic-Wir.err ]]; then
    cat -- "$work/Verify-Generic-Wir.err" >&2
    exit 1
fi
grep -Fq 'wvb status=Valid profile=compiler-aligned' \
    "$work/Verify-Generic-Wir.out" || exit 1
printf 'PASS  language 1 front door step=generic-wir-split wvb-bytes=%s verification=current-native\n' \
    "$generic_wir_wvb_bytes"
"$work/Verifier.elf" "$work/Generic-Nominal-Variant.wvb" \
    >"$work/Verify-Generic-Nominal-Variant.out" \
    2>"$work/Verify-Generic-Nominal-Variant.err" || exit $?
[[ ! -s $work/Verify-Generic-Nominal-Variant.err ]] || exit 1
grep -Fq 'wvb status=Valid profile=compiler-aligned' \
    "$work/Verify-Generic-Nominal-Variant.out" || exit 1
node "$script_directory/Verify-Generic-Nominal-Variant.mjs" \
    "$work/Generic-Nominal-Variant.wvss" \
    "$work/Generic-Nominal-Variant.wvca" \
    "$work/Generic-Nominal-Variant.wvlb" \
    "$work/Generic-Nominal-Variant.wvir" \
    "$work/Generic-Nominal-Variant.wvb" || exit $?
echo 'PASS  language 1 front door step=generic-nominal-variant verification=current-native cases=94'
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
echo 'PASS  language 1 front door phase=fixed-integers item=5/13'

echo 'START language 1 front door phase=runes item=6/13'
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
echo 'PASS  language 1 front door phase=runes item=6/13'

echo 'START language 1 front door phase=floating item=7/13'
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

node "$script_directory/Build-Cached-Split-Project-Wvb.mjs" \
    "$repository_root/Projects/Tools/Windvale-Wvb-Runner.wvproj" \
    "$work/Floating-Runner.wvb" \
    "$work/Analyzer.elf" "$work/Analyzer.identity" \
    "$work/Emitter.elf" "$work/Emitter.identity" >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 5 \
    "$work/Floating-Runner.wvb" "$work/Floating-Runner.elf" linux \
    >/dev/null || exit $?
"$work/Floating-Runner.elf" "$work/Memory-Budget-Entry-A.wvb" \
    >"$work/Memory-Budget-Entry-Run.out" \
    2>"$work/Memory-Budget-Entry-Run.err" || exit $?
[[ ! -s $work/Memory-Budget-Entry-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Memory-Budget-Entry.out"
cmp -s -- "$work/Expected-Memory-Budget-Entry.out" \
    "$work/Memory-Budget-Entry-Run.out" || exit 1
echo 'PASS  language 1 front door step=memory-budget-entry item=runtime transfer=launcher-to-main release=deterministic result=42'
node "$script_directory/Verify-Language-1.0-U8-Enums.mjs" \
    "$work/Verifier.elf" "$work/Floating-Runner.elf" \
    "$work/Enum-U8-Used-Main-A.wvb" \
    "$work/Enum-U8-Malformed" || exit $?
echo 'PASS  language 1 front door step=enum-u8 valid=1 malformed=9 version=1.22 result=42'
"$work/Floating-Runner.elf" "$work/Floating-A.wvb" \
    >"$work/Floating-Run.out" 2>"$work/Floating-Run.err" || exit $?
[[ ! -s $work/Floating-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Floating.out"
cmp -s -- "$work/Expected-Floating.out" "$work/Floating-Run.out" || exit 1

"$work/Floating-Runner.elf" "$work/Generic-Nominal-Variant.wvb" \
    >"$work/Generic-Nominal-Variant-Run.out" \
    2>"$work/Generic-Nominal-Variant-Run.err" || exit $?
[[ ! -s $work/Generic-Nominal-Variant-Run.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Generic-Nominal-Variant.out"
cmp -s -- "$work/Expected-Generic-Nominal-Variant.out" \
    "$work/Generic-Nominal-Variant-Run.out" || exit 1
echo 'PASS  language 1 front door step=generic-nominal-variant cases=97 verification=current-native execution=42 rejections=3'

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
echo 'PASS  language 1 front door phase=floating item=7/13'

echo 'START language 1 front door phase=fixed-arrays item=8/13'
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Array-Main-Pipeline.wv" \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" \
    "$work/Fixed-Array-A.wvb" \
    >"$work/Fixed-Array-A.out" 2>"$work/Fixed-Array-A.err" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Array-Main-Pipeline.wv" \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" \
    "$work/Fixed-Array-B.wvb" \
    >"$work/Fixed-Array-B.out" 2>"$work/Fixed-Array-B.err" || exit $?
[[ ! -s $work/Fixed-Array-A.err && ! -s $work/Fixed-Array-B.err ]] || exit 1
cmp -s -- "$work/Fixed-Array-A.out" "$work/Fixed-Array-B.out" || exit 1
cmp -s -- "$work/Fixed-Array-A.wvb" "$work/Fixed-Array-B.wvb" || exit 1
node "$script_directory/Verify-Language-1.0-Fixed-Arrays.mjs" \
    "$work/Verifier.elf" "$work/Floating-Runner.elf" \
    "$work/Fixed-Array-A.wvb" "$work" || exit $?
fixed_array_wvb_bytes=$(wc -c < "$work/Fixed-Array-A.wvb")
printf 'INFO  language 1 fixed-array wvb-bytes=%s\n' "$fixed_array_wvb_bytes"
echo 'PASS  language 1 front door phase=fixed-arrays item=8/13'

echo 'START language 1 front door phase=vector-sequence-types item=9/13'
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Vector-Sequence-Wvb-Types.wv" \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" \
    "$work/Vector-Sequence-Types.wvb" \
    >"$work/Vector-Sequence-Types.out" \
    2>"$work/Vector-Sequence-Types.err" || exit $?
[[ ! -s $work/Vector-Sequence-Types.err ]] || exit 1
node "$script_directory/Verify-Language-1.0-Vector-Sequence-Types.mjs" \
    "$work/Verifier.elf" "$work/Floating-Runner.elf" \
    "$work/Vector-Sequence-Types.wvb" "$work" || exit $?
node "$script_directory/Verify-Language-1.0-Vector-Sequence-Runtime.mjs" \
    "$work/Verifier.elf" "$work/Floating-Runner.elf" \
    "$work/Vector-Sequence-Types.wvb" "$work" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Sequence-Read-Main-Pipeline.wv" \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" \
    "$work/Sequence-Read.wvb" \
    >"$work/Sequence-Read.out" 2>"$work/Sequence-Read.err" || exit $?
[[ ! -s $work/Sequence-Read.err ]] || exit 1
node "$script_directory/Verify-Language-1.0-Sequence-Reads.mjs" \
    "$work/Verifier.elf" "$work/Floating-Runner.elf" \
    "$work/Sequence-Read.wvb" "$work" || exit $?
node "$script_directory/Run-Split-Compiler.mjs" "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Vector-Read-Freeze-Main-Pipeline.wv" \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" \
    "$work/Vector-Read-Freeze.wvb" \
    >"$work/Vector-Read-Freeze.out" 2>"$work/Vector-Read-Freeze.err" || exit $?
[[ ! -s $work/Vector-Read-Freeze.err ]] || exit 1
node "$script_directory/Verify-Language-1.0-Vector-Reads-Freeze.mjs" \
    "$work/Verifier.elf" "$work/Floating-Runner.elf" \
    "$work/Vector-Read-Freeze.wvb" "$work" || exit $?
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Sequence-Read-Wrong-Owner.wv" \
    Sequence-Read-Wrong-Owner Invalidˉcollection \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" || exit $?
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Sequence-Read-Wrong-Index.wv" \
    Sequence-Read-Wrong-Index Invalidˉargument \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" || exit $?
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Sequence-Read-Unsupported-Element.wv" \
    Sequence-Read-Unsupported-Element Invalidˉcollection \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" || exit $?
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Sequence-Read-Lookalike.wv" \
    Sequence-Read-Lookalike Invalidˉargument \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" \
    "$repository_root/Tests/Fixtures/Language-1.0/Sequence-Read-Lookalike-Module.wv" || exit $?
if node "$script_directory/Run-Split-Compiler.mjs" \
    "$work/Admitter.elf" "$work/Analyzer.elf" "$work/Emitter.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Vector-Freeze-Use-After.wv" \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" \
    "$work/Vector-Freeze-Use-After.wvb" \
    >"$work/Vector-Freeze-Use-After.out" \
    2>"$work/Vector-Freeze-Use-After.err"; then
    exit 1
fi
[[ ! -e $work/Vector-Freeze-Use-After.wvb ]] || exit 1
[[ $(<"$work/Vector-Freeze-Use-After.err") == \
    'source emission status=Invalidˉanalysis analysis-status=Invalidˉwir wvb-status=Sourceˉwir function=0 operation=0 source-line=0' ]] || exit 1
echo 'PASS  language 1 front door step=vector-reads-freeze-rejections item=1/8 case=use-after'
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Vector-Freeze-Wrong-Borrow.wv" \
    Vector-Freeze-Wrong-Borrow Invalidˉborrow \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" || exit $?
echo 'PASS  language 1 front door step=vector-reads-freeze-rejections item=2/8 case=wrong-borrow'
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Vector-Read-Parameter.wv" \
    Vector-Read-Parameter Invalidˉcollection \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" || exit $?
echo 'PASS  language 1 front door step=vector-reads-freeze-rejections item=3/8 case=parameter'
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Vector-Read-Unsupported-Element.wv" \
    Vector-Read-Unsupported-Element Invalidˉcollection \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" || exit $?
echo 'PASS  language 1 front door step=vector-reads-freeze-rejections item=4/8 case=unsupported-element'
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Vector-Read-Wrong-Borrow.wv" \
    Vector-Read-Wrong-Borrow Invalidˉborrow \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" || exit $?
echo 'PASS  language 1 front door step=vector-reads-freeze-rejections item=5/8 case=read-wrong-borrow'
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Vector-Freeze-Inferred-Result.wv" \
    Vector-Freeze-Inferred-Result Invalidˉcollection \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" || exit $?
echo 'PASS  language 1 front door step=vector-reads-freeze-rejections item=6/8 case=inferred-result'
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Vector-Freeze-Mismatched-Result.wv" \
    Vector-Freeze-Mismatched-Result Invalidˉcollection \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" || exit $?
echo 'PASS  language 1 front door step=vector-reads-freeze-rejections item=7/8 case=mismatched-result'
expect_profiled_analysis_failure_with_dependencies \
    "$repository_root/Tests/Fixtures/Language-1.0/Vector-Freeze-Mismatched-Argument.wv" \
    Vector-Freeze-Mismatched-Argument Invalidˉcollection \
    "$repository_root/Libraries/Foundation/Collections/Collections.wv" || exit $?
echo 'PASS  language 1 front door step=vector-reads-freeze-rejections item=8/8 case=mismatched-argument'
vector_sequence_types_wvb_bytes=$(wc -c < "$work/Vector-Sequence-Types.wvb")
sequence_read_wvb_bytes=$(wc -c < "$work/Sequence-Read.wvb")
vector_read_freeze_wvb_bytes=$(wc -c < "$work/Vector-Read-Freeze.wvb")
printf 'INFO  language 1 vector-sequence types wvb-bytes=%s\n' \
    "$vector_sequence_types_wvb_bytes"
printf 'INFO  language 1 sequence reads wvb-bytes=%s cases=10\n' \
    "$sequence_read_wvb_bytes"
printf 'INFO  language 1 vector reads and freeze wvb-bytes=%s cases=19\n' \
    "$vector_read_freeze_wvb_bytes"
echo 'PASS  language 1 front door phase=vector-sequence-types item=9/13'

echo 'START language 1 front door phase=unit-never item=10/13'
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
echo 'PASS  language 1 front door phase=unit-never item=10/13'

echo 'START language 1 front door phase=multi-field-variants item=11/13'
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
echo 'PASS  language 1 front door phase=multi-field-variants item=11/13'

echo 'START language 1 front door phase=typed-failure item=12/13'
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
echo 'PASS  language 1 front door phase=typed-failure item=12/13'

echo 'START language 1 front door phase=foundation-generics item=13/13'
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

for name in Result-Wrong-Arity Result-Extra-Argument Result-Bare Result-Inferred-Construction Try-Wrong-Error; do
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
echo 'PASS  language 1 front door phase=foundation-generics item=13/13'
printf 'native language 1 front door status=Passed cases=449 frozen-inputs=251 source-fixtures=101 descriptor-cases=33 profile-cases=4 value-front-end-cases=39 generic-front-end-cases=4 generic-resolution-cases=1 generic-type-catalog-cases=1 generic-specialization-cases=4 generic-wir-cases=4 generic-nominal-pipeline-cases=26 generic-nominal-function-body-cases=33 generic-nominal-declaration-dependency-cases=33 generic-nominal-variant-cases=97 compiler-cases=36 enum-cases=20 borrow-cases=14 memory-budget-entry-cases=12 fixed-integer-cases=22 rune-cases=20 floating-cases=27 fixed-array-cases=6 vector-sequence-type-cases=6 vector-sequence-runtime-cases=12 sequence-read-cases=10 vector-read-freeze-cases=19 unit-never-cases=21 multi-field-variant-cases=25 typed-failure-cases=5 foundation-generic-cases=6 compiler-result=42 compiler-wvb-bytes=221 memory-budget-entry-wvb-bytes=%s enum-dead-type-wvb-bytes=%s enum-u8-wvb-bytes=%s generic-wir-wvb-bytes=%s generic-type-catalog-wvb-bytes=%s generic-nominal-variant-wvb-bytes=%s value-if-wvb-bytes=%s value-match-wvb-bytes=%s value-match-never-wvb-bytes=%s unit-wvb-bytes=%s never-wvb-bytes=%s record-update-wvb-bytes=1116 enum-i32-wvb-bytes=%s fixed-integer-wvb-bytes=5335 rune-wvb-bytes=%s floating-wvb-bytes=%s fixed-array-wvb-bytes=%s vector-sequence-type-wvb-bytes=%s vector-sequence-runtime-wvb-bytes=1156 sequence-read-wvb-bytes=%s vector-read-freeze-wvb-bytes=%s multi-field-variant-wvb-bytes=%s typed-failure-wvb-bytes=%s foundation-generic-wvb-bytes=%s generic-specializations-wvb-bytes=%s\n' "$memory_budget_entry_wvb_bytes" "$enum_dead_type_wvb_bytes" "$enum_u8_wvb_bytes" "$generic_wir_wvb_bytes" "$generic_type_catalog_wvb_bytes" "$generic_nominal_variant_wvb_bytes" "$value_if_wvb_bytes" "$value_match_wvb_bytes" "$value_match_never_wvb_bytes" "$unit_wvb_bytes" "$never_wvb_bytes" "$enum_i32_wvb_bytes" "$rune_wvb_bytes" "$floating_wvb_bytes" "$fixed_array_wvb_bytes" "$vector_sequence_types_wvb_bytes" "$sequence_read_wvb_bytes" "$vector_read_freeze_wvb_bytes" "$multi_field_variant_wvb_bytes" "$result_try_wvb_bytes" "$foundation_generic_wvb_bytes" "$generic_specializations_wvb_bytes"
