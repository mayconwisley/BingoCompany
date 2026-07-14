# Bingo Company — Guia para agentes

## Objetivo do produto

O Bingo Company é um sistema de bingo corporativo em tempo real. Participantes usam cartelas digitais ou impressas; o operador controla o sorteio; o telão apresenta as pedras, o suspense e o resultado. O backend é a única fonte de verdade para cartelas, pedras, marcações, vencedores e auditoria.

## Estrutura da solução

| Projeto | Responsabilidade |
| --- | --- |
| `BingoCompany.Domain` | Entidades, enums e invariantes de negócio. Não depende de outros projetos. |
| `BingoCompany.Application` | Regras puras: geração de cartelas, sequência segura e avaliação de padrões. |
| `BingoCompany.Infrastructure` | EF Core, PostgreSQL e mapeamento de persistência. |
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
- A API usa PostgreSQL. Ao modificar o modelo persistido, prefira criar migrations EF Core. Não apague ou recrie o banco para resolver problemas de esquema.
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
dotnet test BingoCompany.sln --no-restore -v minimal
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

## Arquitetura e padrões obrigatórios

### DDD (Domain-Driven Design)

- Organize o código respeitando as camadas `Domain`, `Application`, `Infrastructure` e `Api`.
- O `Domain` nunca deve depender de outras camadas.
- Regras de negócio pertencem ao domínio, nunca aos Controllers ou à Infraestrutura.
- Entidades devem proteger seus próprios invariantes.
- Utilize Value Objects sempre que representarem melhor um conceito do domínio.
- Repositórios devem ser definidos no domínio e implementados na Infrastructure.

### Clean Architecture

- Dependências sempre apontam para dentro da arquitetura.
- Controllers apenas recebem requisições e delegam para a camada de Application.
- A camada Application orquestra casos de uso, mas não contém regras de infraestrutura.
- Infrastructure contém EF Core, SignalR, acesso a banco, serviços externos e implementações concretas.

### SOLID

- Cada classe deve possuir uma única responsabilidade (SRP).
- Prefira extensão à modificação (OCP).
- Respeite substituição de Liskov (LSP).
- Interfaces pequenas e específicas (ISP).
- Dependa de abstrações, nunca de implementações (DIP).

### Organização dos arquivos

- Cada classe deve possuir seu próprio arquivo.
- Nunca agrupe múltiplas classes públicas em um mesmo arquivo.
- Evite arquivos excessivamente grandes. Sempre que um arquivo começar a acumular responsabilidades distintas, extraia novas classes, serviços, validadores, mapeamentos ou estratégias.
- Prefira arquivos pequenos, coesos e fáceis de navegar.
- Organize o código por responsabilidade e contexto do domínio, não por tipo técnico quando isso prejudicar a coesão.

### Código limpo

- Escreva código autoexplicativo antes de recorrer a comentários.
- Utilize nomes claros e consistentes.
- Evite duplicação de código (DRY).
- Prefira composição à herança quando apropriado.
- Não utilize métodos longos. Extraia responsabilidades para métodos privados ou classes específicas.
- Não utilize "Helpers" genéricos como depósito de funcionalidades sem relação.
- Não crie classes "God Objects".

### Convenções

- Utilize sempre finais de linha **CRLF** (`\r\n`).
- Nunca gere ou converta arquivos para **LF**.
- Preserve a codificação UTF-8.
- Mantenha a formatação e estilo existentes do projeto.
- Preserve alterações do usuário que não estejam relacionadas à tarefa.

## Convenções .NET

- Utilize `file-scoped namespace`.
- Utilize `required` quando apropriado.
- Prefira `DateTimeOffset` ao invés de `DateTime`.
- Utilize `Guid.CreateVersion7()` para novos identificadores quando o projeto utilizar GUID v7.
- Evite propriedades públicas com `set`; prefira métodos de domínio (`Update`, `Activate`, `Deactivate`, etc.).
- Não utilize `#region`.
- Não utilize comentários desnecessários; escreva código autoexplicativo.
- Utilize `sealed` para classes que não devem ser herdadas.
- Prefira campos `readonly` sempre que possível.
- Utilize `CancellationToken` em todas as operações assíncronas.
- Nunca retorne `null` para coleções; retorne coleções vazias.
- Prefira `async/await` em vez de bloqueios síncronos.
- Utilize `ConfigureAwait(false)` apenas quando fizer sentido para bibliotecas.
- Evite métodos estáticos para regras de negócio do domínio.
- Não misture regras de negócio com acesso a banco de dados.
- Toda alteração de regra de negócio deve possuir testes unitários.
- Mantenha baixo acoplamento e alta coesão entre classes.
- Utilize injeção de dependência para dependências externas.
- Evite números mágicos e strings mágicas; utilize constantes, Value Objects ou enums quando apropriado.
- Toda exceção deve possuir uma mensagem clara e contextualizada.
- Evite lógica condicional extensa; prefira polimorfismo, Strategy ou Specification quando apropriado.

