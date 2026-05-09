# CustomerApp API

API RESTful desenvolvida em **ASP.NET Core (.NET 10)** com autenticação **JWT** e banco de dados **MongoDB**, voltada para o gerenciamento de clientes com isolamento por usuário autenticado.

## Tecnologias utilizadas

| Camada | Tecnologia |
|---|---|
| Framework | ASP.NET Core 10 |
| Banco de Dados | MongoDB |
| Autenticação | JWT Bearer (HMAC-SHA256) |
| Documentação | Swagger / OpenAPI |
| Criptografia de senha | SHA-256 (Base64) |

### Pacotes NuGet

| Pacote | Versão |
|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.7 |
| `Microsoft.AspNetCore.OpenApi` | 10.0.7 |
| `MongoDB.Driver` | 3.8.0 |
| `Swashbuckle.AspNetCore` | 10.1.7 |
| `System.IdentityModel.Tokens.Jwt` | 8.18.0 |

## Objetivo do projeto

O projeto **CustomerApp** tem como objetivo fornecer uma API RESTful para cadastro, consulta, atualização e exclusão de clientes, com autenticação de usuários via JWT. Cada usuário autenticado acessa e gerencia exclusivamente os seus próprios clientes (isolamento por `userId`).

## Estrutura do projeto

```
CustomerApp.API/
├── Controllers/
│   ├── CustomersController.cs   # CRUD de clientes (protegido por JWT)
│   └── UsersController.cs       # Autenticação de usuários
├── Models/
│   ├── Customer.cs              # Modelo de cliente (MongoDB)
│   └── User.cs                  # Modelo de usuário (MongoDB)
├── Services/
│   ├── MongoDbService.cs        # Acesso às collections do MongoDB
│   ├── JwtService.cs            # Geração de tokens JWT
│   └── PasswordService.cs       # Hash e verificação de senha (SHA-256)
├── Runners/
│   └── DatabaseRunner.cs        # Seed automático do banco na inicialização
└── Program.cs                   # Configuração e bootstrap da aplicação
```

## Arquitetura geral

A aplicação segue o padrão **REST API** com controllers ASP.NET Core. O acesso ao MongoDB é centralizado no `MongoDbService` (singleton), e os serviços de negócio (`JwtService`, `PasswordService`) são registrados como scoped.

Fluxo principal:

1. Na inicialização, o `DatabaseRunner` cria o usuário administrador padrão no MongoDB caso ele não exista.
2. O cliente realiza autenticação em `POST /api/users/auth` enviando `email` e `password`.
3. A API valida as credenciais (comparando o hash SHA-256) e retorna um token JWT.
4. O token deve ser enviado no header `Authorization: Bearer {token}` nas requisições protegidas.
5. Os endpoints de clientes isolam os dados por `userId` extraído do claim `sub` do JWT.

## Banco de Dados (MongoDB)

### Collections

#### `users`

| Campo | Tipo BSON | Descrição |
|---|---|---|
| `_id` | ObjectId | Identificador único |
| `name` | string | Nome do usuário |
| `email` | string | E-mail do usuário |
| `password_hash` | string | Senha criptografada em SHA-256 (Base64) |

#### `customers`

| Campo | Tipo BSON | Descrição |
|---|---|---|
| `_id` | ObjectId | Identificador único |
| `name` | string | Nome do cliente |
| `email` | string | E-mail do cliente |
| `phone` | string | Telefone |
| `document` | string | CPF / CNPJ |
| `status` | int | Status do cliente (padrão: `1` = ativo) |
| `created_at` | DateTime | Data de criação (UTC) |
| `updated_at` | DateTime? | Data da última atualização (UTC) |
| `user_id` | ObjectId | Referência ao usuário dono do registro |

> **Isolamento por usuário:** cada usuário só acessa e manipula os seus próprios clientes.

## Seed de Dados (DatabaseRunner)

Na inicialização da aplicação, o `DatabaseRunner` cria automaticamente um usuário administrador padrão caso ele não exista no banco:

| Campo | Valor |
|---|---|
| Nome | `Administrador COTI` |
| E-mail | `administrador@cotiinformatica.com.br` |
| Senha | `Coti@2026` |

## Autenticação

A autenticação da API é baseada em **JWT Bearer Token** com algoritmo **HMAC-SHA256**.

### Claims do token JWT

| Claim | Conteúdo |
|---|---|
| `sub` | ID do usuário (ObjectId) |
| `email` | E-mail do usuário |
| `name` | Nome do usuário |

Após autenticar um usuário, a API retorna um `accessToken` que deve ser enviado no header das requisições protegidas:

```http
Authorization: Bearer {seu_token_jwt}
```

## Swagger

A API possui documentação interativa via Swagger/OpenAPI, configurada com suporte a autenticação JWT.

