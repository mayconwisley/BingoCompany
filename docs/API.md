# Referência da API e SignalR

Base local: `http://localhost:5138`. Em Docker Compose, use `http://localhost:8080`. Em produção, a API é servida abaixo de `/bingo/api`.

As respostas usam JSON com enums serializados como texto. A API retorna `204 No Content` quando uma operação sem corpo é concluída. Mensagens de validação, conflito ou autorização podem ser devolvidas como texto simples; falhas não tratadas seguem o tratamento padronizado de problema da aplicação. Clientes devem usar o código HTTP como contrato principal e exibir mensagens de erro de forma segura, sem supor um formato único para todo erro.

## Convenções de integração

| Tema | Regra |
| --- | --- |
| Identificadores | `eventId`, `roundId` e `participantId` são GUIDs. `code` e `cardCode` são os códigos públicos retornados pela API. Não invente, reaproveite ou trate um ID do cliente como autorizado. |
| JSON | Envie `Content-Type: application/json` quando houver corpo. Enums são textos como `Automatic`, `Ready` e `FullCard`. |
| Sessão | O navegador deve enviar os cookies de sessão (`credentials: "include"` em `fetch`). Aplicações externas precisam preservar o cookie recebido no login. O JWT não é exposto ao JavaScript. |
| Paginação | Rotas paginadas usam `page` e `pageSize`. Use páginas positivas; em listas administrativas o tamanho máximo é 100. Não presuma que todos os registros cabem em uma única resposta. |
| Datas | Registros auditáveis usam data e hora com fuso (`DateTimeOffset`) no JSON. Clientes devem converter para a zona do evento apenas na apresentação. |
| Repetição de requisições | Não repita automaticamente comandos que alteram estado, especialmente `draw`, `start`, `reveal` e marcações. Após timeout, conflito ou reconexão, consulte o recurso para saber se a operação já foi concluída. |

### Códigos HTTP esperados

| Código | Significado para o cliente |
| --- | --- |
| `200 OK` / `201 Created` | Operação concluída; use o corpo devolvido como estado confirmado. |
| `204 No Content` | Operação concluída sem corpo. Atualize a tela por consulta ou SignalR quando necessário. |
| `400 Bad Request` | Dados ausentes, inválidos ou transição de estado não permitida. Corrija a entrada ou recarregue o recurso. |
| `401 Unauthorized` | Não há sessão válida. Solicite login novamente. |
| `403 Forbidden` | A sessão existe, mas não pode acessar aquele recurso/empresa. Não tente contornar pelo cliente. |
| `404 Not Found` | Código/ID não existe ou não está visível para aquela sessão. |
| `409 Conflict` | O estado foi modificado por outra requisição, inclusive duas tentativas de sorteio concorrentes. Recarregue a rodada; nunca execute um novo sorteio como tentativa automática de recuperação. |
| `429 Too Many Requests` | O limite da rota pública foi atingido. Respeite a janela e não faça retry agressivo. |
| `5xx` | Falha transitória ou do servidor. Preserve a intenção do usuário, reconecte/recarregue e confirme o estado antes de repetir uma ação. |

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

No login, envie apenas `email` e `password`. A resposta é `{ "name": "...", "companyName": "..." }` e a sessão é mantida em cookie `HttpOnly`; o token JWT não é retornado ao JavaScript. Para encerrar a sessão, use `POST /api/auth/logout`. `GET /api/auth/session` retorna a sessão atual, inclusive o tipo de conta.

Participantes que compram cartelas podem criar ou acessar a própria conta com `POST /api/participant-auth/register` e `POST /api/participant-auth/login`. Ambos usam `name` (somente no cadastro), `email` e `password`; a resposta contém `{ "name": "..." }` e usa o mesmo cookie protegido. Uma conta de participante não tem acesso às rotas administrativas.

As rotas em `/api/events` exigem a sessão e são limitadas à empresa do usuário, exceto `GET /api/events/{eventId}/cards/{cardCode}/state`, que é a consulta pública da cartela digital pelo seu link. Para métodos que alteram estado autenticado, o navegador precisa enviar uma origem presente em `Cors:AllowedOrigins`; isso protege o cookie contra requisições entre sites.

Não coloque cookie, senha, código de cartela ou dados de colaborador em logs de cliente. As rotas públicas deliberadamente não expõem CPF, matrícula, e-mail, token de acesso ou dados internos de colaboradores.

## Rotas públicas