## Convenções de frontend

### Arquitetura

- Organize o frontend por funcionalidades e contextos de negócio, evitando uma estrutura baseada apenas em tipos técnicos.
- Mantenha separadas as responsabilidades de apresentação, aplicação, domínio e infraestrutura.
- Componentes de interface não devem conhecer detalhes de HTTP, autenticação, armazenamento local ou SignalR.
- A camada de apresentação deve depender de abstrações e contratos estáveis.
- Regras de negócio do frontend devem ficar fora dos componentes visuais.
- Não replique no frontend regras críticas cuja fonte de verdade pertence ao backend.
- Utilize o backend como fonte de verdade para sorteios, vencedores, elegibilidade, marcações, permissões e transições de estado.
- Evite dependências cíclicas entre módulos, features e componentes compartilhados.
- Não importe arquivos internos de outra feature. Exponha apenas uma API pública por meio de um arquivo `index.ts`.
- Código compartilhado deve ser realmente genérico. Não mova código para `shared` apenas para reutilizá-lo uma vez.

### Estrutura recomendada

```text
src/
├── app/                    # bootstrap, providers, rotas e composição global
├── pages/                  # composição de telas
├── features/               # casos de uso e funcionalidades do produto
│   └── bingo/
│       ├── api/            # chamadas HTTP e SignalR da feature
│       ├── components/     # componentes específicos da feature
│       ├── hooks/          # hooks específicos da feature
│       ├── model/          # tipos, estados, mapeadores e regras locais
│       ├── services/       # orquestração sem dependência visual
│       ├── tests/          # testes de integração da feature
│       └── index.ts        # API pública da feature
├── entities/               # conceitos de domínio reutilizados entre features
├── shared/
│   ├── api/                # cliente HTTP, interceptadores e erros comuns
│   ├── config/             # configuração tipada da aplicação
│   ├── hooks/              # hooks realmente genéricos
│   ├── lib/                # utilitários puros e coesos
│   ├── types/              # contratos compartilhados
│   └── ui/                 # componentes visuais reutilizáveis
└── test/                   # configuração global de testes
```

- Adapte a estrutura ao tamanho real do projeto. Não crie pastas vazias ou abstrações sem uso.
- Prefira organização por feature quando ela melhorar coesão, isolamento e manutenção.

### Componentes

- Cada componente deve possuir uma responsabilidade clara.
- Mantenha componentes pequenos, coesos e fáceis de compreender.
- Extraia componentes quando houver responsabilidade visual independente, repetição relevante ou complexidade própria.
- Não fragmente componentes de forma artificial apenas para reduzir linhas.
- Componentes reutilizáveis devem receber dados e callbacks por propriedades.
- Componentes reutilizáveis não devem executar chamadas HTTP diretamente.
- Prefira componentes controlados quando o estado precisar ser coordenado externamente.
- Evite componentes que concentrem renderização, regras de negócio, acesso a dados, navegação e efeitos colaterais.
- Um componente React por arquivo, exceto componentes privados triviais e inseparáveis do componente principal.
- O nome do arquivo deve corresponder ao principal símbolo exportado.
- Utilize composição em vez de componentes altamente parametrizados por várias flags booleanas.
- Evite propriedades booleanas que alterem comportamentos não relacionados. Considere variantes explícitas ou componentes distintos.
- Não utilize índices de array como `key` quando existir um identificador estável.
- Não utilize componentes anônimos complexos dentro do JSX.
- Evite JSX excessivamente aninhado; extraia partes semânticas quando necessário.
- Componentes devem ser acessíveis por teclado e possuir rótulos semânticos apropriados.

### Hooks

