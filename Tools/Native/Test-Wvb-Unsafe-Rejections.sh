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

semantic_report=4938c6906dfb318e36b11a2795699638f4cdef1bef12c477b5b726137cd3d9b5
typed_report=c083d8e4a7dbe48f3c72248285d6f4ace645202ca8f2013df1d51ab328db7930
fixture_root=Tests/Native/Wvb-Unsafe-Rejections
run_or_fail unknown-opcode "$fixture_root/Unknown-Opcode.wvb.b64" f84528a577647a8d9c988f2cf082ea642dc7b8f61220bb5d23d57e8d3238c0aa "$semantic_report"
run_or_fail truncated-operand "$fixture_root/Truncated-Operand.wvb.b64" eac2a31112958af23f89941be6e9591e870438439ea037e2b12a6c23216f74d9 "$semantic_report"
run_or_fail local-index "$fixture_root/Local-Index.wvb.b64" 857f94ae40c95dd2f2e3f27ba07892c0ae351f1875fc16c91695e5a3872f56a3 "$semantic_report"
run_or_fail jump-target "$fixture_root/Jump-Target.wvb.b64" b56e962d4e4d24d6366354e1f4798c4352de236dcad421829d4b8714db3eb2a3 "$semantic_report"
run_or_fail after-return "$fixture_root/After-Return.wvb.b64" ece563bb06b953ef1587004c3517c21098702b644511cdda989e49d89d9061e7 "$typed_report"
run_or_fail record-parameter-type "$fixture_root/Record-Parameter-Type.wvb.b64" 8e89cf9b526e1ea93d81d62425f95986daff4469dc7f113f5e38b580ccf163aa "$semantic_report"
run_or_fail record-field-index "$fixture_root/Record-Field-Index.wvb.b64" 1d5ed90586e2327af309cb9fe6ba1110da879ee461f7fd56d7c5414d1c637999 "$typed_report"
run_or_fail duplicate-record-field "$fixture_root/Duplicate-Record-Field.wvb.b64" 73867dcf74f30f4b9237091aa59ea981200f4139636b67eb730bdb71752571b6 "$semantic_report"
run_or_fail mismatched-enum-comparison "$fixture_root/Mismatched-Enum-Comparison.wvb.b64" 6ae2e65a43f68f0aa4b46b7ca306ad1dd06b72b1328e02e611f98e9f7abc869e "$typed_report"
run_or_fail duplicate-nominal-name "$fixture_root/Duplicate-Nominal-Name.wvb.b64" 60d12d56015678f3197a1413cfb058bff64188a8e2256d09f504280fad805f9c "$semantic_report"

echo "Tests: $total, Passed: $passed, Failed: 0"