Após iniciar a aplicação, acesse:

```txt
https://localhost:{porta}/swagger
```

No Swagger, clique em **Authorize** e informe apenas o token JWT (sem o prefixo `Bearer`).

## Endpoints da API

A API possui dois controllers:

- `UsersController` — público, sem autenticação
- `CustomersController` — protegido, exige JWT

---

# Users

Controller responsável pela autenticação dos usuários.

Rota base:

```http
/api/users
```

## Autenticar usuário

```http
POST /api/users/auth
```

### Descrição

Valida as credenciais do usuário (email + senha) e retorna um token JWT.

### Request Body

```json
{
  "email": "administrador@cotiinformatica.com.br",
  "password": "Coti@2026"
}
```

### Resposta de sucesso — `200 OK`

```json
{
  "accessToken": "<jwt-token>",
  "tokenType": "Bearer",
  "user": {
    "id": "<objectId>",
    "name": "Administrador COTI",
    "email": "administrador@cotiinformatica.com.br"
  }
}
```

### Respostas de erro

| Status | Descrição |
|---|---|
| `401 Unauthorized` | E-mail não encontrado ou senha incorreta |

---

# Customers

Controller responsável pelo gerenciamento dos clientes.

> **Todos os endpoints exigem autenticação JWT.**

Rota base:

```http
/api/customers
```

## Criar cliente

```http
POST /api/customers/create
```

### Descrição

Cria um novo cliente vinculado ao usuário autenticado. O e-mail deve ser único por usuário.

### Request Body

```json
{
  "name": "João Silva",
  "email": "joao@email.com",
  "phone": "(11) 99999-0000",
  "document": "123.456.789-00",
  "status": 1
}
```

### Respostas

| Status | Descrição |
|---|---|
| `201 Created` | Cliente criado com sucesso |
| `400 Bad Request` | Nome ou e-mail ausentes |
| `401 Unauthorized` | Token inválido ou ausente |
| `409 Conflict` | E-mail já cadastrado para este usuário |

---

## Atualizar cliente

```http
PUT /api/customers/update/{id}
```

### Parâmetros de rota

| Nome | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | string (ObjectId) | sim | Identificador do cliente |

### Descrição

Atualiza os dados de um cliente existente pertencente ao usuário autenticado. Verifica duplicidade de e-mail entre os clientes do mesmo usuário.

### Respostas

| Status | Descrição |
|---|---|
| `200 OK` | Cliente atualizado com sucesso |
| `400 Bad Request` | ID, nome ou e-mail ausentes |
| `401 Unauthorized` | Token inválido ou ausente |
| `404 Not Found` | Cliente não encontrado |
| `409 Conflict` | E-mail já usado por outro cliente |

---

## Excluir cliente

```http
DELETE /api/customers/delete/{id}
```

### Parâmetros de rota

| Nome | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | string (ObjectId) | sim | Identificador do cliente |

### Descrição

Remove permanentemente um cliente da base de dados. Somente o dono do registro pode excluí-lo.

### Respostas

| Status | Descrição |
|---|---|
| `200 OK` | `{ "message": "Cliente excluído com sucesso." }` |
| `400 Bad Request` | ID ausente |
| `401 Unauthorized` | Token inválido ou ausente |
| `404 Not Found` | Cliente não encontrado |

---

## Listar clientes

```http
GET /api/customers/list
```

### Query parameters

| Nome | Tipo | Obrigatório | Padrão | Máximo | Descrição |
|---|---|---|---|---|---|
| `page` | int | não | `1` | — | Número da página |
| `limit` | int | não | `10` | `100` | Registros por página |

### Descrição

Retorna a lista paginada de clientes do usuário autenticado, ordenada pela data de criação (mais recentes primeiro).

### Exemplo de requisição

```http
GET /api/customers/list?page=1&limit=10
```

### Resposta de sucesso — `200 OK`

```json
{
  "page": 1,
  "limit": 10,
  "total": 42,
  "totalPages": 5,
  "data": [{ "..." }]
}
```

---

## Consultar cliente por ID

```http
GET /api/customers/find/{id}
```

### Parâmetros de rota

| Nome | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | string (ObjectId) | sim | Identificador do cliente |

### Descrição

Retorna os dados completos de um cliente específico pertencente ao usuário autenticado.

### Respostas

| Status | Descrição |
|---|---|
| `200 OK` | Dados do cliente |
| `400 Bad Request` | ID ausente |
| `401 Unauthorized` | Token inválido ou ausente |
| `404 Not Found` | Cliente não encontrado |

## Configuração do projeto

