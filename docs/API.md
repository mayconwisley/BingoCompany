# Referência da API e SignalR

Base local: `http://localhost:5138`. Em Docker Compose, use `http://localhost:8080`. Em produção, a API é servida abaixo de `/bingo/api`.

As respostas usam JSON com enums serializados como texto. A API retorna `204 No Content` quando uma operação sem corpo é concluída. Mensagens de validação, conflito ou autorização podem ser devolvidas como texto simples.

## Autenticação

`POST /api/auth/register` cria a empresa e inicia a sessão. `POST /api/auth/login` inicia uma sessão existente. Ambos recebem:

```json
{
  "companyName": "Empresa Exemplo",
  "name": "Organizador",
  "email": "organizador@empresa.test",
  "password": "uma-senha-com-12-ou-mais-caracteres"
}
```

No login, envie apenas `email` e `password`. A resposta é `{ "name": "...", "companyName": "..." }` e a sessão é mantida em cookie `HttpOnly`; o token JWT não é retornado ao JavaScript. Para encerrar a sessão, use `POST /api/auth/logout`.

As rotas em `/api/events` exigem a sessão e são limitadas à empresa do usuário, exceto `GET /api/events/{eventId}/cards/{cardCode}/state`, que é a consulta pública da cartela digital pelo seu link. Para métodos que alteram estado autenticado, o navegador precisa enviar uma origem presente em `Cors:AllowedOrigins`; isso protege o cookie contra requisições entre sites.

## Rotas públicas

| Método e rota | Descrição |
| --- | --- |
| `GET /healthz` | Saúde da aplicação. |
| `GET /api/application/info` | Nome, descrição e versão da aplicação. |
| `GET /api/public/events/{code}` | Estado público do evento, rodada selecionada, pedras, etapas, estatísticas agregadas e vencedor já revelado. Em desempate revelado, inclui todos os participantes, suas pedras e a indicação do vencedor. |
| `POST /api/public/events/{code}/join` | Inscreve um participante e gera cartela digital. Limite: 20 requisições/minuto por IP. |
| `GET /api/public/events/{code}/audit` | Histórico público, cartelas, rodadas, resultados e auditoria. A sequência completa somente é exposta ao fim da rodada. |

O corpo de inscrição pública é:

```json
{
  "name": "Ana Participante",
  "type": "Employee",
  "employeeRegistration": "opcional",
  "responsibleEmployeeName": "necessário para familiar/convidado quando aplicável"
}
```

`type` aceita `Employee`, `FamilyMember` e `Guest`. Informações internas de colaborador não são devolvidas pelas consultas públicas.

## Eventos e cartelas

| Método e rota | Descrição |
| --- | --- |
| `GET /api/events?page=1&pageSize=12` | Lista paginada dos eventos da empresa. `pageSize` é limitado a 100. |
| `POST /api/events` | Cria evento. |
| `GET /api/events/{eventId}` | Detalhes administrativos de um evento. |
| `POST /api/events/{eventId}/registration/open` | Abre inscrições de um evento em preparação. |
| `POST /api/events/{eventId}/participants` | Inscreve participante pelo painel administrativo. |
| `POST /api/events/{eventId}/cards/printed` | Gera de 1 a 1.000 cartelas impressas. |
| `GET /api/events/{eventId}/cards/printed` | Lista cartelas impressas e seu QR Code. |
| `POST /api/events/{eventId}/cards/{cardCode}/assign` | Associa cartela impressa a um participante. |
| `POST /api/events/{eventId}/cards/{cardCode}/activate` | Ativa cartela impressa associada. |
| `GET /api/events/{eventId}/cards/{cardCode}/state` | Estado da cartela, marcações, pedras e possibilidade de renovação. |
| `POST /api/events/{eventId}/cards/{cardCode}/marks` | Registra uma marcação manual válida. |
| `POST /api/events/{eventId}/cards/{cardCode}/next` | Gera a próxima cartela digital quando a anterior puder ser renovada. |
| `POST /api/events/{eventId}/finish` | Finaliza o evento após todas as rodadas. |

Criação de evento:

```json
{
  "name": "Festa da empresa",
  "cardsPerParticipant": 1,
  "markingMode": "Automatic"
}
```

`markingMode` aceita `Automatic`, `ManualRequired` ou `AssistedManual`. A criação de cartelas impressas recebe `{ "quantity": 10 }` e permite de 1 a 1.000 cartelas por requisição; novos lotes podem ser criados para o mesmo evento. A associação recebe `{ "participantId": "GUID" }`; a marcação recebe `{ "number": 42 }`.

## Rodadas e prêmios

| Método e rota | Descrição |
| --- | --- |
| `POST /api/events/{eventId}/rounds` | Cria uma rodada pronta. |
| `PUT /api/events/{eventId}/rounds/{roundId}` | Edita uma rodada ainda não iniciada. |
| `POST /api/events/{eventId}/rounds/{roundId}/start` | Congela as cartelas elegíveis, gera a sequência segura e inicia a primeira etapa. |
| `POST /api/events/{eventId}/rounds/{roundId}/cancel` | Cancela uma rodada que ainda está em sorteio, sem concluir a etapa atual. |
| `POST /api/events/{eventId}/rounds/{roundId}/draw` | Sorteia a próxima pedra. |
| `POST /api/events/{eventId}/rounds/{roundId}/reveal` | Revela o vencedor ou conclui o desempate e aguarda a confirmação da entrega. |
| `POST /api/events/{eventId}/rounds/{roundId}/prize-delivered` | Registra a entrega, conclui a etapa e avança a rodada. |
| `POST /api/events/{eventId}/rounds/{roundId}/prize-declined` | Registra que o vencedor não retirou o prêmio e retoma o sorteio na mesma etapa. |
| `POST /api/events/{eventId}/rounds/{roundId}/winner-presentation/close` | Fecha a apresentação após a confirmação da entrega para liberar a próxima pedra ou etapa. |

