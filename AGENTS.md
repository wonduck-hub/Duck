# AGENTS.md

## Purpose

This repository follows the POCU Academy C# coding standard.
When generating, modifying, or reviewing C# code, follow these rules consistently.

Reference: https://docs.popekim.com/ko/coding-standards/pocu-csharp

## Core principles

- Prioritize readability above all else. Code should usually serve as its own documentation.
- Follow Visual Studio's default formatting unless there is a strong reason not to.
- Prefer simple, explicit, maintainable code over clever or overly abstract code.
- Learn from the existing code in this repository and keep the style consistent.
- Do not introduce new architectural patterns or abstractions unless they are clearly justified.

## Naming conventions

- Class names and struct names use PascalCase.
- Namespace names use PascalCase.
- Method names use PascalCase when public.
- Non-public methods use camelCase.
- Local variables and method parameters use camelCase.
- Method names should usually be verb + noun, such as `GetAge()`, `ReadPacket()`, or `SendCommand()`.
- Boolean-returning methods should start with `Is`, `Can`, `Has`, or `Should` when natural.
- Boolean variables should use the `b` prefix.
 - Example: `bool bConnected;`
- Private boolean member variables should use the `mb` prefix.
 - Example: `private bool mbConnected;`
- Boolean properties should start with `Is`, `Has`, `Can`, or `Should`.
- Interfaces must start with `I`.
 - Example: `ITransport`
- Enums must start with `E`.
 - Example: `EConnectionState`
- Non-readonly structs should start with `S`.
 - Example: `SUserData`
- Private member variables must start with `m` and use PascalCase.
 - Example: `private int mRetryCount;`
- Constants use UPPER_CASE_WITH_UNDERSCORES.
 - Example: `const int MAX_RETRY_COUNT = 3;`
- Static readonly values used as constants also use UPPER_CASE_WITH_UNDERSCORES.
- Do not use vague variable names such as `i`, `e`, or `data` unless the scope is very small and the meaning is obvious.
- Acronyms at the end of a name should be fully capitalized.
 - Example: `OrderID`
 - Example: `HttpCode`

## Properties, fields, and immutability

- Prefer properties over Java-style getters and setters.
- Use `readonly` for fields that do not change after initialization.
- Use `static readonly` for object values used as constants.
- Avoid mutable static state unless there is a strong reason.

## Method and API design

- Function names that return values should clearly describe what they return.
- Avoid overloading methods when the parameter types are too generic.
 - Prefer `GetAnimByIndex(int index)` and `GetAnimByName(string name)` over `GetAnim(int)` and `GetAnim(string)`.
- Prefer method overloads over default parameters.
- If default parameters are used, prefer zero-bit-pattern values such as `null`, `false`, or `0`.
- Recursive methods must end with `Recursive`.
- Avoid returning `null`, especially from public methods.
- If a method may return `null`, add `OrNull` to the method name.
 - Example: `GetNameOrNull()`
- Avoid accepting `null` parameters, especially in public methods.
- If a parameter may be `null`, add `OrNull` to the parameter name.
 - Example: `string nameOrNull`

## Validation and exceptions

- Validate external input at the boundary of the system.
- Once data has crossed the boundary, internal code may assume it is valid.
- Avoid throwing exceptions from internal functions unless truly appropriate.
- Handle exceptions at system boundaries when possible.
- It is acceptable to throw in a `default` case when an enum switch does not handle a newly added enum value.
- Do not swallow exceptions silently.
- Add enough diagnostic context when reporting errors.

## Assertions

- Use `Debug.Assert()` for assumptions that must be true.
- Add assertions for important assumptions made while writing code.
- Use assertions for unrecoverable programmer errors, not for normal runtime validation.
- In `switch` statements where `default` should be unreachable, use `Debug.Fail()` or throw an appropriate exception.

## Switch statements

- Every `switch` statement must include a `default:` case.
- If the default case should never happen, explicitly fail or throw.
- Enum switches should make unhandled values obvious.

## Async and threading

- Use `async Task` instead of `async void`.
- `async void` is allowed only for event handlers.
- Avoid blocking async code with `.Wait()`, `.Result`, or unnecessary thread sleeps.
- Use `CancellationToken` for cancelable long-running or I/O operations.
- Protect shared device, transport, or communication resources with explicit synchronization.
- Do not hide race conditions behind overly broad abstractions.

## Collections and types

- Use `System.Collections.Generic` collections instead of non-generic `System.Collections` collections.
- Arrays are acceptable when appropriate.
- Avoid `var` unless the right-hand side makes the type obvious or the exact type is unimportant.
 - Acceptable: `var employee = new Employee();`
 - Prefer explicit type when returned from a method and the type is not obvious.
- Do not use target-typed `new()`. Always write the type explicitly after `new`.
 - Use: `Employee employee = new Employee();`
 - Avoid: `Employee employee = new();`
- Avoid object initializers when practical.
- Do not use the null-coalescing operator.
- Do not use C# 8 using declarations. Use traditional `using` statements instead.
- Declare `out` variables on a separate line before passing them as arguments.

## Class and file organization

- In a class, order members as follows:
 1. Member variables
 2. Properties
 3. Constructors
 4. Methods
- Public methods should appear before private methods when practical.
- Group related member variables and methods together.
- Each class should usually be in its own source file.
- File names must match class names exactly, including casing.
- For partial classes, file names should start with the class name and then include the specific part name.
 - Example: `Human.Head.cs`

## Formatting

- Use Visual Studio default formatting.
- Use tabs according to Visual Studio defaults. If tabs are unavailable, use 4 spaces.
- Opening braces must always be on a new line.
- Always use braces, even for one-line `if`, `for`, `while`, and similar statements.
- Declare only one variable per line.
- Declare local variables close to where they are used.
- Add `F` to float literals unless `double` is required.
 - Example: `float ratio = 0.5F;`

## Flags enum

- Bit flag enums must end with `Flags`.
- Use `[Flags]` for bit flag enums.
 - Example: `EVisibilityFlags`

## Code generation behavior

When generating code:

1. Follow the naming, formatting, and organization rules above.
2. Match the style of nearby existing code.
3. Prefer minimal, direct changes over broad refactoring.
4. Do not introduce new interfaces, factories, managers, strategies, or base classes unless they reduce real duplication or solve a clear design problem.
5. Keep control flow easy to read.
6. Make error paths explicit.
7. Avoid clever one-liners when a clearer multi-line implementation is easier to read.
8. Do not silently change public APIs unless requested.
9. Do not mix unrelated cleanup with the requested change.
10. If a rule conflicts with existing project style, point out the conflict before applying a large change.

## Review checklist

Before finalizing code, verify:

- Names follow the POCU C# standard.
- Public methods use PascalCase.
- Non-public methods use camelCase.
- Private fields use the `m` prefix.
- Boolean variables and properties follow the required naming rules.
- Constants use UPPER_CASE_WITH_UNDERSCORES.
- `switch` statements have `default:` cases.
- Important assumptions use `Debug.Assert()` or `Debug.Fail()`.
- `async void` is used only for event handlers.
- No unnecessary `var`, target-typed `new()`, null-coalescing operator, or using declaration was introduced.
- Each variable declaration is on its own line.
- Braces are always used.
- Opening braces are on new lines.
- The change is readable, minimal, and consistent with existing code.
