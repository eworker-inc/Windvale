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

echo 'START language 1 front door phase=frozen-fixtures item=1/9'
node "$script_directory/Verify-Language-1.0-Migration-Fixtures.mjs" || exit $?
echo 'PASS  language 1 front door phase=frozen-fixtures item=1/9'

echo 'START language 1 front door phase=descriptor item=2/9'
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
echo 'PASS  language 1 front door phase=descriptor item=2/9'

echo 'START language 1 front door phase=value-front-end item=3/9'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Language-1-Value-Front-End.wvproj" \
    "$work/Value-Front-End.wvb" >/dev/null || exit $?
"$script_directory/Run-Wvb.sh" "$work/Value-Front-End.wvb" \
    >"$work/Value-Front-End.out" 2>"$work/Value-Front-End.err" || exit $?
[[ ! -s $work/Value-Front-End.err ]] || exit 1
printf 'Result: 42\n' >"$work/Expected-Value-Front-End.out"
cmp -s -- "$work/Expected-Value-Front-End.out" "$work/Value-Front-End.out" || exit 1
echo 'PASS  language 1 front door phase=value-front-end item=3/9'

echo 'START language 1 front door phase=compiler-slice item=4/9'
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 \
    "$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate/Wvb/Windvale-Compiler.wvb" \
    "$work/Bootstrap-Compiler.elf" --development-cache || exit $?
"$script_directory/Compile-Compiler-Source-Set.sh" \
    "$work/Bootstrap-Compiler.elf" \
    "$repository_root" "$work/Compiler.wvb" || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 1 \
    "$work/Compiler.wvb" "$work/Compiler.elf" --development-cache || exit $?
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
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Unit-Control.wv" \
    "$work/Unit-A.wvb" >"$work/Unit-A.out" 2>"$work/Unit-A.err" || exit $?
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Unit-Control.wv" \
    "$work/Unit-B.wvb" >"$work/Unit-B.out" 2>"$work/Unit-B.err" || exit $?
[[ ! -s $work/Unit-A.err && ! -s $work/Unit-B.err ]] || exit 1
cmp -s -- "$work/Unit-A.out" "$work/Unit-B.out" || exit 1
cmp -s -- "$work/Unit-A.wvb" "$work/Unit-B.wvb" || exit 1
cat -- "$work/Unit-A.out"
printf 'INFO  language 1 unit wvb-bytes=%s\n' "$(wc -c < "$work/Unit-A.wvb")"
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Record-Update.wv" \
    "$work/Record-Update-A.wvb" \
    >"$work/Record-Update-A.out" 2>"$work/Record-Update-A.err" || exit $?
"$work/Compiler.elf" \
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
    if "$work/Compiler.elf" \
        --source-input-lock "$source_lock" "$digest" \
        --source-profile "$profile" "$source" "$output" \
        >"$output.out" 2>"$output.err"; then
        return 1
    fi
    [[ ! -e $output ]]
}
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Unit-Return-Value.wv" \
    "$work/Unit-Return-Value.wvb" || exit 1
expect_rejection \
    "$repository_root/Tests/Fixtures/Language-1.0/Unit-Return-From-I32.wv" \
    "$work/Unit-Return-From-I32.wvb" || exit 1
if "$work/Compiler.elf" \
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
if "$work/Compiler.elf" \
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
echo 'PASS  language 1 front door phase=compiler-slice item=4/9'

echo 'START language 1 front door phase=fixed-integers item=5/9'
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Integer-Program.wv" \
    "$work/Fixed-Integer-A.wvb" \
    >"$work/Fixed-Integer-A.out" 2>"$work/Fixed-Integer-A.err" || exit $?
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Fixed-Integer-Program.wv" \
    "$work/Fixed-Integer-B.wvb" \
    >"$work/Fixed-Integer-B.out" 2>"$work/Fixed-Integer-B.err" || exit $?
[[ ! -s $work/Fixed-Integer-A.err && ! -s $work/Fixed-Integer-B.err ]] || exit 1
cmp -s -- "$work/Fixed-Integer-A.out" "$work/Fixed-Integer-B.out" || exit 1
cmp -s -- "$work/Fixed-Integer-A.wvb" "$work/Fixed-Integer-B.wvb" || exit 1

for name in Overflow Divide-By-Zero Invalid-Shift; do
    "$work/Compiler.elf" \
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
echo 'PASS  language 1 front door phase=fixed-integers item=5/9'

echo 'START language 1 front door phase=runes item=6/9'
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Rune-Program.wv" \
    "$work/Rune-A.wvb" \
    >"$work/Rune-A.out" 2>"$work/Rune-A.err" || exit $?
"$work/Compiler.elf" \
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
echo 'PASS  language 1 front door phase=runes item=6/9'

echo 'START language 1 front door phase=floating item=7/9'
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Floating-Program.wv" \
    "$work/Floating-A.wvb" \
    >"$work/Floating-A.out" 2>"$work/Floating-A.err" || exit $?
"$work/Compiler.elf" \
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
echo 'PASS  language 1 front door phase=floating item=7/9'

echo 'START language 1 front door phase=unit-never item=8/9'
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Never-Control.wv" \
    "$work/Never-A.wvb" \
    >"$work/Never-A.out" 2>"$work/Never-A.err" || exit $?
"$work/Compiler.elf" \
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
echo 'PASS  language 1 front door phase=unit-never item=8/9'

echo 'START language 1 front door phase=multi-field-variants item=9/9'
"$work/Compiler.elf" \
    --source-input-lock "$source_lock" "$source_lock_hash" \
    --source-profile "$source_profile" \
    "$repository_root/Tests/Fixtures/Language-1.0/Multi-Field-Variant.wv" \
    "$work/Multi-Field-Variant-A.wvb" \
    >"$work/Multi-Field-Variant-A.out" \
    2>"$work/Multi-Field-Variant-A.err" || exit $?
"$work/Compiler.elf" \
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

"$work/Compiler.elf" \
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
multi_field_variant_wvb_bytes=$(wc -c < "$work/Multi-Field-Variant-A.wvb")
printf 'INFO  language 1 multi-field-variants wvb-bytes=%s\n' \
    "$multi_field_variant_wvb_bytes"
echo 'PASS  language 1 front door phase=multi-field-variants item=9/9'
printf 'native language 1 front door status=Passed cases=117 frozen-inputs=250 source-fixtures=72 descriptor-cases=33 profile-cases=4 value-front-end-cases=23 compiler-cases=17 fixed-integer-cases=22 rune-cases=20 floating-cases=27 unit-never-cases=21 multi-field-variant-cases=21 compiler-result=42 compiler-wvb-bytes=221 unit-wvb-bytes=%s never-wvb-bytes=%s record-update-wvb-bytes=1116 fixed-integer-wvb-bytes=5335 rune-wvb-bytes=%s floating-wvb-bytes=%s multi-field-variant-wvb-bytes=%s\n' "$unit_wvb_bytes" "$never_wvb_bytes" "$rune_wvb_bytes" "$floating_wvb_bytes" "$multi_field_variant_wvb_bytes"
