# Sebo-Andy-API
Projeto pessoal API REST de gerenciamento de sebo(Livraria de usados) desenvolvida como objeto de estudo.

# Tecnologias
- C# / .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT para autenticação
- BCrypt para hash de senhas

## Sobre o projeto
CRUD completo de livros, usuários e categorias com controle de navegação baseado em Roles (Admin, Funcionario e Cliente)

Endpoints que fazem adição, remoção ou quaisquer outras alterações no banco de dados necessitam um login de um usuário com Role de Admin.
Retornando DTOs para controlar o que é exposto ao usuário/consumidor da API.

### Pré-requisitos
- .NET 10 SDK
- SQL Server Express

### Primeiro Acesso
Apenas um usuário como Role de Admin pode fazer cadastro de novos usuários, sendo então necessário a injeção manual diretamente no banco de dados de um usuário administrador para fazer o uso das principais funções da API.

### Passos
1. Clone o repositório
2. Ajuste a string de conexão em `appsettings.json` para sua instância do SQL Server
3. Execute as migrations:
```
   dotnet ef database update
```
4. Rode o projeto:
```
   dotnet run
```
5. Acesse o Swagger em `https://localhost:{porta}/swagger`

### Autenticação no Swagger
1. Use o endpoint `POST /api/Login` com email e senha
2. Copie o token retornado
3. Clique em **Authorize** no topo do Swagger
4. Digite `Bearer {seu token}` e confirme