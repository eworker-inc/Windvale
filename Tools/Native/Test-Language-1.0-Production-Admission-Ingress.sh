#!/usr/bin/env sh
set -eu

mode=''
if [ "$#" -ne 0 ]; then
    if [ "$#" -ne 1 ] || [ "$1" != '--project4-launcher' ]; then
        echo 'Usage: ./Tools/Native/Test-Language-1.0-Production-Admission-Ingress.sh [--project4-launcher]' >&2
        exit 64
    fi
    mode='--project4-launcher'
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
if [ -n "$mode" ]; then
    exec node "$script_directory/Test-Language-1.0-Production-Admission-Ingress.mjs" "$mode"
    exit 64
fi

exec node "$script_directory/Test-Language-1.0-Production-Admission-Ingress.mjs"
