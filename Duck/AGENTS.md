# AGENTS.md

## C# coding standard

IMPORTANT: Before generating, modifying, or reviewing any C# code, read the `.editorconfig` file located in the same directory as this `AGENTS.md` and follow all applicable naming, formatting, and code-style rules.

- Treat the applicable `.editorconfig` as the single source of truth for C# naming, formatting, and mechanically enforceable code-style rules.
- If a target file is governed by a more specific nested `.editorconfig`, follow the closest applicable `.editorconfig` according to normal EditorConfig precedence.
- Do not duplicate, reinterpret, or replace `.editorconfig` rules with standard C# conventions.
- If nearby legacy code conflicts with `.editorconfig`, follow `.editorconfig` for newly written or modified code.
- Do not modify `.editorconfig` unless the user explicitly requests a coding-standard change.
- Do not manually edit generated code or `*.Designer.cs` files unless explicitly requested.

## Core principles

- Prioritize readability above all else. Code should usually serve as its own documentation.
- Follow Visual Studio's default formatting unless the applicable `.editorconfig` specifies otherwise.
- Prefer simple, explicit, maintainable code over clever or overly abstract code.
- Learn from the existing code in this repository while treating `.editorconfig` as authoritative for mechanical style rules.
- Do not introduce new architectural patterns or abstractions unless they are clearly justified.

## Semantic naming guidance

- Method names should usually be verb + noun, such as `GetAge()`, `ReadPacket()`, or `SendCommand()`.
- Boolean-returning methods should start with `Is`, `Can`, `Has`, or `Should` when natural.
- Boolean properties should start with `Is`, `Has`, `Can`, or `Should` when natural.
- Do not use vague variable names such as `i`, `e`, or `data` unless the scope is very small and the meaning is obvious.
- Acronyms at the end of a name should be fully capitalized when required by the project coding standard.

## Code generation behavior

When generating or modifying code:

1. Read the applicable `.editorconfig` before writing C# code.
2. Follow its naming, formatting, and code-style rules exactly.
3. Match the design and organization of nearby existing code when it does not conflict with `.editorconfig`.
4. Prefer minimal, direct changes over broad refactoring.
5. Do not introduce new interfaces, factories, managers, strategies, or base classes unless they reduce real duplication or solve a clear design problem.
6. Keep control flow easy to read.
7. Make error paths explicit.
8. Avoid clever one-liners when a clearer multi-line implementation is easier to read.
9. Do not silently change public APIs unless requested.
10. Do not mix unrelated cleanup with the requested change.
11. If a requested change conflicts with `.editorconfig`, report the conflict rather than silently ignoring the rule.

## Validation

- After modifying C# code, run the repository's formatter, analyzers, and build when available.
- Resolve new `.editorconfig` and analyzer diagnostics introduced by the change.
- Do not broadly reformat unrelated legacy files.
- Verify that the final change is readable, minimal, and consistent with the applicable `.editorconfig`.