| Método e rota | Descrição |
| --- | --- |
| `GET /healthz` | Liveness da aplicação; não consulta dependências externas. É apropriado para saber se o processo responde. |
| `GET /readyz` | Readiness para tráfego; valida a conexão com o PostgreSQL. É o health check a usar antes de encaminhar tráfego. |
| `GET /api/application/info` | Nome, descrição e versão da aplicação. |
| `GET /api/public/events/{code}` | Estado público do evento, rodada selecionada, pedras, etapas, estatísticas agregadas e vencedor já revelado. Em desempate revelado, inclui todos os participantes, suas pedras e a indicação do vencedor. |
| `POST /api/public/events/{code}/join` | Inscreve um participante e gera cartela digital. Limite: 20 requisições/minuto por IP. |
| `GET /api/public/events/{code}/audit?page=1&pageSize=25&roundSequence=2` | Histórico público, cartelas, rodadas, resultados e auditoria. A linha do tempo é paginada e filtrada por rodada no backend quando `roundSequence` é informado; a sequência completa somente é exposta ao fim da rodada. |
| `POST /api/public/events/{code}/cards/{cardCode}/activate` | Ativa uma cartela digital comprada pela conta de participante. |

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

O limite de inscrição pública é por IP, portanto uma página de divulgação não deve disparar tentativas em segundo plano. Após uma inscrição bem-sucedida, guarde o link/código da cartela devolvido e apresente ao participante como a forma de retornar à mesma cartela.

## Eventos e cartelas

| Método e rota | Descrição |
| --- | --- |
| `GET /api/events?page=1&pageSize=12` | Lista paginada dos eventos da empresa. `pageSize` é limitado a 100. |
| `POST /api/events` | Cria evento. |
| `GET /api/events/{eventId}` | Detalhes administrativos de um evento. |
| `POST /api/events/{eventId}/registration/open` | Abre inscrições de um evento em preparação. |
| `POST /api/events/{eventId}/card-purchase/open` | Abre a venda de até `quantity` cartelas digitais para contas de participante. |
| `PUT /api/events/{eventId}/card-purchase` | Atualiza o limite da venda enquanto permitido. |
| `POST /api/events/{eventId}/card-purchase/cancel` | Cancela a venda e registra o motivo. |
| `POST /api/events/{eventId}/participants` | Inscreve participante pelo painel administrativo. |
| `POST /api/events/{eventId}/cards/printed` | Gera de 1 a 1.000 cartelas impressas. |
| `GET /api/events/{eventId}/cards/printed` | Lista cartelas impressas e seu QR Code. |
| `POST /api/events/{eventId}/cards/{cardCode}/assign` | Associa cartela impressa a um participante. |
| `POST /api/events/{eventId}/cards/{cardCode}/register` | Cria e associa um participante à cartela impressa. |
| `POST /api/events/{eventId}/cards/{cardCode}/activate` | Ativa cartela impressa associada. |
| `GET /api/events/{eventId}/cards/{cardCode}/state` | Estado da cartela, marcações, pedras e possibilidade de renovação. |
| `POST /api/events/{eventId}/cards/{cardCode}/marks` | Registra uma marcação manual válida. |
| `POST /api/events/{eventId}/cards/{cardCode}/next` | Gera a próxima cartela digital quando a anterior puder ser renovada. |
| `GET /api/participant/cards?page=1&pageSize=5` | Lista as cartelas ativas e o histórico da conta de participante autenticada. |
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

Em `ManualRequired`, a API aceita somente a marca da pedra atual e válida da cartela. Em `AssistedManual`, aceita apenas números da própria cartela que já foram sorteados. Em ambos, uma marca duplicada ou inválida não é uma vitória: a fonte de verdade é o `CardMark` persistido pelo backend. Não implemente “bingo” no cliente a partir da cor da interface.

Para abrir a venda de cartelas, envie `{ "quantity": 50 }`. Para cancelá-la, envie `{ "reason": "Motivo visível para participantes" }`. O cadastro direto em cartela impressa usa o mesmo corpo de inscrição pública. A conta de participante pode comprar somente até o limite ainda disponível e precisa ativar cada cartela comprada antes de ela concorrer.

## Rodadas e prêmios

| Método e rota | Descrição |
| --- | --- |
| `POST /api/events/{eventId}/rounds` | Cria uma rodada pronta. |
| `PUT /api/events/{eventId}/rounds/{roundId}` | Edita uma rodada ainda não iniciada. |
| `POST /api/events/{eventId}/rounds/{roundId}/start` | Congela as cartelas elegíveis, gera a sequência segura e inicia a primeira etapa. |
| `POST /api/events/{eventId}/rounds/{roundId}/cancel` | Cancela uma rodada que ainda está em sorteio, sem concluir a etapa atual. |
| `POST /api/events/{eventId}/rounds/{roundId}/draw` | Sorteia a próxima pedra e devolve a pedra/posição confirmadas. Pode retornar `409` se houve concorrência. |
| `POST /api/events/{eventId}/rounds/{roundId}/reveal` | Revela o vencedor ou conclui o desempate e aguarda a confirmação da entrega. |
| `POST /api/events/{eventId}/rounds/{roundId}/prize-delivered` | Registra a entrega, conclui a etapa e avança a rodada. |
| `POST /api/events/{eventId}/rounds/{roundId}/prize-declined` | Registra que o vencedor não retirou o prêmio e retoma o sorteio na mesma etapa. |
| `POST /api/events/{eventId}/rounds/{roundId}/winner-presentation/close` | Fecha a apresentação após a confirmação da entrega para liberar a próxima pedra ou etapa. |
| `POST /api/events/{eventId}/rounds/{roundId}/printed-cards/{cardCode}/validate-winner` | Confere pelo operador uma cartela impressa candidata a vencer em modo manual. |

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

