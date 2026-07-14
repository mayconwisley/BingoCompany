# Bingo Company

MVP de bingo corporativo com uma cartela por participante, rodadas progressivas, etapas de prêmio e sincronização em tempo real.

## Projetos

- `BingoCompany.Domain`: entidades e regras de negócio.
- `BingoCompany.Application`: geração segura de cartelas/sequências e avaliação dos padrões.
- `BingoCompany.Infrastructure`: EF Core, PostgreSQL e migrations.
- `BingoCompany.Api`: REST, SignalR e Swagger.
- `BingoCompany.Frontend`: React/Vite para a cartela do participante.
- `BingoCompany.Tests`: testes unitários e integração de persistência.

## Executar

```powershell
dotnet run --project .\BingoCompany.Api
cd .\BingoCompany.Frontend
npm install
npm run dev
```

A API aplica as migrations do PostgreSQL automaticamente e disponibiliza Swagger em `/swagger`.
Configure `DBBingoUser` e `DBBingoPass` antes de iniciar a API. Host, porta e banco ficam em `BingoCompany.Api/appsettings.json` e podem ser substituídos por `ConnectionStrings__Bingo`.

## Demonstração com Docker Compose

O Docker Compose é destinado apenas à apresentação rápida do sistema; o desenvolvimento continua sendo executado normalmente no Windows.

Prepare as credenciais locais antes da primeira execução. O arquivo `.env` não é versionado.

```powershell
Copy-Item .env.example .env
```

Edite `.env` e informe uma senha de banco e uma chave JWT aleatórias (com pelo menos 32 caracteres). O ambiente do Compose é deliberadamente `Development`, porque a demonstração usa HTTP local e disponibiliza Swagger. Não o use como configuração de produção.

```powershell
docker compose up --build
```

Abra [http://localhost:8080](http://localhost:8080). A API, o SignalR e o Swagger ficam acessíveis pela mesma origem, em `http://localhost:8080/api`, `http://localhost:8080/hubs/bingo` e [http://localhost:8080/swagger](http://localhost:8080/swagger), respectivamente.

Os dados do PostgreSQL permanecem no volume `bingo-postgres-data`. Para encerrar a apresentação, execute:

```powershell
docker compose down
```

Para reiniciar a demonstração sem dados anteriores, execute `docker compose down -v`.

## Fluxo básico da API

1. `POST /api/events`
2. `POST /api/events/{eventId}/registration/open`
3. `POST /api/events/{eventId}/participants`
4. `POST /api/events/{eventId}/rounds`
5. `POST /api/events/{eventId}/rounds/{roundId}/start`
6. `POST /api/events/{eventId}/rounds/{roundId}/draw`

Cada chamada de sorteio publica `NumberDrawn` no hub `/hubs/bingo`. Quando uma cartela elegível completa a etapa ativa, a API publica `WinningCardDetected` sem expor o participante.
