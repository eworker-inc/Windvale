# Mathematical and scientific modeling exploration

- Date: 2026-08-07
- Status: Exploratory future work; not accepted language syntax, an implementation commitment, or a roadmap phase
- Revisit after: the ordinary Windvale language, compiler, numeric model, libraries, and native execution path have evolved enough to support a measured scientific consumer

## Purpose

This document explores whether mathematical and physical equations could be
expressed as precise Windvale-associated source rather than existing only as
handwritten or typeset notation. The aim is not to replace readable
mathematics with ordinary imperative code. It is to investigate one semantic
model that can provide both familiar mathematical presentation for people and
typed, inspectable input for checking, simulation, proof, and compilation.

Traditional notation is compact and expressive, but it commonly leaves
important meaning in surrounding prose or disciplinary convention. Domains,
units, coordinate frames, assumptions, approximations, boundary conditions,
and even the intended meaning of equality may be implicit. A machine-readable
form could make those choices explicit while an editor or documentation tool
continues to render conventional fractions, powers, sums, integrals,
derivatives, and aligned equations.

## Possible product shape

The canonical artifact should remain deterministic plain source that is easy
to review, diff, search, and reproduce. Two-dimensional typesetting should be a
derived view rather than the only parseable form. A future path might be:

```text
scientific model source
        |
        v
typed mathematical model
   |            |                 |
   v            v                 v
typeset      unit/proof      explicit numerical method
notation     checks                 |
                                     v
                              Windvale WIR/WVB
                                     |
                              interpreter/JIT/AOT
```

The model, numerical method, and executable implementation must remain
separate contracts. An equation describes a relationship; it does not
necessarily provide an algorithm. Selecting a floating-point width, mesh,
integration method, tolerance, stopping rule, or approximation can change a
scientific result and must not be hidden behind automatic lowering.

Illustrative future pseudo-source might resemble the following. This is not
accepted Windvale syntax:

```text
model Pendulum {
    parameter Length: metre where Length > 0 metre;
    parameter Gravity: metre / second^2 = 9.80665;

    state Angle(Time: second): radian;

    equation Derivative(Angle, Time, 2)
        + (Gravity / Length) * Sin(Angle) = 0;

    initial Angle(0 second) = 0.2 radian;
    initial Derivative(Angle, Time)(0 second) = 0 radian / second;
}
```

A tool could render the central equation conventionally while checking that
`Sin` receives an angle, both added terms have the same dimensions, the
initial conditions are well typed, and a separately selected solver supports
the model's requirements.

## Potential value

- Catch dimensional, type, shape, and domain mistakes before execution.
- Record assumptions, constants, units, initial conditions, and solver choices
  as versioned evidence rather than leaving them in prose or local scripts.
- Reduce errors introduced when equations are manually translated into
  simulation or production code.
- Generate readable notation, executable kernels, tests, and machine-readable
  reports from one reviewed semantic model.
- Permit bounded symbolic transformations or proof obligations where they are
  useful without claiming that every equation can be solved automatically.
- Make scientific models searchable, refactorable, accessible through
  nonvisual tools, and reproducible across supported Windvale execution modes.
- Keep data-set, sensor, accelerator, filesystem, and network authority
  explicit through ordinary capability requirements rather than giving a
  mathematical model ambient host access.

Windvale's deterministic artifacts, checked operations, explicit limits,
verified bytecode, and shared interpreter/JIT/AOT direction are attractive
foundations for this work. A capability-free model could remain portable,
while acquisition of observations or publication of results would use
separate rights-limited providers.

## Risks and boundaries

- Formal source can become much more verbose than conventional notation if the
  language requires every contextual convention to be restated.
- Exact, symbolic, real, floating-point, interval, uncertain, and measured
  quantities have different semantics and must not be collapsed into one
  convenient `number` type.
- Units alone are insufficient for many physical models; coordinate frames,
  tensor variance, basis choice, orientation, scale, and uncertainty may also
  matter.
- Symbolic rewriting, simplification, and proof search can consume unbounded
  time or memory unless their rules and budgets are explicit.
- Differential equations, integrals, infinite series, and optimization
  problems may have no general computable solution. A compiler must not imply
  that a successful parse proves existence, uniqueness, stability, or physical
  validity.
- Numerical discretization and floating-point behavior can dominate the error
  in a result. Generated execution must expose these choices and preserve
  enough evidence to reproduce them.
- A large scientific type system or syntax expansion could distract from the
  smaller application-and-systems language and create a parallel compiler or
  runtime stack.

For those reasons, this exploration does not add mathematical operators,
operator overloading, unrestricted macros, floating point, general generics,
or a solver to the current language. A later proposal should reuse the shared
Windvale compiler, verifier, runtime, and native backend wherever their
contracts fit. If a distinct notation front end is justified, it should lower
through an explicit typed mathematical representation and the existing
execution stack rather than silently defining a second machine model.

## Candidate first experiment

After the language has matured, the smallest useful experiment should begin
with one real, bounded consumer rather than a general computer-algebra system.
A candidate scope is:

- exact integers and rational constants;
- named physical dimensions and units with checked conversion;
- scalar variables, parameters, equations, and explicit assumptions;
- one bounded symbolic operation such as differentiation;
- conventional rendered output from canonical plain source; and
- explicit lowering of one numerically computable model through a selected
  method to ordinary Windvale execution.

Vectors, matrices, tensors, complex values, uncertainty, automatic proof,
optimization, partial differential equations, and general solver integration
should follow only when measured models require them.

## Revisit questions

- Which real mathematical or physical model is small enough to qualify the
  first experiment while still exposing value beyond an ordinary library?
- Does that consumer require new source syntax, or can an evolved Windvale
  library and editor view provide the semantic model without a new front end?
- Which exact numeric contracts are required: rational, decimal, binary
  floating point, interval, complex, arbitrary precision, or a bounded subset?
- Which properties are type checks, which are proof obligations, and which are
  solver claims with separately recorded evidence?
- What canonical form preserves meaningful names and source structure without
  making algebraic equivalence a requirement for reproducible bytes?
- Which resource budgets bound symbolic work, numerical execution,
  diagnostics, generated artifacts, and hostile model input?
- What differential oracle and reference models can validate units,
  transformations, solvers, and cross-host results independently?

Until those questions have a measured consumer and the underlying language is
ready, mathematical and scientific modeling remains a documented exploration,
not an active implementation priority.
