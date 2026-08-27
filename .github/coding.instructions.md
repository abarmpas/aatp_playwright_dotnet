---
applyTo: '**'
---
# Coding Standards

## .NET Coding Guidelines
- Follow Microsoft .NET coding conventions for naming, formatting, and structure.
- Use clear, descriptive names for classes, methods, and variables.
- Prefer code that is self-explanatory through structure and well-chosen abstractions; do not add XML comments or explanatory comments by default.
- Keep methods and classes small and focused on a single responsibility.
- Prefer explicit over implicit code for clarity.

## SOLID Principles
- **Single Responsibility Principle**: Each class or method should have one reason to change.
- **Open/Closed Principle**: Classes should be open for extension, but closed for modification.
- **Liskov Substitution Principle**: Derived types must be substitutable for their base types.
- **Interface Segregation Principle**: Prefer many small, specific interfaces over large, general ones.
- **Dependency Inversion Principle**: Depend on abstractions, not concretions.

## DRY (Don't Repeat Yourself)
- Avoid code duplication by extracting reusable logic into methods, classes, or extensions.
- Use shared libraries and utilities where appropriate.

## Pragmatism
- Apply architectural principles (SOLID, DRY, Clean Architecture) only where they yield demonstrable value.
- Avoid overengineering; keep solutions as simple as possible.

## Decoupling
- Composition over inheritance is mandatory. Favor small, composable units.
- Use dependency injection to decouple components.

## Asynchrony
- Never block asynchronous code. Explicitly avoid using `.Wait()`, `.Result()`, or similar synchronous blocking calls.
- Use async/await throughout the codebase for I/O-bound operations.

## Exception Handling
- Implement robust, meaningful, and context-aware exception handling.
- Avoid catching general exceptions; catch specific exception types.
- Always log exceptions with sufficient context for troubleshooting.

## Logging
- Use Serilog's `Log` methods for diagnostic logging. Avoid `Console.WriteLine` or `Debug.WriteLine`.

## Code Hygiene
- Keep compiler/analyzer warnings clean in the files you change.
- Remove dead code (unused methods/variables) instead of leaving it behind.
- Do not commit commented-out code blocks meant for "future use".