- Hooks devem encapsular uma responsabilidade específica.
- Não crie hooks apenas para mover código sem formar uma abstração coerente.
- Hooks não devem retornar estruturas excessivamente grandes ou ambíguas.
- Prefira nomes que expressem intenção, como `useBingoRound`, `useDrawNumber` ou `useLiveBingo`.
- Efeitos devem ser utilizados apenas para sincronização com sistemas externos.
- Não utilize `useEffect` para derivar estado que pode ser calculado durante a renderização.
- Toda inscrição, temporizador ou listener deve possuir limpeza correspondente.
- Dependências de `useEffect`, `useMemo` e `useCallback` devem estar corretas e completas.
- Não utilize `useMemo` ou `useCallback` sem uma necessidade mensurável ou sem estabilização de referência necessária.
- Hooks de acesso a dados devem expor estados de carregamento, erro e sucesso de forma previsível.

### Estado

- Mantenha o estado o mais próximo possível de quem o utiliza.
- Não coloque no estado global informações que pertencem apenas a uma tela ou componente.
- Não duplique estado derivável.
- Prefira estado normalizado quando houver coleções relacionadas.
- Atualizações de estado devem ser imutáveis.
- Modele estados assíncronos explicitamente quando necessário, evitando combinações inválidas de várias flags booleanas.
- Não use armazenamento local como fonte de verdade para dados críticos.
- Tokens, dados sensíveis e permissões não devem ser tratados como confiáveis apenas porque estão no cliente.

### TypeScript

- Utilize TypeScript em modo estrito.
- Não utilize `any`. Quando inevitável, documente a razão e restrinja seu escopo.
- Prefira `unknown` para dados externos ainda não validados.
- Valide dados recebidos de APIs, armazenamento, URL e integrações externas.
- Evite type assertions com `as` para mascarar inconsistências de tipos.
- Prefira tipos discriminados para representar variações de estado.
- Utilize `type` e `interface` de forma consistente com o padrão existente.
- Não crie tipos genéricos excessivamente abstratos sem necessidade concreta.
- Evite enums numéricos no frontend quando unions literais oferecerem melhor segurança e interoperabilidade.
- Não replique manualmente contratos do backend quando houver geração ou compartilhamento seguro de contratos.
- Tipos de DTO não devem ser usados diretamente como estado visual quando houver necessidades diferentes.
- Crie mapeadores entre contratos externos e modelos internos quando isso reduzir acoplamento.

### SOLID no frontend

- Cada componente, hook, serviço e módulo deve possuir uma única responsabilidade.
- Novos comportamentos devem ser adicionados preferencialmente por composição, não por condicionais crescentes.
- Componentes substituíveis devem manter contratos compatíveis.
- Interfaces e propriedades devem ser pequenas e específicas.
- Dependências externas devem ser encapsuladas por adaptadores quando isso facilitar substituição e testes.
- Não acople componentes diretamente a implementações globais quando uma dependência explícita melhorar testabilidade.

### Clean Code

- Utilize nomes que expressem intenção.
- Evite abreviações obscuras, nomes genéricos e variáveis de uma letra fora de escopos triviais.
- Funções devem ser pequenas, previsíveis e possuir poucos níveis de abstração.
- Não misture transformação de dados, efeitos colaterais e renderização na mesma função.
- Prefira funções puras para regras, cálculos, filtros, mapeamentos e validações.
- Evite condicionais profundas; utilize guard clauses e extração de funções.
- Não crie arquivos genéricos como `utils.ts`, `helpers.ts` ou `common.ts` para responsabilidades sem relação.
- Arquivos grandes devem ser divididos por responsabilidade, não por quantidade arbitrária de linhas.
- Remova código morto, imports não utilizados, comentários obsoletos e abstrações sem uso.
- Comentários devem explicar decisões e contexto, não repetir o que o código já expressa.
- Não utilize valores mágicos. Extraia constantes com nomes semânticos.
- Mantenha tratamento de erro consistente e mensagens úteis para diagnóstico.

### Acesso a dados e integrações

