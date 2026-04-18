# Copilot instructions for this repository

## Project overview
This is a solo student exam project built in ASP.NET Core with a React frontend.

The application is a **media tracking app** where users can:
- Log media (movies, TV series, books, games, etc.)
- Rate and review entries
- Track consumed and planned media

The main goal of the project is to demonstrate:
- Reusable backend architecture (Repositories, Services, Mappers)
- A clean implementation of the Result Pattern for error handling
- Clear and maintainable code

## Constraints
- Solo project
- Focus is on learning AND delivering a working system

## Priorities
- Learning and understanding > clever or complex solutions
- Simplicity and clarity > overengineering
- Delivering a working vertical slice early
- Incremental improvements over large rewrites

## Architecture goals
- Use a clean, layered structure (Domain, Application, Infrastructure, Presentation)
- Keep separation of concerns clear
- Use DTOs, services, and repositories in a consistent way
- Use Result pattern for operation outcomes and error handling

## What Copilot should optimize for
- Suggest solutions that are realistic within the current project scope
- Prefer simple, explicit code over highly generic abstractions
- Help structure code, but avoid unnecessary complexity
- Highlight tradeoffs when suggesting patterns or abstractions

## Coding style
- Use clear and descriptive naming
- Keep methods small and focused
- Prefer readability over compact or "clever" code
- Match the existing style of the repository
- Avoid hiding logic in overly generic helpers unless clearly beneficial

## When suggesting architecture
- Clearly distinguish:
  - Must-have (important for the project)
  - Nice-to-have (can be skipped if time is limited)
- Avoid suggesting full rewrites unless necessary
- Prefer evolving the current design instead of replacing it

## When generating code
- Keep it beginner-to-intermediate friendly
- Do not assume advanced knowledge without explanation
- Prefer step-by-step solutions when possible
- Avoid generating large amounts of code without explanation

## Response style
- Be concise but clear
- Suggest one primary approach, optionally mention alternatives
- Explain *why* something is done, not just *what*- Explain *why* something is done, not just *what*