<p align="center">
  <img src="BingoCompany.Frontend/public/assets/bingo-company-logo.png" width="150" alt="Logo do Bingo Company">
</p>

<h1 align="center">Bingo Company</h1>

<p align="center">
  Plataforma corporativa de bingo em tempo real, com sorteios auditáveis e experiências para organização, participantes, operação e telão.
</p>

<p align="center">
  <a href="https://github.com/mayconwisley/BingoCompany/actions/workflows/ci.yml"><img src="https://github.com/mayconwisley/BingoCompany/actions/workflows/ci.yml/badge.svg?branch=master" alt="CI"></a>
  <a href="https://github.com/mayconwisley/BingoCompany/actions/workflows/release.yml"><img src="https://github.com/mayconwisley/BingoCompany/actions/workflows/release.yml/badge.svg" alt="Deploy de produção"></a>
  <a href="https://github.com/mayconwisley/BingoCompany/releases/latest"><img src="https://img.shields.io/github/v/release/mayconwisley/BingoCompany?display_name=tag&sort=semver" alt="Versão"></a>
  <a href="https://mcnwly.com.br/bingo/"><img src="https://img.shields.io/website?url=https%3A%2F%2Fmcnwly.com.br%2Fbingo%2F&label=deploy" alt="Aplicação em produção"></a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white" alt=".NET 10">
  <img src="https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=20232A" alt="React 19">
  <img src="https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white" alt="TypeScript">
  <img src="https://img.shields.io/badge/Vite-6-646CFF?logo=vite&logoColor=white" alt="Vite">
  <img src="https://img.shields.io/badge/PostgreSQL-18-4169E1?logo=postgresql&logoColor=white" alt="PostgreSQL">
  <img src="https://img.shields.io/badge/SignalR-tempo%20real-512BD4" alt="SignalR">
  <img src="https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white" alt="Docker Compose">
</p>

## Visão geral

O Bingo Company organiza eventos de bingo corporativo com cartelas digitais e impressas. A plataforma mantém o backend como fonte de verdade para cartelas, sequência de pedras, marcações, elegibilidade, vencedores e auditoria.

- Eventos com várias rodadas e etapas progressivas de prêmio.
- Inscrição pública por link ou QR Code.
- Cartelas digitais e físicas com rastreabilidade.
- Marcação automática, manual obrigatória ou manual assistida.
- Operação em tempo real via SignalR e telão público.
- Auditoria de sequência, rodadas, cartelas e vencedores.
- Fluxo de sorteio validado no núcleo da aplicação com 1.000 cartelas elegíveis e 1.000 empates simultâneos.

## Garantias do produto

O Bingo Company foi desenhado para que o resultado não dependa do navegador, da rapidez de uma pessoa ao clicar ou da ordem em que as telas recebem atualizações. Estas são as garantias que devem orientar a operação e qualquer integração:

| Garantia | Como é aplicada | Impacto prático |
| --- | --- | --- |
| Backend como fonte de verdade | Cartelas, pedras, marcações, elegibilidade, vencedores e auditoria são decididos e persistidos no backend. | Nunca use uma tela local ou uma conferência manual como resultado oficial. |
| Sequência auditável | Cada rodada gera as 75 pedras sem repetição com fonte criptograficamente segura e publica o hash antes da primeira pedra. | A auditoria permite verificar, ao término, que a sequência já estava definida. |
| Elegibilidade congelada | Ao iniciar a rodada, a lista de cartelas ativas e associadas é capturada em um snapshot. | Ativar ou associar uma cartela depois do início não a inclui na rodada corrente. |
| Marcação conforme o modo | Em `Automatic`, a pedra sorteada vale automaticamente. Nos modos manuais, somente a marca persistida pela API vale. | Uma marca feita apenas no papel ou em uma interface desatualizada não cria vencedor. |
| Sorteio serializado | O estado da rodada possui controle de concorrência e a persistência impede a repetição de número ou posição na sequência. | Dois comandos simultâneos não devem revelar duas pedras válidas; em conflito, recarregue a operação. |
| Privacidade no telão | Antes da revelação, o telão recebe somente contagens agregadas. | O nome do vencedor e o desempate só aparecem após a ação explícita do operador. |

