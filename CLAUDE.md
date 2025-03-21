# NetNitel Development Guidelines

## Build Commands
- Build: `dotnet build`
- Run: `dotnet run --project src/netnitel.csproj`
- Watch: `dotnet watch --project src/netnitel.csproj run`
- Publish: `dotnet publish -c Release`

## Code Style
- Use C# 9.0+ features (target is .NET 9.0)
- Enable Nullable reference types
- XML documentation for public methods and classes
- 4-space indentation, no tabs
- PascalCase for classes, methods, properties
- camelCase for local variables and parameters
- _camelCase for private fields

## Conventions
- Async methods should have `async` suffix
- Use dependency injection where appropriate
- Prefix interfaces with 'I'
- Handle exceptions appropriately with try/catch blocks
- Use `var` when the type is obvious
- Favor composition over inheritance
- Keep methods short and focused on a single responsibility

## Minitel Specifics
- Respect 40x24 character screen limitations
- Use Minitel color palette (8 colors) for graphics
- Image dimensions should be 80x72 pixels for display