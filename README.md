# student-events-manager
Desafio Técnico - Desenvolvedor FullStack

Este projeto é uma solução completa para sincronização de estudantes e eventos utilizando a API do Microsoft Graph, Backend em .NET 9 e Frontend em React.

## 🚀 Tecnologias Utilizadas

- **Backend:** .NET 9, Entity Framework Core, Hangfire, xUnit.
- **Frontend:** React, TypeScript, TailwindCSS, Axios.
- **Banco de Dados:** SQL Server.
- **Integração:** Microsoft Graph API.

---

## 📋 Pré-requisitos

Certifique-se de ter instalado em sua máquina:
- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 ou superior)
- [SQL Server](https://www.microsoft.com/sql-server/) (LocalDB ou Docker)

---

## ⚙️ Configuração e Execução

### 1. Backend (.NET API)

1. Navegue até a pasta da API:
   ```bash
   cd src/backend
   ```

2. Configure o acesso ao Banco de Dados e Microsoft Graph no `appsettings.json` (ou via User Secrets):
   > O projeto já está configurado para criar o banco automaticamente ao iniciar.
   > **⚠️ Importante:** Você deve alterar as credenciais do **Microsoft Graph** (`ClientId`, `Secret`, `TenantId`) no arquivo `appsettings.json` para credenciais válidas.

3. Restaure os pacotes e rode a aplicação:
   ```bash
   dotnet restore
   dotnet run --project StudentEvent.API
   ```

4. A API estará disponível em: `https://localhost:57851`
   - **Swagger:** https://localhost:57851/swagger
   - **Hangfire Dashboard:** https://localhost:57851/hangfire

   > **⚠️ Importante:** A porta `57851` pode variar dependendo do ambiente. Verifique a saída do console (`Now listening on...`) e, se for diferente, atualize o arquivo `src/frontend/frontend/src/config.ts`.

### 2. Frontend (React)

1. Em um novo terminal, navegue até a pasta do frontend:
   ```bash
   cd src/frontend/frontend
   ```

2. Instale as dependências:
   ```bash
   npm install
   ```

3. Inicie o servidor de desenvolvimento:
   ```bash
   npm run dev
   ```

4. Acesse a aplicação em: http://localhost:5173

   > **⚠️ Importante:** Se o Frontend iniciar em uma porta diferente de `5173`, atualize a chave `FrontendUrl` no arquivo `appsettings.json` da API para evitar erros de CORS.

---

## 🔐 Acesso ao Sistema

Para acessar o Dashboard, utilize o usuário administrador criado automaticamente pelo Seed:

- **E-mail:** `admin@teste.com`
- **Senha:** `123456`

---

## 🔄 Sincronização de Dados (Hangfire)

O sistema utiliza o **Hangfire** para realizar a sincronização de dados em segundo plano.
- Um job recorrente ("sync-graph-data") roda a cada **5 minutos**.
- Ele consulta a API do **Microsoft Graph** para buscar estudantes e seus eventos de calendário.
- Os dados são processados e salvos no banco de dados local (**SQL Server**).
- O Frontend consome apenas os dados já persistidos no banco local, garantindo performance e disponibilidade.

---

## 🧪 Rodando os Testes

Para garantir que tudo está funcionando corretamente, execute os testes unitários do backend:

```bash
cd src/backend
dotnet test
```