## Antes do evento: checklist operacional

Faça esta preparação antes de abrir o local ao público. Ela reduz os problemas que não podem ser corrigidos quando a rodada já começou.

1. Confirme que o ambiente de produção responde ao health check de prontidão e que o banco está acessível.
2. Abra o evento, defina o modo de marcação e teste o link/QR Code de inscrição em um celular fora da conta administrativa.
3. Para cartelas impressas, gere um lote de teste, imprima uma unidade, confira QR Code e código, registre/associe o participante e ative a cartela.
4. Crie as rodadas e todos os prêmios. Revise nomes, ordem efetiva das regras e imagens antes de iniciar; rodadas não podem ser editadas após o início.
5. Em uma rede e dispositivo semelhantes aos do evento, abra o telão e a operação. Verifique que ambos mostram o mesmo estado e que o indicador da operação está como **Conectado**.
6. Defina quem será o organizador, quem poderá operar o sorteio e como será confirmada a retirada de cada prêmio. Mantenha um segundo dispositivo apenas para monitorar o telão/auditoria.

Durante a rodada, clique uma única vez em cada ação. Se a interface perder conexão, estiver processando ou retornar conflito, não tente “compensar” sorteando novamente: espere a reconexão e recarregue o estado oficial.

## Status de qualidade e publicação

