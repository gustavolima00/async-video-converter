## Rodando migrações

As migrações do banco são gerenciadas pelo projeto na pasta migrations. Ele utiliza a biblioteca DbUp para gerenciar migrações.

### Pré-requisitos

- .NET SDK
- PostgreSQL
- Docker (opcional)

### Executando as Migrações

Para executar as migrações localmente:

1. Garanta que o PostgreSQL esteja rodando.
2. Execute o comando a seguir, substituindo os valores de conexão se necessário:

   ```bash
   cd migrations
   dotnet build
   dotnet run --project Migrations/Migrations.csproj "Host=localhost;Port=5432;Database=file_converter;Username=postgres;Password=postgres"
   ```

### Executando com Docker (Opcional)

Para construir e executar o projeto com Docker:

1. Construa a imagem Docker:

   ```bash
   docker build -f Migration.Dockerfile -t migrations .
   ```

2. Execute o container, substituindo os valores de conexão se necessário:

   ```bash
   docker run --rm migrations "Host=host;Port=5432;Database=file_converter;Username=postgres;Password=postgres"
   ```
