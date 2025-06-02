# Features Directory Structure

This directory follows the CQRS (Command Query Responsibility Segregation) pattern:

## Commands
- Contains all command handlers and command models
- Each command should be in its own folder with the following structure:
  - CommandName/
    - CommandNameCommand.cs
    - CommandNameCommandHandler.cs
    - CommandNameValidator.cs

## Queries
- Contains all query handlers and query models
- Each query should be in its own folder with the following structure:
  - QueryName/
    - QueryNameQuery.cs
    - QueryNameQueryHandler.cs
    - QueryNameValidator.cs

## Common
- Shared models and utilities used by both commands and queries 