| Item | Status | O que valida |
| --- | --- | --- |
| CI | [![CI](https://github.com/mayconwisley/BingoCompany/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/mayconwisley/BingoCompany/actions/workflows/ci.yml) | Testes, build, ESLint, tipos, formatação e auditoria de dependências. |
| Deploy | [![Deploy](https://github.com/mayconwisley/BingoCompany/actions/workflows/release.yml/badge.svg)](https://github.com/mayconwisley/BingoCompany/actions/workflows/release.yml) | Publicação versionada na VPS após aprovação do CI. |
| Produção | [![Produção](https://img.shields.io/website?url=https%3A%2F%2Fmcnwly.com.br%2Fbingo%2F&label=online)](https://mcnwly.com.br/bingo/) | Disponibilidade da aplicação pública. |

Os badges são atualizados pelo GitHub Actions e pelo monitoramento do endereço público; verde indica a última execução ou verificação bem-sucedida.

## Arquitetura

| Projeto | Responsabilidade |
| --- | --- |
| `BingoCompany.Domain` | Entidades, enums e invariantes de negócio. |
| `BingoCompany.Application` | Casos de uso puros: geração segura, avaliação de padrões e regras da rodada. |
| `BingoCompany.Infrastructure` | EF Core, PostgreSQL e mapeamento de persistência. |
| `BingoCompany.Api` | API REST, SignalR, autenticação e inicialização. |
| `BingoCompany.Frontend` | React + TypeScript + Vite para participantes, organização, operação, telão e auditoria. |
| `BingoCompany.Tests` | Testes unitários e de integração do backend. |

## Tecnologias

| Camada | Recursos |
| --- | --- |
| Backend | .NET 10, ASP.NET Core, EF Core, JWT, OpenAPI/Swagger |
| Frontend | React 19, TypeScript, Vite, React Router, Vitest, Testing Library |
| Tempo real | ASP.NET Core SignalR |
| Dados | PostgreSQL |
| Operação | Docker Compose, Nginx, systemd e GitHub Actions |

## Começar rapidamente

### Pré-requisitos

- .NET SDK 10.
- Node.js 22 ou superior e npm.
- PostgreSQL acessível.
- Docker Desktop opcional para a demonstração completa.

### Desenvolvimento local

1. Configure as credenciais locais do PostgreSQL e a chave JWT:

   ```powershell
   $env:DBBingoUser = "bingo"
   $env:DBBingoPass = "uma-senha-local"
   $env:BingoJwtKey = "uma-chave-aleatoria-com-pelo-menos-32-caracteres"
   ```

2. Inicie a API:

   ```powershell
   dotnet run --project .\BingoCompany.Api
   ```

3. Em outro terminal, inicie o frontend:

   ```powershell
   cd .\BingoCompany.Frontend
   npm ci
   npm run dev
   ```

O frontend fica disponível em `http://localhost:5173`; a API em `http://localhost:5138`; o Swagger, apenas em desenvolvimento, em `http://localhost:5138/swagger`.

Para verificar a aplicação localmente sem depender do frontend:

```powershell
Invoke-WebRequest http://localhost:5138/healthz
Invoke-WebRequest http://localhost:5138/readyz
```

`/healthz` verifica apenas se o processo está vivo. `/readyz` também verifica se o PostgreSQL aceita conexão; use este último antes de liberar tráfego para uma instância.

### Demonstração com Docker Compose

```powershell
Copy-Item .env.example .env
# Edite .env e informe segredos locais.
docker compose up --build
```

Abra `http://localhost:8080`. O Compose atende o frontend, a API em `/api`, o SignalR em `/hubs/bingo` e o Swagger em `/swagger`.

Para encerrar:

```powershell
docker compose down
```

> Para remover também os dados locais da demonstração, use `docker compose down -v`.

## Fluxo de uso

1. Cadastre a empresa e entre na administração.
2. Crie o evento e escolha o modo de marcação.
3. Abra as inscrições e compartilhe o link público ou QR Code.
4. Gere, imprima, registre ou associe e ative cartelas impressas quando necessário.
5. Configure a rodada e as etapas de prêmio.
6. Inicie o sorteio, revele os vencedores, confirme a entrega dos prêmios e acompanhe o telão em tempo real.
7. Finalize o evento e disponibilize a auditoria pública.

## Qualidade

Na raiz da solução:

```powershell
dotnet test BingoCompany.sln --no-restore -v minimal
dotnet build BingoCompany.Api/BingoCompany.Api.csproj --no-restore -v minimal
```

No frontend:

```powershell
cd .\BingoCompany.Frontend
npm test
npm run lint
npm run typecheck
npm run typeformat
npm run build
```

Antes de uma publicação, execute as validações acima em um checkout limpo e confira também a migration pendente:

```powershell
$env:Database__EnsureDatabaseExists = "false"
dotnet ef migrations has-pending-model-changes --no-build --project .\BingoCompany.Infrastructure --startup-project .\BingoCompany.Api
```

O comando deve informar que não há mudanças pendentes. Ele não substitui o backup do banco nem a revisão da migration que será aplicada no ambiente de produção.

### Capacidade de rodada

O núcleo do sorteio possui teste de regressão para 1.000 cartelas elegíveis na mesma rodada, incluindo a detecção de 1.000 vencedores simultâneos e o respectivo desempate. O desempate atribui posições únicas por embaralhamento criptograficamente seguro e não é limitado às 75 pedras.

O lote de cartelas impressas aceita até 1.000 unidades por requisição; novos lotes podem ser gerados para o mesmo evento. A capacidade efetiva em produção também depende do PostgreSQL, da infraestrutura de rede e da quantidade de conexões SignalR, que devem ser testadas no ambiente de implantação antes de um evento de grande porte.

## Documentação

- [Guia de uso](docs/GUIA-DE-USO.md)
- [Referência da API e SignalR](docs/API.md)
- [Exemplos HTTP](BingoCompany.Api/http)
- [Publicação na VPS](deploy/vps/README.md)

O [guia de uso](docs/GUIA-DE-USO.md) é a referência para organizadores, operadores e participantes. A tela **Ajuda** no sistema resume o mesmo fluxo e inclui os cuidados de conexão durante o sorteio.

## Segurança e auditoria

- A sequência é gerada no backend com fonte criptograficamente segura e publicada com hash antes da revelação.
- O frontend não decide vencedores, elegibilidade, marcações ou pedras sorteadas.
- Dados sensíveis de colaboradores não são expostos pelas rotas públicas.
- Todas as ações relevantes são registradas para auditoria.
- Uma etapa só é concluída quando o operador confirma a entrega do prêmio; se não houver retirada, a rodada segue com a mesma regra e sem a cartela já recusada.
- O aplicativo instalável (PWA) pode manter a interface estática disponível após uma visita, mas não armazena respostas de API, conexão SignalR ou ações de jogo. Para jogar, marcar ou operar é necessária conexão com o backend.

## Licença

Consulte [LICENSE.txt](LICENSE.txt).
