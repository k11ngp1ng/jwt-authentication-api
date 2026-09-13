# JWT Authentication API

API RESTful de autenticação e autorização baseada em JWT, construída com C#,
ASP.NET Core e .NET 10. O projeto demonstra separação de responsabilidades,
persistência com SQLite, hash seguro de senhas e autorização baseada em papéis.

## Funcionalidades

- Registro de usuários com validação de entrada.
- Hash de senha com BCrypt Enhanced, SHA-384 e work factor 12.
- Login com retorno de access token JWT.
- Claims de ID, username, e-mail e papel.
- Autorização com [Authorize] e [Authorize(Roles = "Admin")].
- Persistência com Entity Framework Core e SQLite.
- Migrações aplicadas automaticamente na inicialização.
- Respostas de erro padronizadas com RFC 7807 ProblemDetails.
- OpenAPI/Swagger com autenticação Bearer.
- Testes unitários e de integração HTTP.

## Arquitetura

~~~text
src/
├── JwtAuthenticationApi.Api
│   ├── Controllers
│   └── Middleware
├── JwtAuthenticationApi.Application
│   ├── DTOs
│   ├── Exceptions
│   ├── Interfaces
│   └── Services
├── JwtAuthenticationApi.Domain
│   ├── Entities
│   └── Enums
└── JwtAuthenticationApi.Infrastructure
    ├── Authentication
    ├── Persistence
    └── Repositories

tests/
└── JwtAuthenticationApi.Tests
    ├── Unit
    └── Integration
~~~

Dependências entre camadas:

~~~text
Api ───────────► Application
 │                    │
 └──► Infrastructure  └──► Domain
          │
          └───────────────► Domain
~~~

## Tecnologias

- .NET 10 e ASP.NET Core
- Entity Framework Core 10
- SQLite
- JWT Bearer Authentication
- BCrypt.Net-Next
- Swashbuckle/OpenAPI
- xUnit

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- Certificado HTTPS de desenvolvimento confiável:

~~~powershell
dotnet dev-certs https --trust
~~~

## Configuração

Restaure as dependências e ferramentas locais:

~~~powershell
dotnet restore
dotnet tool restore
~~~

A chave JWT nunca deve ser armazenada no repositório. Para desenvolvimento,
gere uma chave aleatória e salve-a no .NET User Secrets:

~~~powershell
$jwtSecret = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
dotnet user-secrets set "Jwt:SecretKey" $jwtSecret --project .\src\JwtAuthenticationApi.Api
~~~

As demais configurações estão em
src/JwtAuthenticationApi.Api/appsettings.json:

~~~json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=jwt-auth.db"
  },
  "Jwt": {
    "Issuer": "JwtAuthenticationApi",
    "Audience": "JwtAuthenticationApi.Client",
    "SecretKey": "",
    "ExpirationMinutes": 60
  }
}
~~~

Em produção, forneça Jwt__SecretKey por variável de ambiente ou por um
gerenciador de segredos.

## Executando

~~~powershell
dotnet run --project .\src\JwtAuthenticationApi.Api --launch-profile https
~~~

A interface Swagger estará disponível em:

~~~text
https://localhost:7035/swagger
~~~

O arquivo SQLite e as tabelas são criados automaticamente. Para aplicar
migrações manualmente:

~~~powershell
dotnet ef database update --project .\src\JwtAuthenticationApi.Infrastructure --startup-project .\src\JwtAuthenticationApi.Api
~~~

## Endpoints

| Método | Endpoint | Autorização | Descrição |
|---|---|---|---|
| POST | /api/auth/register | Público | Registra usuário e retorna JWT |
| POST | /api/auth/login | Público | Autentica usuário e retorna JWT |
| GET | /api/protected/user | Bearer | Acesso para usuário autenticado |
| GET | /api/protected/admin | Papel Admin | Acesso administrativo |

Novos registros recebem sempre o papel User. A promoção para Admin deve
ser feita por um processo administrativo confiável, nunca pelo endpoint
público de registro.

### Registro

~~~http
POST /api/auth/register
Content-Type: application/json

{
  "username": "demo_user",
  "email": "demo@example.com",
  "password": "StrongPassword@123"
}
~~~

### Login

~~~http
POST /api/auth/login
Content-Type: application/json

{
  "email": "demo@example.com",
  "password": "StrongPassword@123"
}
~~~

Resposta:

~~~json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresAtUtc": "2026-09-13T19:00:00+00:00"
}
~~~

Use o token nas rotas protegidas:

~~~http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
~~~

Uma coleção pronta está disponível em
[requests/JwtAuthenticationApi.http](requests/JwtAuthenticationApi.http).

## Validação de entrada

O registro exige:

- Username entre 3 e 50 caracteres, usando letras, números ou _.
- E-mail válido com no máximo 254 caracteres.
- Senha entre 8 e 128 caracteres.
- Senha contendo letra maiúscula, minúscula, número e caractere especial.

Credenciais inválidas retornam uma mensagem genérica para evitar revelar se
um e-mail está cadastrado.

## Testes

~~~powershell
dotnet test --configuration Release
~~~

A suíte cobre:

- Validação dos DTOs.
- Geração e claims do JWT.
- Hash e verificação BCrypt.
- Serviços de registro e login.
- Repositório usando SQLite real em memória.
- Controllers e metadados de autorização.
- Fluxo HTTP completo, incluindo respostas 401, 403 e 409.

## Segurança

- Senhas nunca são persistidas em texto puro.
- A chave JWT fica fora do código-fonte.
- Tokens validam assinatura, emissor, audiência e expiração.
- O tempo de tolerância de expiração (ClockSkew) é zero.
- E-mail e username possuem índices únicos no banco.
- O registro público não permite escolher o papel do usuário.
- Erros internos não expõem stack traces ao cliente.

## Licença

Este projeto não possui uma licença definida. Consulte o proprietário antes
de reutilizar ou redistribuir o código.
