# Windvale security policy

Windvale treats source modules, bytecode, object files, assembly, linked inputs, capability requests, diagnostics, and repository contributions as potentially untrusted. Security reports that could help someone exploit a defect must not begin in a public issue.

## Supported versions

Windvale has no stable release or compatibility-support window yet. Security work targets the current `main` branch. Historical commits, development formats, generated artifacts, and unqualified forks are unsupported unless a release notice explicitly says otherwise.

## Reporting a vulnerability

Use GitHub's **Report a vulnerability** form under the repository's Security tab. This creates a private repository security advisory visible to the project security maintainers. Private vulnerability reporting must be enabled when the public repository is created.

Include, when available:

- The affected commit, command, format version, and host.
- A minimal reproducer or malformed input.
- The expected and observed result.
- Security impact and realistic attack conditions.
- Whether output, credentials, private data, or system integrity may be affected.
- Any proposed mitigation, without requiring the reporter to supply a patch.

Do not include secrets or personal data that are unnecessary to reproduce the issue. If the GitHub private-reporting control is unavailable, use a private business contact route published at [eworker.ca](https://eworker.ca/) and request a confidential security channel; do not disclose exploit details in that first message.

## Response and disclosure

E-Worker Inc will acknowledge a valid channel when reasonably possible, assess the report, and coordinate remediation and disclosure according to severity and available maintainer capacity. Windvale is an experimental open-source project and does not promise a service-level or response-time guarantee.

Please allow reasonable time for investigation before public disclosure. The project may create a private fix, request additional evidence, reject a report that is not a vulnerability, or publish an advisory after affected users have a practical mitigation.

Good-faith research that avoids privacy violations, data destruction, service disruption, credential access, and testing against systems without authorization is welcome. This policy does not authorize activity that would otherwise be unlawful or outside systems controlled by the researcher.
