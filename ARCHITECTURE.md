# Jira Clone Backend - Clean Architecture Guidelines

## 1. Solution Structure
- **NewJira.Domain**: Chứa các Enterprise Entities, Enums, và Value Objects cốt lõi. (Không reference project nào khác).
- **NewJira.Application**: Chứa Business Logic interfaces, DTOs, Validators, và Use Cases/Handlers. (Chỉ reference Domain).
- **NewJira.Infrastructure**: Chứa Entity Framework Core (DbContext, Migrations), Repositories implementation, và External Services (JWT, Email). (Reference Application).
- **NewJira.API**: Chứa Presentation layer (Controllers, Middlewares, Program.cs). (Reference Application & Infrastructure).

## 2. Naming Conventions & Rules
- **Entities**: PascalCase, đại diện cho bảng trong Database.
- **DTOs**: Chia rõ `Request` và `Response` (Ví dụ: `CreateProjectRequestDto`, `ProjectResponseDto`).
- **Dependency Injection**: Đăng ký services theo từng tầng thông qua các Extension Methods (ví dụ: `AddInfrastructureServices()`).