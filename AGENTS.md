# Bingo Company — Guia para agentes

## Objetivo do produto

O Bingo Company é um sistema de bingo corporativo em tempo real. Participantes usam cartelas digitais ou impressas; o operador controla o sorteio; o telão apresenta as pedras, o suspense e o resultado. O backend é a única fonte de verdade para cartelas, pedras, marcações, vencedores e auditoria.

## Estrutura da solução

| Projeto | Responsabilidade |
| --- | --- |
| `BingoCompany.Domain` | Entidades, enums e invariantes de negócio. Não depende de outros projetos. |
| `BingoCompany.Application` | Regras puras: geração de cartelas, sequência segura e avaliação de padrões. |
| `BingoCompany.Infrastructure` | EF Core, SQLite e mapeamento de persistência. |
| `BingoCompany.Api` | Endpoints REST, SignalR, serialização e inicialização do banco. |
| `BingoCompany.Frontend` | React + TypeScript + Vite para participante, administração, operador, telão e auditoria. |
| `BingoCompany.Tests` | Testes unitários e de integração do backend. |

## Regras de domínio essenciais

- A cartela pertence ao evento, não à rodada.
- Um evento pode conter várias rodadas; cada rodada tem nova sequência de 75 pedras e novas marcações.
- Uma rodada pode conter várias etapas de prêmio progressivas. A sequência não reinicia entre etapas da mesma rodada.
- Somente cartelas presentes no snapshot `RoundEligibleCard` concorrem em uma rodada.
- Nunca repita pedra. Gere toda a sequência com `RandomNumberGenerator` e Fisher–Yates; publique somente o hash antes da revelação.
- No modo `Automatic`, números sorteados contam como marcados. Nos modos manuais, somente `CardMark` persistido pelo backend conta para vencer.
- O frontend não decide vencedores, empates, elegibilidade nem pedras sorteadas.
- O nome do vencedor só deve ser enviado ao telão após a ação de revelação. Antes disso, enviar apenas contagens agregadas.

## Backend

- Mantenha as entidades e regras de transição de estado no domínio; controladores apenas orquestram requisições e respostas.
- Use `DateTimeOffset` para registros auditáveis.
- Todas as alterações de estado relevantes devem ser persistidas antes de publicar eventos SignalR.
- Preserve o contrato de eventos SignalR: `RoundStarted`, `NumberDrawn`, `WinningCardDetected` e `WinnerRevealed`.
- Grupos SignalR usam `event:{eventId}` e `round:{roundId}`.
- A API usa SQLite no MVP. Ao modificar o modelo persistido, prefira criar migrations EF Core. Não apague o arquivo `bingo.db` para resolver problemas de esquema.
- Não exponha na API pública CPF, matrícula, e-mail, token de acesso ou dados internos de colaboradores.

## Frontend

O frontend segue organização por responsabilidade:

```text
src/
├── app/                 # composição de rotas
├── pages/               # telas, sem regras de acesso HTTP
├── features/bingo/      # API, tipos, hooks e componentes do domínio de bingo
├── shared/api/          # cliente HTTP e erros comuns
├── shared/hooks/        # hooks genéricos reutilizáveis
├── shared/ui/           # componentes visuais genéricos
└── test/                # configuração de testes
```

- Uma página compõe componentes e delega rede para `features/bingo/bingoApi.ts`.
- Componentes devem receber dados e callbacks por props; evite chamadas HTTP dentro de componentes reutilizáveis.
- Todo componente interativo deve ter rótulos acessíveis e poder ser renderizado isoladamente em teste.
- Use `useLiveBingo` exclusivamente para sincronização SignalR e recarregue o estado da API após reconexão.
- Não misture rotas, contratos HTTP e elementos visuais em um único arquivo.

## Testes e validação

Execute a partir da raiz:

```powershell
dotnet test BingoCompany.slnx --no-restore -v minimal
dotnet build BingoCompany.Api/BingoCompany.Api.csproj --no-restore -v minimal
```

Execute no frontend:

```powershell
cd BingoCompany.Frontend
npm test
npm run build
```

Ao alterar uma regra de negócio, cubra o caso no projeto `BingoCompany.Tests`. Ao criar ou alterar um componente, adicione um teste próximo ao arquivo (`*.test.tsx`) usando Testing Library e Vitest.

## Convenções de mudança

- Não use `Random` ou `Random.Shared` para sorteios ou desempates.
- Não confie em IDs/tokens recebidos pelo cliente sem verificar evento, cartela e elegibilidade no backend.
- Não faça alterações destrutivas no banco ou no repositório sem autorização explícita.
- Preserve as alterações do usuário que não sejam relacionadas à tarefa.
- Ao adicionar dependências de frontend, atualize `package.json` e `package-lock.json` juntos.
- Ao alterar endpoints, atualize `bingoApi.ts`, tipos relacionados e testes de tela/componentes afetados.
