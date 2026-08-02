# Decision 0113: Community-source licensing and commercial stewardship

- Date: 2026-08-02
- Status: Accepted and implemented

## Context

Windvale is a young public project whose source was initially published under the MIT License. That license allowed organizations of any size to use Windvale in production, redistribute Windvale-based products, or commercialize the stack without contributing to its development. E-Worker Inc needs a sustainable way to fund the compute, security work, verification, hosting, and stewardship required to develop the project, without charging individuals, researchers, evaluators, or qualifying small organizations for ordinary use.

The project also needs a clear boundary between Windvale and software created with Windvale. An application should not become Windvale-owned or inherit Windvale's license merely because it is written in the language, uses documented interfaces and formats, or includes the ordinary runtime material needed to execute. Separately owned third-party tools, fonts, libraries, and other components cannot be relicensed as Windvale work.

Future external contributions must provide enough rights for E-Worker to continue the community-source distribution and offer negotiated commercial licenses. A DCO sign-off confirms a contributor's authority to submit but does not itself grant those broader relicensing rights.

## Decision

Apply the [Windvale Community Source License 1.0](../../LICENSE) to Windvale-owned work published from the adopting commit onward. It is a source-available license and is not represented as an Open Source Initiative approved open-source license.

The license permits personal, noncommercial, evaluation, and qualifying small-organization use without charge. Large-organization production use, Windvale-as-a-product use, certain hosted offerings, and proprietary redistribution require a separate written commercial agreement. The consolidated gross-revenue threshold is US$20 million over the preceding consecutive twelve months, with the transition period defined in the license.

The application and runtime exception is an explicit ownership and licensing boundary. Independent applications, services, content, and data created with Windvale belong to their creators and may use terms of their choice. Ordinary use of Windvale tools, documented APIs and formats, and identified redistributable runtime material does not extend the Windvale license to an application as a whole. Windvale and modifications to Windvale remain covered.

Adopt the [Windvale Contributor License Agreement 1.0](../../CONTRIBUTOR-LICENSE-AGREEMENT.md) for external contributions in addition to the existing DCO sign-off. Contributors retain their copyright while granting E-Worker broad copyright and patent permissions, including the ability to distribute contributions under the community license and negotiate separate commercial terms. Pull requests record the contributor's legal identity, account, agreement version, and acceptance date.

Third-party material remains outside the Windvale license and under its own terms. Distributed copies must preserve the applicable notices summarized in [Third-party notices](../../THIRD-PARTY-NOTICES.md).

This decision supersedes the current licensing conclusion in [Decision 0028](0028-MIT-License-And-E-Worker-Stewardship.md), the MIT-specific legal explanation in [Decision 0031](0031-AI-Authorship-And-Vendor-Neutrality.md), and the DCO-without-CLA contribution rule in [Decision 0032](0032-Public-Contribution-And-Governance-Foundation.md). Their other historical, attribution, governance, security, and project-identity records remain applicable where not inconsistent with this decision.

Repository history published before this adoption remains governed by the licenses applied to those versions. No additional statement in the operative license is required to preserve rights already granted.

## Consequences

Current-facing documentation and website text must describe Windvale as source-available or community source rather than open source. Source distributions must include the root license, the contributor agreement where contribution instructions are carried, and the notices required by redistributed third-party components.

Organizations can evaluate Windvale before negotiating terms, and qualifying small organizations can use it in production within the public license. E-Worker retains a commercial path for larger production deployments and products whose principal value is Windvale itself. Application authors retain control of their independent work.

The custom license may be less familiar to adopters and package registries than a standard open-source license. Ambiguous use cases and commercial licensing questions are directed to [info@eworker.ca](mailto:info@eworker.ca). The project must avoid describing third-party components as Windvale-owned or promising rights that their upstream licenses do not grant.

## Reconsider when

- The revenue threshold no longer reflects the intended small-organization boundary.
- A stable release needs component-specific exceptions or a separately versioned runtime exception.
- Contributor volume requires automated CLA identity and version tracking.
- A standards body, foundation, or other entity assumes licensing or stewardship responsibilities.
- Adoption evidence supports simplifying, replacing, or versioning the community-source terms.
