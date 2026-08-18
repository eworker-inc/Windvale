# Simplified Chinese terminology review

## Status and sources

Every entry below is an AI-authored draft requiring native technical review.
Existing Chinese technical documentation was used only as a consistency input,
not as authority for Windvale. Useful reference terminology includes Microsoft's
[C# keyword reference](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/keywords/),
[enum documentation](https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/builtin-types/enum),
[task-based asynchronous programming](https://learn.microsoft.com/zh-cn/dotnet/standard/parallel-programming/task-based-asynchronous-programming),
and [.NET generic terminology](https://learn.microsoft.com/zh-cn/dotnet/standard/generics/).

Windvale semantics remain controlling. A familiar translation is rejected when
it would misstate ownership, failure, capability, profile, or structured-task
behavior.

## Keyword table

| Ordinal | Token | Canonical spelling | Draft `zh-Hans` spelling | Status |
| ---: | --- | --- | --- | --- |
| 0001 | `KW_APPLICATION` | `application` | `应用` | Draft |
| 0002 | `KW_AS` | `as` | `作为` | Draft |
| 0003 | `KW_ASYNC` | `async` | `异步` | Draft |
| 0004 | `KW_AUTHORITY` | `authority` | `权限` | Draft |
| 0005 | `KW_AWAIT` | `await` | `等待` | Draft |
| 0006 | `KW_BASE` | `base` | `基于` | Draft |
| 0007 | `KW_BOOL` | `bool` | `布尔` | Draft |
| 0008 | `KW_BORROW` | `borrow` | `借用` | Draft |
| 0009 | `KW_BREAK` | `break` | `跳出` | Draft |
| 0010 | `KW_BYTES` | `bytes` | `字节串` | Draft |
| 0011 | `KW_CANCEL_JOIN` | `cancel_join` | `取消汇合` | Draft |
| 0012 | `KW_CAPABILITY` | `capability` | `能力` | Draft |
| 0013 | `KW_CASE` | `case` | `分支` | Draft |
| 0014 | `KW_CONST` | `const` | `常量` | Draft |
| 0015 | `KW_CONTINUE` | `continue` | `继续` | Draft |
| 0016 | `KW_COPY` | `copy` | `复制` | Draft |
| 0017 | `KW_CORE` | `core` | `核心` | Draft |
| 0018 | `KW_DATA` | `data` | `数据` | Draft |
| 0019 | `KW_DERIVE` | `derive` | `派生` | Draft |
| 0020 | `KW_EFFECTS` | `effects` | `效应` | Draft |
| 0021 | `KW_ELSE` | `else` | `否则` | Draft |
| 0022 | `KW_ENUM` | `enum` | `枚举` | Draft |
| 0023 | `KW_EXPORT` | `export` | `导出` | Draft |
| 0024 | `KW_FAIL_JOIN` | `fail_join` | `失败汇合` | Draft |
| 0025 | `KW_FALSE` | `false` | `假` | Draft |
| 0026 | `KW_FN` | `fn` | `函数` | Draft |
| 0027 | `KW_FOR` | `for` | `遍历` | Draft |
| 0028 | `KW_FOREIGN` | `foreign` | `外部` | Draft |
| 0029 | `KW_HOSTED` | `hosted` | `托管` | Draft |
| 0030 | `KW_IF` | `if` | `如果` | Draft |
| 0031 | `KW_IMPLEMENT` | `implement` | `实现` | Draft |
| 0032 | `KW_IMPORT` | `import` | `导入` | Draft |
| 0033 | `KW_IN` | `in` | `在` | Draft |
| 0034 | `KW_JOIN` | `join` | `汇合` | Draft |
| 0035 | `KW_LET` | `let` | `令` | Draft |
| 0036 | `KW_LIBRARY` | `library` | `库` | Draft |
| 0037 | `KW_MATCH` | `match` | `匹配` | Draft |
| 0038 | `KW_MODULE` | `module` | `模块` | Draft |
| 0039 | `KW_MOVE` | `move` | `移动` | Draft |
| 0040 | `KW_MUT` | `mut` | `可变` | Draft |
| 0041 | `KW_NEVER` | `never` | `永不` | Draft |
| 0042 | `KW_OPTIONAL` | `optional` | `可选` | Draft |
| 0043 | `KW_MAXIMUM` | `maximum` | `上限` | Draft |
| 0044 | `KW_PACKAGE` | `package` | `包` | Draft |
| 0045 | `KW_PLATFORM` | `platform` | `平台` | Draft |
| 0046 | `KW_POLICY` | `policy` | `策略` | Draft |
| 0047 | `KW_PROFILE` | `profile` | `配置` | Draft |
| 0048 | `KW_PROTOCOL` | `protocol` | `协议` | Draft |
| 0049 | `KW_RECORD` | `record` | `记录` | Draft |
| 0050 | `KW_REQUIRES` | `requires` | `需要` | Draft |
| 0051 | `KW_RETURN` | `return` | `返回` | Draft |
| 0052 | `KW_RUNE` | `rune` | `字符标量` | Draft |
| 0053 | `KW_SCOPE` | `scope` | `作用域` | Draft |
| 0054 | `KW_SERVICE` | `service` | `服务` | Draft |
| 0055 | `KW_SYSTEM` | `system` | `系统` | Draft |
| 0056 | `KW_TASK` | `task` | `任务` | Draft |
| 0057 | `KW_TEXT` | `text` | `文本` | Draft |
| 0058 | `KW_TRUE` | `true` | `真` | Draft |
| 0059 | `KW_TRY` | `try` | `尝试` | Draft |
| 0060 | `KW_UNIT` | `unit` | `单元` | Draft |
| 0061 | `KW_UNSAFE` | `unsafe` | `不安全` | Draft |
| 0062 | `KW_USING` | `using` | `使用` | Draft |
| 0063 | `KW_VAR` | `var` | `变量` | Draft |
| 0064 | `KW_VARIANT` | `variant` | `变体` | Draft |
| 0065 | `KW_VERSION` | `version` | `版本` | Draft |
| 0066 | `KW_WHERE` | `where` | `约束` | Draft |

## Foundation option catalog

| Kind | Canonical key | Draft `zh-Hans` label | Status |
| --- | --- | --- | --- |
| Module | `Foundationˉoption` | `基础库ˉ可选值` | Draft |
| Declaration | `Option` | `可选值` | Draft |
| Case | `Absent` | `无值` | Draft |
| Case | `Present` | `有值` | Draft |
| Operation | `Borrow` | `获取ˉ借用` | Draft |
| Operation | `Borrowˉmut` | `获取ˉ可变ˉ借用` | Draft |
| Operation | `Isˉpresent` | `是否ˉ有值` | Draft |
| Operation | `Map` | `映射` | Draft |
| Operation | `Take` | `取出` | Draft |
| Parameter | `Borrow.Value` | `值` | Draft |
| Parameter | `Borrowˉmut.Value` | `值` | Draft |
| Parameter | `Isˉpresent.Value` | `值` | Draft |
| Parameter | `Map.Transform` | `转换器` | Draft |
| Parameter | `Map.Value` | `值` | Draft |
| Parameter | `Option.Present.Value` | `值` | Draft |
| Parameter | `Take.Value` | `值` | Draft |

`Borrow` cannot use the single-segment label `借用` because that spelling is the
localized `borrow` keyword. The draft uses `获取ˉ借用` to preserve meaning while
remaining lexically usable. Reusing `值` for parameters is legal because those
parameters occupy different operation or case namespaces.

## Terms requiring special reviewer attention

- `权限` must convey Windvale's authority classification rather than a capability
  grant.
- `能力` must read as an explicitly bound semantic capability, not general skill.
- `汇合`, `取消汇合`, and `失败汇合` must communicate structured task-exit policy
  rather than arbitrary thread joining.
- `效应` must represent declared function effects without implying runtime side
  effects that are absent from the signature.
- `配置` must represent the `core`/`hosted`/`system` language profile, not a
  mutable application configuration file.
- `令` must read naturally as an immutable binding declaration beside mutable
  `变量`.
- `字符标量` must communicate one Unicode scalar rather than a grapheme or UTF
  code unit.
- `可选值`, `无值`, and `有值` must make absence explicit without suggesting
  implicit `null`.

## Change rule

Review edits occur in the exact `.wvlex` or `.wvcat` first. Regenerate the
dependent profile, lock, and artifact index hashes afterward. A prose-only
approval that disagrees with artifact bytes is invalid.