Corpo para criar ou editar uma rodada:

```json
{
  "name": "Rodada principal",
  "stages": [
    { "sequence": 1, "prizeName": "Vale-presente", "pattern": "HorizontalLine" },
    { "sequence": 2, "prizeName": "Prêmio final", "pattern": "FullCard", "prizeImageDataUrl": null }
  ]
}
```

As etapas são ordenadas pela prioridade da regra, não pelo campo `sequence` recebido: colunas B, I, N, G e O; quatro cantos; diagonais B e O; uma linha; X; T; cruz; duas linhas; moldura; e cartela cheia. Não repita um padrão na mesma rodada. `prizeImageDataUrl`, quando enviado, deve ser JPEG, PNG ou WebP em data URL de até 2 MB.

Padrões aceitos: `HorizontalLine`, `TwoHorizontalLines`, `FourCorners`, `FullCard`, `BDiagonal`, `ODiagonal`, `BColumn`, `IColumn`, `NColumn`, `GColumn`, `OColumn`, `XPattern`, `TPattern`, `Frame` e `Cross`.

## Estados e regras de transição

| Recurso | Estados |
| --- | --- |
| Evento | `Draft` → `RegistrationOpen` → `Running` → `Finished` |
| Rodada | `Ready` → `Drawing` → `WinnerDetected` / `TieBreaker` → `Finished` |

Ao iniciar uma rodada, somente as cartelas ativas e associadas existentes naquele instante entram no snapshot de elegibilidade. Cada rodada usa uma sequência de 75 pedras gerada com criptografia segura, sem repetição. O hash é divulgado no início e a sequência inteira é disponibilizada na auditoria ao final.

Em marcação automática, toda pedra sorteada é considerada marcada. Nos modos manuais, apenas `CardMark` persistida pela API vale para a detecção do vencedor. O nome do vencedor só aparece na API pública e no telão após a revelação.

Após a revelação, o operador confirma se o prêmio foi entregue. A confirmação conclui a etapa, mas o telão permanece na apresentação até `winner-presentation/close`. Se o vencedor não retirar o prêmio, a cartela é excluída apenas daquela etapa, o telão retorna ao sorteio e a mesma regra permanece ativa para encontrar outro vencedor.

Em caso de empate, o backend atribui a cada cartela uma posição única de desempate por embaralhamento criptograficamente seguro. As posições vão de `1` até a quantidade de cartelas empatadas, portanto o desempate não fica limitado às 75 pedras do bingo.

O núcleo possui teste de regressão com 1.000 cartelas elegíveis e 1.000 candidatos simultâneos. Isso valida a regra, a detecção e o desempate na aplicação; a carga de PostgreSQL, rede e SignalR deve ser medida no ambiente de implantação conforme o número esperado de conexões.

## SignalR

Conecte-se a `/hubs/bingo`. Depois de iniciar a conexão, invoque `JoinEvent(eventId)` e, se necessário, `JoinRound(roundId)`. A aplicação usa os grupos `event:{eventId}` e `round:{roundId}`.

| Evento | Conteúdo principal |
| --- | --- |
| `RoundStarted` | `roundId`, `sequenceHash` |
| `NumberDrawn` | `roundId`, `number`, `sequence`, `winnersDetected` |
| `WinningCardDetected` | `roundId`, `count`, e `tieBreakerRequired` quando aplicável |
| `WinnerRevealed` | Nome, código da cartela, prêmio e dados de desempate |
| `WinnerPresentationClosed` | `roundId` |
| `PrizeStageChanged` | `roundId`, prêmio atual e status da rodada |
| `RoundFinished` | `roundId` |
| `EventFinished` | `eventId` |

Também são emitidos `WinnerDetected` e, em empate, `TieBreakerStarted`; ambos têm o mesmo aviso agregado da detecção. Eventos podem ser repetidos ou chegar fora de ordem. Após reconectar, recarregue o estado pela API em vez de depender somente do fluxo recebido.

## Exemplos executáveis

Os arquivos HTTP foram separados por responsabilidade para não concentrar todos os endpoints num único arquivo:

- [`BingoCompany.Api/http/auth.http`](../BingoCompany.Api/http/auth.http)
- [`BingoCompany.Api/http/public.http`](../BingoCompany.Api/http/public.http)
- [`BingoCompany.Api/http/events.http`](../BingoCompany.Api/http/events.http)
- [`BingoCompany.Api/http/cards.http`](../BingoCompany.Api/http/cards.http)

Eles usam valores de exemplo. Após criar um evento, substitua `@eventId`, `@roundId`, `@publicCode`, `@cardCode` e `@participantId` pelos valores retornados. O cliente HTTP deve manter os cookies recebidos no cadastro/login para testar rotas autenticadas. Mantenha `@origin` igual a uma origem autorizada — localmente, `http://localhost:5173`; no Compose, `http://localhost:8080`.
