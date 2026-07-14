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
  <img src="https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white" alt="PostgreSQL">
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
   npm install
   npm run dev
   ```

O frontend fica disponível em `http://localhost:5173`; a API em `http://localhost:5138`; o Swagger, apenas em desenvolvimento, em `http://localhost:5138/swagger`.

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
4. Gere, associe e ative cartelas impressas quando necessário.
5. Configure a rodada e as etapas de prêmio.
6. Inicie o sorteio, revele os vencedores e acompanhe o telão em tempo real.
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
npm run build
```

## Documentação

- [Guia de uso](docs/GUIA-DE-USO.md)
- [Referência da API e SignalR](docs/API.md)
- [Exemplos HTTP](BingoCompany.Api/http)
- [Publicação na VPS](deploy/vps/README.md)

## Segurança e auditoria

- A sequência é gerada no backend com fonte criptograficamente segura e publicada com hash antes da revelação.
- O frontend não decide vencedores, elegibilidade, marcações ou pedras sorteadas.
- Dados sensíveis de colaboradores não são expostos pelas rotas públicas.
- Todas as ações relevantes são registradas para auditoria.

## Licença

Consulte [LICENSE.txt](LICENSE.txt).
