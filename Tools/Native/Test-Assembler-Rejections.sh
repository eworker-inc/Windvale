#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Assembler-Rejections.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-assembler-rejections.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-assembler-rejections.*)
            rm -f -- \
                "$temporary_directory/Oversized.wva" \
                "$temporary_directory/Sentinel.wvo" \
                "$temporary_directory/Destination.wvo" \
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
oversized="$temporary_directory/Oversized.wva"
sentinel="$temporary_directory/Sentinel.wvo"
destination="$temporary_directory/Destination.wvo"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
sentinel_digest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5

if ! base64 --decode \
    "$repository_root/Tests/Native/Wvo/Return-42.wvo.b64" \
    > "$sentinel" 2> "$decode_error"; then
    echo 'The native assembler destination sentinel could not be decoded.' >&2
    exit 1
fi
if [[ -s $decode_error ]]; then
    echo 'The native assembler destination decoder wrote a diagnostic.' >&2
    cat -- "$decode_error" >&2
    exit 1
fi
if ! check_hash "$sentinel" "$sentinel_digest"; then
    echo 'The native assembler destination sentinel identity differs.' >&2
    exit 1
fi

truncate -s 1048577 "$oversized" || {
    echo 'The native assembler oversized fixture could not be created.' >&2
    exit 1
}
if ! check_hash \
    "$oversized" \
    '2cb74edba754a81d121c9db6833704a8e7d417e5b13d1a19f4a52f007d644264'; then
    echo 'The native assembler oversized fixture identity differs.' >&2
    exit 1
fi

run_case() {
    local name=$1
    local input=$2
    local input_digest=$3
    local report_digest=$4
    total=$((total + 1))
    if ! check_hash "$input" "$input_digest"; then
        echo "FAIL  $name: native assembler input identity differs" >&2
        return 1
    fi
    cp -- "$sentinel" "$destination" || return 1
    "$repository_root/Tools/Native/Assemble-Wva.sh" \
        "$input" "$destination" > "$run_output" 2> "$run_error"
    local run_status=$?
    if ((run_status != 2)); then
        echo "FAIL  $name: native assembler exit differs" >&2
        return 1
    fi
    if [[ -s $run_output ]]; then
        echo "FAIL  $name: rejected assembly wrote standard output" >&2
        cat -- "$run_output" >&2
        return 1
    fi
    if ! check_hash "$run_error" "$report_digest"; then
        echo "FAIL  $name: native assembler report differs" >&2
        cat -- "$run_error" >&2
        return 1
    fi
    if ! check_hash "$destination" "$sentinel_digest"; then
        echo "FAIL  $name: rejected assembly changed the destination" >&2
        return 1
    fi
    rm -f -- "$destination" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name"
}

run_or_fail() {
    run_case "$@" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
}

fixture_root="$repository_root/Tests/Native/Wva-Rejections"
run_or_fail wva1001 "$fixture_root/Bad-Header.wva" a0c401f0ff8df946469bc46a2a8e6aeeea17ac1335267d377c5636f2ada31376 4cfa4a4e82f3f03d8447865354e4c6f4d433680dadf3ce5c074e708c79a4de31
run_or_fail wva1002 "$fixture_root/Late-Symbol.wva" e80f74ddb1daa2e52b731d70f01c2bef21910b70a5a5b3a83baafbf290bb35dd 8642b0a6d4d2ac84a8e5be5d8d6009bdbc945082c954c5a8e15359494c212d58
run_or_fail wva1003 "$fixture_root/Short-Symbol.wva" 05db14bde97f50b4373bac9d1d4432aceb84d67cd040f059a6ff275ace41de88 b627119175c5b48c0ea1e7ad8566e61df57c467ebe2de91d92cf456131f8a53a
run_or_fail wva1004 "$fixture_root/Bad-Machine-Name.wva" 13dcbcc9a1882d238c220f5ce91a9407e86e5ab558b2742a5454accd596cf694 909f464d645ede6ec49f119c933573638eff80525d1b1c49738b90c96cfcc27c
run_or_fail wva1005 "$fixture_root/Bad-Alignment.wva" f7e2b5e7adc5e782289ba6d9e5f2f1505d7352ea2d79e8ce30af44d677633bdc d46c03051b79c8af12274df50737cc7963b77a6aa4404282558c052b07e94b65
run_or_fail wva1006 "$fixture_root/Noncanonical-Symbols.wva" ddce08026e091ef40e43f770557e510660c99a3beba4eea4d840d66c5616c9e8 9c2270a866c3383ea43020bea7693d8d0ae87aae06fe86d46a35d30146e1a4ec
run_or_fail wva1007 "$fixture_root/Wrong-Symbol-Section.wva" 490ee170d0899f724f2c51c326ebfb6b90b540d95f4663813d20ef2969fae9ac 440fb3e5eaf8153ee771d926274393e58a4689a14824c5a1846317bf819053d1
run_or_fail wva1008 "$fixture_root/Wrong-Statement-Section.wva" e10d6cfc9568bfb13cc3281953eff68d9fc988521b5ca726adff59eb8e63a267 9715af284f22626fd002ea4465185bfecf609be0fe378febbb93765d9736344a
run_or_fail wva1009 "$fixture_root/Missing-Call-Target.wva" d5f86e0b5c975edaff2b82bfd7b48c5f8fcb1fd7a0ac49ea2601ca41a4f7d1ec d0d7a09622b8cc73cf2a4b87863f1b0cfe7c20f3da343f01764092472f6f1fd8
run_or_fail wva1010 "$fixture_root/Unclosed-Definition.wva" bfefa2b17caad9c1966854ff0f23dc0e73647db9ad8cbea9c3f1c882002c6030 ce6ea19735ebbbfa18725b7b600c12be4a240e68b7f6e5aec061722278969af4
run_or_fail wva1011 "$oversized" 2cb74edba754a81d121c9db6833704a8e7d417e5b13d1a19f4a52f007d644264 0637a77d191b3e749c5779bcd069859f330314be167647d6db05bb96eb8d483c

echo "Tests: $total, Passed: $passed, Failed: 0"
