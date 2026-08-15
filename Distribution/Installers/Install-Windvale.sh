#!/bin/sh

windvale_install_bootstrap() (
    set -eu

    version='0.1.0'
    target='linux-x64'
    archive_name="windvale-$version-$target.tar.gz"
    archive_sha256='4c99bda1b98156493df77b5e7b337265517c573e9ea3554fad2979315e88c11a'
    download_url="https://github.com/eworker-inc/Windvale/releases/download/v$version/$archive_name"
    temporary_parent=$(realpath -m -- "${TMPDIR:-/tmp}")
    temporary_root=$(mktemp -d "$temporary_parent/windvale-install.XXXXXX")

    cleanup() {
        case "$temporary_root" in
            "$temporary_parent"/windvale-install.*) rm -rf -- "$temporary_root" ;;
            *) echo "Refusing to remove an unexpected temporary directory: $temporary_root" >&2 ;;
        esac
    }
    trap cleanup EXIT HUP INT TERM

    archive_path=$temporary_root/$archive_name
    echo 'windvale bootstrap step=download item=1/5'
    if command -v curl >/dev/null 2>&1; then
        curl -fsSL "$download_url" -o "$archive_path"
    elif command -v wget >/dev/null 2>&1; then
        wget -qO "$archive_path" "$download_url"
    else
        echo 'Windvale installation requires curl or wget.' >&2
        exit 1
    fi

    echo 'windvale bootstrap step=verify-download item=2/5'
    printf '%s  %s\n' "$archive_sha256" "$archive_path" | sha256sum --check -

    echo 'windvale bootstrap step=extract item=3/5'
    extract_root=$temporary_root/extracted
    mkdir -- "$extract_root"
    tar -xzf "$archive_path" -C "$extract_root"
    package_root=$extract_root/windvale-$version-$target
    [ -f "$package_root/install.sh" ] || {
        echo 'The downloaded Windvale installer entry point is missing.' >&2
        exit 1
    }

    echo 'windvale bootstrap step=install item=4/5'
    "$package_root/install.sh"

    echo 'windvale bootstrap step=persist-path item=5/5'
    profile=${HOME:?HOME is required}/.profile
    path_record='export PATH="$HOME/.local/bin:$PATH"'
    if ! grep -Fqx "$path_record" "$profile" 2>/dev/null; then
        printf '\n%s\n' "$path_record" >>"$profile"
        echo "Added Windvale's per-user command directory to: $profile"
    fi
    printf '%s\n' "windvale bootstrap status=Installed version=$version target=$target root=${XDG_DATA_HOME:-$HOME/.local/share}/windvale"
)

if ! windvale_install_bootstrap; then
    unset -f windvale_install_bootstrap 2>/dev/null || true
    return 1 2>/dev/null || exit 1
fi
unset -f windvale_install_bootstrap 2>/dev/null || true

case ":${PATH:-}:" in
    *":$HOME/.local/bin:"*) ;;
    *) PATH=$HOME/.local/bin${PATH:+:$PATH}; export PATH ;;
esac

printf '%s\n' 'The current shell and future login shells can now use Windvale.'
wv version
wv doctor