### appsettings.json

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "customerapp"
  },
  "Jwt": {
    "Key": "<chave-secreta-minimo-32-caracteres>",
    "Issuer": "CustomerApp",
    "Audience": "CustomerAppUsers",
    "ExpirationMinutes": 60
  }
}
```

> **Atenção:** as chaves de configuração do MongoDB no `appsettings.json` são `MongoDb` (com `b` minúsculo), conforme mapeado no `Program.cs`.

### Variáveis de ambiente recomendadas para produção

Para ambientes de produção, não armazene credenciais diretamente no `appsettings.json`. Utilize variáveis de ambiente ou secrets:

```txt
MongoDb__ConnectionString
MongoDb__DatabaseName
Jwt__Key
Jwt__Issuer
Jwt__Audience
Jwt__ExpirationMinutes
```

## Como executar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [MongoDB](https://www.mongodb.com/try/download/community) local ou connection string do MongoDB Atlas

### Passos

```bash
# Clone o repositório
git clone https://github.com/fernandoborel/CustomerApp.git
cd CustomerApp

# Configure o appsettings.json com as credenciais do MongoDB e a chave JWT

# Execute a aplicação
dotnet run --project CustomerApp.API
```

Na inicialização, o usuário administrador padrão é criado automaticamente no banco.

## Serviços

### `MongoDbService`
Singleton. Gerencia a conexão com o MongoDB e expõe as collections `users` e `customers`.

### `JwtService`
Scoped. Gera tokens JWT assinados com HMAC-SHA256, incluindo as claims `sub`, `email` e `name`.

### `PasswordService`
Scoped. Realiza o hash de senhas com SHA-256 (Base64) e a comparação segura durante a autenticação.

### `DatabaseRunner`
Scoped. Executado uma vez na inicialização da aplicação para garantir a existência do usuário administrador padrão.

---

## Autor

Desenvolvido por **Fernando Borel**
Repositório: [github.com/fernandoborel/CustomerApp](https://github.com/fernandoborel/CustomerApp)Configurações necessárias:

1. Criar um cluster no MongoDB Atlas.
2. Criar o banco de dados da aplicação.
3. Configurar usuário e senha de acesso.
4. Liberar acesso de rede para o ambiente Azure.
5. Configurar a connection string no Azure App Service.

Exemplo de connection string:

```txt
mongodb+srv://usuario:senha@cluster.mongodb.net/CustomerAppDB
```

## Azure

A aplicação pode ser publicada no **Azure App Service**.

Configurações recomendadas:

- Runtime compatível com ASP.NET 10
- Variáveis de ambiente configuradas no App Service
- Application Settings para JWT e MongoDB
- Pipeline de CI/CD configurado para build e deploy automático

## CI/CD

O projeto utiliza pipeline de CI/CD para automatizar:

1. Restauração de pacotes
2. Build da aplicação
3. Execução de testes
4. Publicação do artefato
5. Deploy no Azure App Service

## Exemplo de pipeline com GitHub Actions

```yaml
name: CustomerApp API CI/CD

on:
  push:
    branches:
      - main

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout do código
        uses: actions/checkout@v4

      - name: Configurar .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restaurar dependências
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Executar testes
        run: dotnet test --configuration Release --no-build

      - name: Publicar aplicação
        run: dotnet publish --configuration Release --output ./publish

      - name: Deploy no Azure App Service
        uses: azure/webapps-deploy@v3
        with:
          app-name: customerapp-api
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ./publish
```

## Estrutura sugerida do projeto

```txt
CustomerApp/
├── src/
│   └── CustomerApp.API/
│       ├── Controllers/
│       │   ├── CustomersController.cs
│       │   └── UsersController.cs
│       ├── Models/
│       ├── Services/
│       ├── Repositories/
│       ├── Settings/
│       ├── Program.cs
│       └── appsettings.json
├── tests/
│   └── CustomerApp.Tests/
├── README.md
└── CustomerApp.sln
```

## Executando o projeto localmente

Clone o repositório:

```bash
git clone https://github.com/sua-organizacao/customerapp.git
```

Acesse a pasta do projeto:

```bash
cd customerapp
```

Restaure as dependências:

```bash
dotnet restore
```

Execute a aplicação:

```bash
dotnet run --project src/CustomerApp.API
```

Acesse o Swagger:

```txt
https://localhost:{porta}/swagger
```

## Segurança

Recomendações de segurança para o projeto:

- Não versionar connection strings reais.
- Não versionar chaves JWT.
- Utilizar Azure Key Vault ou App Settings para secrets.
- Configurar CORS apenas para domínios permitidos.
- Utilizar HTTPS em todos os ambientes.
- Validar expiração e assinatura dos tokens JWT.
- Proteger endpoints sensíveis com `[Authorize]`.

## Status do projeto

Projeto em desenvolvimento.

## Licença

Este projeto é de uso interno.