### Consistência do sorteio

Todo sorteio altera a versão da rodada e é salvo antes da publicação em tempo real. A persistência também possui unicidade para `(roundId, number)` e `(roundId, sequence)`. Essas proteções cobrem o cenário de duas requisições quase simultâneas para a mesma rodada, inclusive em processos diferentes conectados ao mesmo PostgreSQL.

Uma integração deve tratar `draw` como comando de uma única tentativa. Se a conexão cair depois do envio, faça `GET /api/events/{eventId}` ou consulte o estado público/autorizado da rodada, compare a última pedra e somente então permita o próximo comando. Não há endpoint para “desfazer” uma pedra já persistida.

## SignalR

Conecte-se a `/hubs/bingo`. Depois de iniciar a conexão, invoque `JoinEvent(eventId)` e, se necessário, `JoinRound(roundId)`. A aplicação usa os grupos `event:{eventId}` e `round:{roundId}`. Um cliente normalmente escolhe **um** grupo: `JoinEvent` recebe a visão ampla do evento; `JoinRound` é útil quando só acompanha uma rodada. Assinar os dois pode produzir a mesma notificação duas vezes, pois alguns eventos são publicados para os dois grupos.

| Evento | Conteúdo principal |
| --- | --- |
| `RoundStarted` | `roundId`, `sequenceHash` |
| `NumberDrawn` | `roundId`, `number`, `sequence`, `winnersDetected` e `statistics` agregada: `totalCards`, `oneNumberAway`, `twoNumbersAway`, `threeNumbersAway` e `awardedCards`. Não contém nomes de vencedores. |
| `WinningCardDetected` | `roundId`, `count`, e `tieBreakerRequired` quando aplicável |
| `WinnerRevealed` | Nome, código da cartela, prêmio e dados de desempate |
| `WinnerPresentationClosed` | `roundId` |
| `PrizeStageChanged` | `roundId`, prêmio atual e status da rodada |
| `RoundFinished` | `roundId` |
| `EventFinished` | `eventId` |

Também são emitidos `WinnerDetected` e, em empate, `TieBreakerStarted`; ambos têm o mesmo aviso agregado da detecção. Eventos podem ser repetidos ou chegar fora de ordem. Após reconectar, recarregue o estado pela API em vez de depender somente do fluxo recebido.

### Regras obrigatórias para clientes em tempo real

1. Instale os handlers antes de iniciar a conexão e remova-os no descarte da página/componente.
2. Mantenha um indicador visível de `conectado`, `reconectando` e `desconectado` para quem opera o sorteio.
3. Durante reconexão, bloqueie comandos críticos. Ao reconectar, consulte a API e substitua o estado local pelo estado retornado.
4. Aplique `NumberDrawn` de forma idempotente: se o `sequence` já foi visto, ignore o duplicado; se houver lacuna ou evento fora de ordem, recarregue.
5. Nunca use `WinningCardDetected` como autorização para mostrar nome de pessoa. Somente `WinnerRevealed` contém o resultado que pode ser apresentado.

### Endereços por ambiente

| Ambiente | REST | Hub |
| --- | --- | --- |
| Desenvolvimento direto | `http://localhost:5138/api` | `http://localhost:5138/hubs/bingo` |
| Docker Compose | `http://localhost:8080/api` | `http://localhost:8080/hubs/bingo` |
| Produção | `https://mcnwly.com.br/bingo/api` | `https://mcnwly.com.br/bingo/hubs/bingo` |

Os endpoints de health são mapeados na raiz da aplicação (`/healthz` e `/readyz`). Na configuração atual de VPS, eles são verificados localmente em `http://127.0.0.1:5138` e não devem ser publicados pela rota pública sem uma decisão explícita de infraestrutura.

## Exemplos executáveis

Os arquivos HTTP foram separados por responsabilidade para não concentrar todos os endpoints num único arquivo:

- [`BingoCompany.Api/http/auth.http`](../BingoCompany.Api/http/auth.http)
- [`BingoCompany.Api/http/public.http`](../BingoCompany.Api/http/public.http)
- [`BingoCompany.Api/http/events.http`](../BingoCompany.Api/http/events.http)
- [`BingoCompany.Api/http/cards.http`](../BingoCompany.Api/http/cards.http)

Eles usam valores de exemplo. Após criar um evento, substitua `@eventId`, `@roundId`, `@publicCode`, `@cardCode` e `@participantId` pelos valores retornados. O cliente HTTP deve manter os cookies recebidos no cadastro/login para testar rotas autenticadas. Mantenha `@origin` igual a uma origem autorizada — localmente, `http://localhost:5173`; no Compose, `http://localhost:8080`.
