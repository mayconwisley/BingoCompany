# Bingo Company

MVP de bingo corporativo com uma cartela por participante, rodadas progressivas, etapas de prêmio e sincronização em tempo real.

## Projetos

- `BingoCompany.Domain`: entidades e regras de negócio.
- `BingoCompany.Application`: geração segura de cartelas/sequências e avaliação dos padrões.
- `BingoCompany.Infrastructure`: EF Core e SQLite.
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

A API cria o banco SQLite `bingo.db` automaticamente e disponibiliza Swagger em `/swagger`.

## Fluxo básico da API

1. `POST /api/events`
2. `POST /api/events/{eventId}/registration/open`
3. `POST /api/events/{eventId}/participants`
4. `POST /api/events/{eventId}/rounds`
5. `POST /api/events/{eventId}/rounds/{roundId}/start`
6. `POST /api/events/{eventId}/rounds/{roundId}/draw`

Cada chamada de sorteio publica `NumberDrawn` no hub `/hubs/bingo`. Quando uma cartela elegível completa a etapa ativa, a API publica `WinningCardDetected` sem expor o participante.
