# CustomerApp API

API REST desenvolvida em **ASP.NET 10** para gerenciamento de clientes, autenticação de usuários com **JWT**, persistência de dados em **MongoDB** e pipeline de **CI/CD com Azure** integrado ao **MongoDB Atlas**.

## Tecnologias utilizadas

- ASP.NET 10
- C#
- MongoDB
- MongoDB Atlas
- JWT Bearer Authentication
- Swagger / OpenAPI
- Azure DevOps ou GitHub Actions
- Azure App Service
- CI/CD

## Objetivo do projeto

O projeto **CustomerApp** tem como objetivo fornecer uma API para cadastro, consulta, atualização e exclusão de clientes, além de autenticação de usuários por meio de tokens JWT.

A API foi projetada para ser publicada em ambiente cloud utilizando Azure, com banco de dados hospedado no MongoDB Atlas.

## Arquitetura geral

A aplicação segue uma estrutura baseada em API REST, utilizando controllers ASP.NET para expor os endpoints HTTP.

Fluxo principal:

1. O usuário realiza autenticação no endpoint de login.
2. A API valida as credenciais.
3. Em caso de sucesso, a API retorna um token JWT.
4. O token deve ser enviado nas próximas requisições protegidas.
5. Os dados dos clientes são armazenados no MongoDB Atlas.
6. O deploy da aplicação é realizado automaticamente por pipeline de CI/CD no Azure.

## Autenticação

A autenticação da API é baseada em **JWT Bearer Token**.

Após autenticar um usuário, a API retorna um token JWT que deve ser enviado no header das requisições protegidas.

Exemplo de header:

```http
Authorization: Bearer {seu_token_jwt}
```

## Swagger

A API possui documentação interativa via Swagger/OpenAPI.

Em ambiente de desenvolvimento, o Swagger pode ser acessado em:

```txt
https://localhost:{porta}/swagger
```

No Swagger, é possível testar os endpoints e informar o token JWT no botão **Authorize**.

## Endpoints da API

A API possui dois controllers principais:

- `CustomersController`
- `UsersController`

---

# Customers

Controller responsável pelo gerenciamento dos clientes.

Rota base:

```http
/api/customers
```

## Criar cliente

Endpoint para cadastro de cliente.

```http
POST /api/customers/create
```

### Descrição

Cria um novo cliente na base de dados.

### Resposta de sucesso

```http
200 OK
```

---

## Atualizar cliente

Endpoint para atualização de cliente.

```http
PUT /api/customers/update/{id}
```

### Parâmetros

| Nome | Tipo | Local | Obrigatório | Descrição |
|---|---|---|---|---|
| id | string | rota | sim | Identificador do cliente |

### Descrição

Atualiza os dados de um cliente existente com base no ID informado.

### Exemplo de requisição

```http
PUT /api/customers/update/65f1a7c9e4b0a12345678901
```

### Resposta de sucesso

```http
200 OK
```

---

## Excluir cliente

Endpoint para exclusão de cliente.

```http
DELETE /api/customers/delete/{id}
```

### Parâmetros

| Nome | Tipo | Local | Obrigatório | Descrição |
|---|---|---|---|---|
| id | string | rota | sim | Identificador do cliente |

### Descrição

Remove um cliente da base de dados com base no ID informado.

### Exemplo de requisição

```http
DELETE /api/customers/delete/65f1a7c9e4b0a12345678901
```

### Resposta de sucesso

```http
200 OK
```

---

## Listar clientes

Endpoint para consulta paginada de clientes.

```http
GET /api/customers/list
```

### Query parameters

| Nome | Tipo | Obrigatório | Valor padrão | Descrição |
|---|---|---|---|---|
| page | int | não | 1 | Número da página |
| limit | int | não | 10 | Quantidade de registros por página |

### Descrição

Retorna uma lista paginada de clientes cadastrados.

### Exemplo de requisição

```http
GET /api/customers/list?page=1&limit=10
```

### Resposta de sucesso

```http
200 OK
```

---

## Consultar cliente por ID

Endpoint para consultar um cliente através do ID.

```http
GET /api/customers/find/{id}
```

### Parâmetros

| Nome | Tipo | Local | Obrigatório | Descrição |
|---|---|---|---|---|
| id | string | rota | sim | Identificador do cliente |

### Descrição

Retorna os dados de um cliente específico com base no ID informado.

### Exemplo de requisição

```http
GET /api/customers/find/65f1a7c9e4b0a12345678901
```

### Resposta de sucesso

```http
200 OK
```

---

# Users

Controller responsável pela autenticação dos usuários.

Rota base:

```http
/api/users
```

## Autenticar usuário

Endpoint para autenticação dos usuários.

```http
POST /api/users/auth
```

### Descrição

Valida as credenciais do usuário e retorna um token JWT para acesso aos endpoints protegidos da API.

### Exemplo de resposta esperada

```json
{
  "accessToken": "token.jwt.gerado",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

### Resposta de sucesso

```http
200 OK
```

---

# Configuração do projeto

## appsettings.json

Exemplo de configuração esperada:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb+srv://usuario:senha@cluster.mongodb.net",
    "DatabaseName": "CustomerAppDB"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-aqui",
    "Issuer": "CustomerApp",
    "Audience": "CustomerAppUsers",
    "ExpiresInMinutes": 60
  }
}
```

## Variáveis de ambiente recomendadas

Para ambientes de produção, recomenda-se não armazenar credenciais diretamente no `appsettings.json`.

Utilize variáveis de ambiente ou secrets do Azure:

```txt
MongoDB__ConnectionString
MongoDB__DatabaseName
Jwt__Key
Jwt__Issuer
Jwt__Audience
Jwt__ExpiresInMinutes
```

## MongoDB Atlas

O banco de dados da aplicação é hospedado no **MongoDB Atlas**.

Configurações necessárias:

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
