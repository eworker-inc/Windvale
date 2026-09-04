#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Wvb-Unsafe-Rejections.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wvb-unsafe-rejections.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wvb-unsafe-rejections.*)
            rm -f -- \
                "$temporary_directory/Input.wvb" \
                "$temporary_directory/Run.out" \
                "$temporary_directory/Run.err" \
                "$temporary_directory/Decode.err"
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

check_hash() {
    local path=$1
    local digest=$2
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    (cd -- "$directory" && printf '%s  %s\n' \
        "$digest" "$(basename -- "$path")" | sha256sum --check --strict --quiet)
}

total=0
passed=0
input="$temporary_directory/Input.wvb"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"

run_launcher() {
    local name=$1
    local launcher=$2
    local input_digest=$3
    local report_digest=$4
    "$repository_root/Tools/Native/$launcher" \
        "$input" > "$run_output" 2> "$run_error"
    local run_status=$?
    if ((run_status != 1)); then
        echo "FAIL  $name: native WVB read-only exit differs" >&2
        return 1
    fi
    if [[ -s $run_output ]]; then
        echo "FAIL  $name: rejected WVB wrote standard output" >&2
        cat -- "$run_output" >&2
        return 1
    fi
    if ! check_hash "$run_error" "$report_digest"; then
        echo "FAIL  $name: native WVB report differs" >&2
        cat -- "$run_error" >&2
        return 1
    fi
    if ! check_hash "$input" "$input_digest"; then
        echo "FAIL  $name: native WVB read-only command changed its input" >&2
        return 1
    fi
    rm -f -- "$run_output" "$run_error"
}

run_case() {
    local name=$1
    local fixture=$2
    local input_digest=$3
    local report_digest=$4
    total=$((total + 1))
    if ! base64 --decode "$repository_root/$fixture" > "$input" 2> "$decode_error"; then
        echo "FAIL  $name: WVB fixture could not be decoded" >&2
        return 1
    fi
    if [[ -s $decode_error ]]; then
        echo "FAIL  $name: WVB decoder wrote a diagnostic" >&2
        cat -- "$decode_error" >&2
        return 1
    fi
    if ! check_hash "$input" "$input_digest"; then
        echo "FAIL  $name: WVB input identity differs" >&2
        return 1
    fi
    run_launcher "$name" Verify-Wvb.sh "$input_digest" "$report_digest" || return 1
    run_launcher "$name" Inspect-Wvb.sh "$input_digest" "$report_digest" || return 1
    rm -f -- "$input" "$decode_error"
    passed=$((passed + 1))
    echo "PASS  $name"
}

run_or_fail() {
    run_case "$@" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
}

