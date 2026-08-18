# Simplified Chinese source equivalence oracle

## Purpose

This oracle separates source-language localization from project-name
translation. The paired files use identical Chinese project-owned identifiers:

- module `选项ˉ选择`;
- import alias `选项库`;
- function `选择ˉ启用`; and
- parameter `已启用`.

Only the descriptor-selected keyword spellings and imported public-library
labels differ. Therefore a compiler can compare canonical token and declaration
streams without pretending that it translates arbitrary project vocabulary.

## Profile-dependent mappings

| `en@1` bytes | `zh-Hans@1` bytes | Canonical identity |
| --- | --- | --- |
| `module` | `模块` | `KW_MODULE` |
| `profile` | `配置` | `KW_PROFILE` |
| `core` | `核心` | `KW_CORE` |
| `platform` | `平台` | `KW_PLATFORM` |
| `authority` | `权限` | `KW_AUTHORITY` |
| `application` | `应用` | `KW_APPLICATION` |
| `import` | `导入` | `KW_IMPORT` |
| `as` | `作为` | `KW_AS` |
| `export` | `导出` | `KW_EXPORT` |
| `fn` | `函数` | `KW_FN` |
| `bool` | `布尔` | `KW_BOOL` |
| `effects` | `效应` | `KW_EFFECTS` |
| `if` | `如果` | `KW_IF` |
| `return` | `返回` | `KW_RETURN` |
| `true` | `真` | `KW_TRUE` |
| `Foundationˉoption` | `基础库ˉ可选值` | canonical module `Foundationˉoption` |
| `Option` | `可选值` | canonical declaration `Foundationˉoption.Option` |
| `Present` | `有值` | canonical case `Foundationˉoption.Option.Present` |
| `Absent` | `无值` | canonical case `Foundationˉoption.Option.Absent` |
| `Value` | `值` | canonical parameter `Foundationˉoption.Option.Present.Value` |

Registered platform IDs `windows`, `linux`, and `windvale`, punctuation, and the
project-owned identifiers are exact equal inputs rather than mappings.

## Expected canonical source projection

After descriptor/profile admission, keyword lowering, and imported-label
resolution, both files project to the following diagnostic representation:

~~~text
module 选项ˉ选择;
profile core;
platform windows, linux, windvale;
authority application;

import Foundationˉoption as 选项库;

export fn 选择ˉ启用(已启用: bool) -> 选项库.Option<bool> effects() {
    if 已启用 {
        return 选项库.Option.Present { Value: true };
    }
    return 选项库.Option.Absent;
}
~~~

This projection is an inspection oracle, not stored replacement source. Raw
source hashes, descriptor spans, profile hashes, lexicon hashes, catalog hashes,
and localized token spans remain distinct.

## Required future executable comparison

The first implementation must compare, in order:

1. canonical token ID stream excluding descriptor bytes;
2. project-owned identifier byte stream;
3. canonical imported module/declaration/case/parameter identity stream;
4. parsed AST after removing source spans and profile provenance;
5. typed semantic model after removing source spans and profile provenance;
6. WIR semantic sections;
7. WVB semantic sections;
8. native object semantic sections; and
9. linked executable semantic sections and observed result.

Items 1 through 3 have an exact paper oracle in this bundle. Items 4 through 9
remain implementation evidence and must not be reported as passed yet.

## Permitted differences

- raw source bytes and raw source SHA-256;
- descriptor profile identity/version;
- lexicon, vocabulary, catalog, and composite-profile hashes;
- source byte offsets and byte lengths for corresponding tokens;
- localized diagnostic presentation; and
- optional debug records that deliberately preserve original source spelling.

Any semantic type, ownership, effect, capability, control-flow, constant,
canonical declaration, WIR, WVB, object, executable, or runtime-result difference
is a failed equivalence case.