- Centralize configuração de cliente HTTP, URL base, headers comuns e tratamento de erros.
- Não espalhe chamadas `fetch` ou `axios` diretamente por páginas e componentes.
- Separe DTOs de entrada, DTOs de saída e modelos de apresentação quando necessário.
- Utilize `AbortSignal` para cancelar requisições quando a biblioteca adotada oferecer suporte.
- Trate erros de rede, autorização, validação, timeout e indisponibilidade de forma distinta quando isso afetar a experiência.
- Não exponha mensagens internas do backend diretamente ao usuário sem sanitização.
- SignalR deve ficar encapsulado em serviço ou hook específico.
- Após reconexão do SignalR, recarregue o estado atual por API para evitar perda de eventos.
- Não assuma que eventos em tempo real serão entregues uma única vez ou em ordem perfeita.
- Mantenha handlers idempotentes sempre que possível.

### Formulários

- Separe validação, estado do formulário e submissão.
- Validações de interface devem melhorar a experiência, mas não substituem validações do backend.
- Exiba mensagens de erro próximas ao campo relacionado.
- Desabilite submissões duplicadas enquanto uma requisição estiver em andamento.
- Preserve dados preenchidos quando ocorrer um erro recuperável.
- Utilize elementos HTML semânticos antes de criar controles customizados.

### Acessibilidade

- Todo campo deve possuir rótulo associado.
- Botões devem possuir nomes acessíveis claros.
- Elementos clicáveis devem utilizar elementos semânticos apropriados.
- Modais devem controlar foco, permitir fechamento por teclado e restaurar o foco ao fechar.
- Estados de erro, carregamento e sucesso devem ser anunciáveis quando necessário.
- Não dependa apenas de cor para transmitir informação.
- Mantenha contraste adequado e navegação funcional por teclado.
- Imagens informativas devem possuir texto alternativo; imagens decorativas devem ser ignoradas por tecnologias assistivas.

### Testabilidade

- Componentes devem poder ser renderizados isoladamente.
- Dependências externas devem ser injetáveis ou substituíveis em testes.
- Evite acessar diretamente objetos globais em regras testáveis.
- Prefira testes orientados ao comportamento observado pelo usuário.
- Utilize Testing Library para interagir com a interface por papéis, rótulos e texto visível.
- Evite testar detalhes internos de implementação, estados privados ou estrutura exata do DOM sem necessidade.
- Todo componente interativo deve possuir testes para seus principais comportamentos.
- Toda regra pura deve possuir testes unitários.
- Toda feature crítica deve possuir ao menos um teste de integração.
- Bugs corrigidos devem receber um teste de regressão.
- Testes devem ser determinísticos e não depender de ordem, horário real, rede real ou dados compartilhados.
- Utilize mocks somente nas fronteiras externas. Não faça mock de tudo indiscriminadamente.
- Prefira builders e factories de teste para dados complexos.
- Testes devem seguir Arrange, Act e Assert de forma legível.
- O nome do teste deve descrever comportamento, contexto e resultado esperado.
- Evite snapshots extensos como substitutos de asserções significativas.

### Desempenho

- Não otimize prematuramente.
- Meça antes de aplicar memoização ou virtualização.
- Evite renderizações desnecessárias causadas por estado global amplo ou referências instáveis.
- Utilize lazy loading em rotas ou módulos quando houver benefício real.
- Não carregue bibliotecas grandes para funcionalidades triviais.
- Evite transformações custosas diretamente no JSX.
- Listas extensas devem considerar paginação ou virtualização.
- Imagens e ativos devem ser dimensionados e carregados de forma adequada.

### Segurança

- Nunca confie em autorização feita apenas no frontend.
- Não armazene segredos, chaves privadas ou credenciais sensíveis no bundle.
- Variáveis expostas pelo Vite devem ser tratadas como públicas.
- Não renderize HTML não confiável.
- Evite `dangerouslySetInnerHTML`; quando inevitável, sanitize o conteúdo.
- Não registre tokens, CPF, e-mail, credenciais ou dados sensíveis no console.
- Trate dados vindos da URL e do armazenamento como entrada não confiável.

### Estilo e qualidade

- Siga a configuração existente de ESLint, TypeScript e formatter.
- Não desabilite regras de lint sem justificativa técnica clara.
- Corrija warnings e erros de TypeScript antes de concluir uma tarefa.
- Evite imports relativos longos quando aliases já estiverem configurados.
- Preserve o padrão de nomenclatura existente.
- Utilize finais de linha CRLF em todos os arquivos de frontend.
- Nunca gere ou converta arquivos para LF.
- Preserve UTF-8.
- Ao alterar dependências, atualize `package.json` e o arquivo de lock correspondente.
- Antes de concluir alterações, execute os testes, o lint e o build aplicáveis.