structure_report=3c619f27145a7ce7dff62b303e739ac145df640662b6f58666ebafebdf267a79
types_report=3e3e68daeb03b21644951817cf4a256ec0c958e3add6f84725bca7c38461047a
functions_report=54662bfb7b2473a8608601b5cdd3e5576e6452d0e3a6e06b567b95c304047752
code_exports_report=6db319329296181174ae9e22df7be3b0be781ff3f98e4a81cf8cb532f8aec96b
typed_report=c083d8e4a7dbe48f3c72248285d6f4ace645202ca8f2013df1d51ab328db7930
fixture_root=Tests/Native/Wvb-Unsafe-Rejections
run_or_fail unknown-opcode "$fixture_root/Unknown-Opcode.wvb.b64" f84528a577647a8d9c988f2cf082ea642dc7b8f61220bb5d23d57e8d3238c0aa "$structure_report"
run_or_fail truncated-operand "$fixture_root/Truncated-Operand.wvb.b64" eac2a31112958af23f89941be6e9591e870438439ea037e2b12a6c23216f74d9 "$structure_report"
run_or_fail local-index "$fixture_root/Local-Index.wvb.b64" 857f94ae40c95dd2f2e3f27ba07892c0ae351f1875fc16c91695e5a3872f56a3 "$code_exports_report"
run_or_fail jump-target "$fixture_root/Jump-Target.wvb.b64" b56e962d4e4d24d6366354e1f4798c4352de236dcad421829d4b8714db3eb2a3 "$code_exports_report"
run_or_fail after-return "$fixture_root/After-Return.wvb.b64" ece563bb06b953ef1587004c3517c21098702b644511cdda989e49d89d9061e7 "$typed_report"
run_or_fail record-parameter-type "$fixture_root/Record-Parameter-Type.wvb.b64" 8e89cf9b526e1ea93d81d62425f95986daff4469dc7f113f5e38b580ccf163aa "$functions_report"
run_or_fail record-field-index "$fixture_root/Record-Field-Index.wvb.b64" 1d5ed90586e2327af309cb9fe6ba1110da879ee461f7fd56d7c5414d1c637999 "$typed_report"
run_or_fail duplicate-record-field "$fixture_root/Duplicate-Record-Field.wvb.b64" 73867dcf74f30f4b9237091aa59ea981200f4139636b67eb730bdb71752571b6 "$types_report"
run_or_fail mismatched-enum-comparison "$fixture_root/Mismatched-Enum-Comparison.wvb.b64" 6ae2e65a43f68f0aa4b46b7ca306ad1dd06b72b1328e02e611f98e9f7abc869e "$typed_report"
run_or_fail duplicate-nominal-name "$fixture_root/Duplicate-Nominal-Name.wvb.b64" 60d12d56015678f3197a1413cfb058bff64188a8e2256d09f504280fad805f9c "$types_report"
run_or_fail mismatched-merge "$fixture_root/Mismatched-Merge.wvb.b64" f3f98931b5a701c805e9889768abe2c8536fb4ff04fd6a614ddf7f0732f6b7a2 "$typed_report"
run_or_fail bytes-length-on-i32 "$fixture_root/Bytes-Length-On-I32.wvb.b64" f06d084a5f78b8d12e8503cfacd841565527c7a075dbcad40626e48f6d9e48c0 "$typed_report"
run_or_fail record-create-wrong-field-type "$fixture_root/Record-Create-Wrong-Field-Type.wvb.b64" a074c6a8229870bb45a3de8764a2ffd51b8091f0e4d50f48330c560927ca4c59 "$typed_report"
run_or_fail invalid-enum-member "$fixture_root/Invalid-Enum-Member.wvb.b64" ddd000954aeb8d0c02775128ae52615d9bf4237bda9741eb39e6f9efb4f2ddbe "$code_exports_report"
run_or_fail enum-const-on-record "$fixture_root/Enum-Const-On-Record.wvb.b64" 3d09445c44bf2d1e3f5b811f254e0bccc902366ad242ea4cf101fc44f23b99d8 "$code_exports_report"
run_or_fail duplicate-enum-value "$fixture_root/Duplicate-Enum-Value.wvb.b64" da453ca0cbe661ab695e21ce8f2ee2530a303ad996bbedfe6f0ae5e9bbb0a00c "$types_report"
run_or_fail stack-capacity "$fixture_root/Stack-Capacity.wvb.b64" ba69564377f6e9b2ded8b9c6125205654eaf22cb4015be535015de33af23c728 "$typed_report"
run_or_fail record-field-on-primitive "$fixture_root/Record-Field-On-Primitive.wvb.b64" d5deb4c26a19234066db169a40e5a2eaac99a4e03a4f0d08b816485431ca3396 "$typed_report"
run_or_fail enum-name-on-primitive "$fixture_root/Enum-Name-On-Primitive.wvb.b64" 155d619ae7732c705b7881693ba1e6f1cd7db3cbbe2e8a5687fbd27e60097405 "$typed_report"
run_or_fail wrong-nominal-kind "$fixture_root/Wrong-Nominal-Kind.wvb.b64" da375377c69ca8c87fe17f34460617330fdcc1763e1a465de4805e1ead98cc93 "$functions_report"

echo "Tests: $total, Passed: $passed, Failed: 0"
