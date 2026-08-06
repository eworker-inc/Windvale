#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Wvo-Read-Only-Rejections.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wvo-read-only-rejections.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wvo-read-only-rejections.*)
            rm -f -- \
                "$temporary_directory/Input.wvo" \
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
input="$temporary_directory/Input.wvo"
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
    if ((run_status != 2)); then
        echo "FAIL  $name: native WVO read-only exit differs" >&2
        return 1
    fi
    if [[ -s $run_output ]]; then
        echo "FAIL  $name: rejected WVO wrote standard output" >&2
        cat -- "$run_output" >&2
        return 1
    fi
    if ! check_hash "$run_error" "$report_digest"; then
        echo "FAIL  $name: native WVO report differs" >&2
        cat -- "$run_error" >&2
        return 1
    fi
    if ! check_hash "$input" "$input_digest"; then
        echo "FAIL  $name: native WVO read-only command changed its input" >&2
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
        echo "FAIL  $name: WVO fixture could not be decoded" >&2
        return 1
    fi
    if [[ -s $decode_error ]]; then
        echo "FAIL  $name: WVO decoder wrote a diagnostic" >&2
        cat -- "$decode_error" >&2
        return 1
    fi
    if ! check_hash "$input" "$input_digest"; then
        echo "FAIL  $name: WVO input identity differs" >&2
        return 1
    fi
    run_launcher "$name" Verify-Wvo.sh "$input_digest" "$report_digest" || return 1
    run_launcher "$name" Inspect-Wvo.sh "$input_digest" "$report_digest" || return 1
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

run_or_fail short-header Tests/Native/Wvo-Rejections/Short-Header.wvo.b64 6e340b9cffb37a989ca544e6bb780a2c78901d3fb33738768511a30617afa01d 97779c19c3b55c92f53faa567de292403493fbff7180cfb6e2bade8991ef63aa
run_or_fail bad-magic Tests/Native/Wvo/Bad-Magic.wvo.b64 0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288 2e53f573d1e94159c58368c4d9ebcba284d6c13f63a286bd75264bc837a162e4
run_or_fail bad-version Tests/Native/Wvo-Rejections/Bad-Version.wvo.b64 3c724339c2a6fe6d41c07a461907e5bbee7abc95cf899b0605e77f744f0c6081 bce421b96f8ee4ce19c322eba64a71bcefa3539640b41ecca2a5cd70bab4055e
run_or_fail bad-architecture Tests/Native/Wvo-Rejections/Bad-Architecture.wvo.b64 7ff46081c9b5f3d50d0a499f74d665bb9b474e308432ddcf484079a6f434db3d 8f6a586a1323284e6aeb9522fc292b266e4368f44d5e022b87fab28632a2da97
run_or_fail unsupported-flags Tests/Native/Wvo-Rejections/Unsupported-Flags.wvo.b64 b1b581c75901f1bba0dfeb37fb888342d32c6f6eff565165277d051d7ae0f4c7 3eab07bbffa763acfd259b4e3b0b09206098c61f625bf23e202dc16fb19cc11c
run_or_fail limit-exceeded Tests/Native/Wvo-Rejections/Limit-Exceeded.wvo.b64 6e191db4e2ce6107493baed610e9d116018ae887972d94d3df5969d3d405c0a8 d502b71111e5f7557fff108bc740b558d6d15acdc5eb22ada9f8cfe2dca0a46e
run_or_fail out-of-bounds Tests/Native/Wvo/Truncated.wvo.b64 6f120ce6b833f781ab014844af535b25fe28eb2d565afa2b2f4360c7a0c99371 9b45f12022ab0ba549e6c2ffa49cb15673d96c8f58efd5d6d9c2def87097aedb
run_or_fail invalid-name Tests/Native/Wvo-Rejections/Invalid-Name.wvo.b64 2cf0c91c9e6df189f2a79214bc5b5a3690e3b0140e41eae2683efd817bf9d067 bf35958972ccf812961fd52b92b1ebeb6f5e9b7e87a77c7083064de590c548cb
run_or_fail invalid-section Tests/Native/Wvo-Rejections/Invalid-Section.wvo.b64 d0a93c19fceb58070797c893f3ba5eb3ebae60e380a85d5fd84cf037995702e8 430a541121485335be6635ec6277141489dafb4b73ec47dcfb1ddc72a32e649d
run_or_fail invalid-symbol Tests/Native/Wvo-Rejections/Invalid-Symbol.wvo.b64 9ba10fcccc2e6d4b9a9fef8343dacb1743a2c2e1f0c1795ef0b97a3b50f655a5 b3dd9e318a471bf1f8f5e589d1c119f4b89b02f69d3956f42a51bde5afc1875e
run_or_fail invalid-relocation Tests/Native/Wvo-Rejections/Invalid-Relocation.wvo.b64 b36011ba5615c228dcf6c4d389c7c50f24b25934b47d01bbbc701c9bf02b2736 b6b147e8141a3de78ab59b3af4d04081c37d77b8124ccdbefffc94645ab18995
run_or_fail noncanonical-order Tests/Native/Wvo-Rejections/Noncanonical-Order.wvo.b64 443499e89326160f6172be9dd0be918935373e1c862d2192570cc922471026a7 2012a1501f7861708c992f61dfe308bc8ef217781b5e92bd2ca67fc56d6e31d8
run_or_fail trailing-bytes Tests/Native/Wvo/Trailing.wvo.b64 3ca5e84240e8f12be84fdb957df37f8162e74415417cd7009f92698e683ee981 3cdcb2fa62f4fc698e9624e68dc10dbf95e7363cf0332b280066083cc1783711

echo "Tests: $total, Passed: $passed, Failed: 0"
