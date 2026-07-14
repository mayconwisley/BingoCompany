# Bingo Company

Plataforma de bingo corporativo em tempo real. A organização cria eventos e rodadas com etapas progressivas de prêmio; participantes usam cartelas digitais ou impressas; o operador conduz o sorteio; o telão e as cartelas acompanham a partida ao vivo.

O backend é a fonte de verdade para a sequência de pedras, cartelas elegíveis, marcações e vencedores.

## Para quem usa o sistema

A ajuda está disponível dentro do produto em **Ajuda** (`/ajuda`). Para uma referência que pode ser compartilhada ou impressa, consulte o [guia de uso](docs/GUIA-DE-USO.md).

Fluxo da organização:

1. Cadastre a empresa e entre na área administrativa.
2. Crie o evento e escolha o modo de marcação.
3. Abra as inscrições e compartilhe o link ou QR Code.
4. Gere/associe/ative cartelas físicas, se houver.
5. Crie uma rodada com uma ou mais etapas de prêmio.
6. Inicie a rodada, faça os sorteios e revele cada vencedor.
7. Encerre a apresentação do prêmio antes de continuar e finalize o evento somente após concluir todas as rodadas.

## Arquitetura

| Projeto | Responsabilidade |
| --- | --- |
| `BingoCompany.Domain` | Entidades, enums e invariantes de negócio. |
| `BingoCompany.Application` | Geração segura de cartelas e sequência, avaliação de padrões e jogo da rodada. |
| `BingoCompany.Infrastructure` | EF Core, PostgreSQL e migrations. |
| `BingoCompany.Api` | REST, SignalR, autenticação e inicialização do banco. |
| `BingoCompany.Frontend` | React, TypeScript e Vite para participantes, organização, operação, telão e auditoria. |
| `BingoCompany.Tests` | Testes unitários e de integração. |

## Requisitos

- .NET SDK compatível com `BingoCompany.Api/BingoCompany.Api.csproj`.
- Node.js e npm para o frontend.
- PostgreSQL acessível. No desenvolvimento local, a aplicação pode criar o banco configurado, desde que o usuário informado em `DBBingoUser` tenha a permissão necessária.

## Executar localmente

1. Configure as credenciais do PostgreSQL. O arquivo `BingoCompany.Api/appsettings.json` contém a string de conexão com os marcadores `{{username}}` e `{{password}}`; informe os valores por variáveis de ambiente:

   ```powershell
   $env:DBBingoUser = "bingo"
   $env:DBBingoPass = "uma-senha-local"
   $env:BingoJwtKey = "uma-chave-aleatoria-com-pelo-menos-32-caracteres"
   ```

2. Em um terminal, execute a API:

   ```powershell
   dotnet run --project .\BingoCompany.Api
   ```

3. Em outro terminal, instale e inicie o frontend:

   ```powershell
   cd .\BingoCompany.Frontend
   npm install
   npm run dev
   ```

Por padrão, o frontend é aberto em `http://localhost:5173` e a API em `http://localhost:5138`. Swagger só é exposto no ambiente `Development`, em `http://localhost:5138/swagger`. O endpoint de saúde é `GET /healthz`.

## Demonstração com Docker Compose

O Compose é destinado a uma demonstração local por HTTP, não a produção.

```powershell
Copy-Item .env.example .env
# Edite .env e substitua os valores de exemplo por segredos aleatórios.
docker compose up --build
```

Abra `http://localhost:8080`. A mesma origem atende o frontend, a API em `/api`, o SignalR em `/hubs/bingo` e o Swagger em `/swagger`. Os dados ficam no volume `bingo-postgres-data`.

```powershell
docker compose down
```

Para apagar os dados da demonstração, use `docker compose down -v`. Isso remove o volume do PostgreSQL e não deve ser usado para resolver alterações de esquema em ambientes persistentes.

## API, tempo real e testes manuais

- A referência funcional dos endpoints está em [docs/API.md](docs/API.md).
- Os exemplos executáveis estão divididos em [`BingoCompany.Api/http`](BingoCompany.Api/http): autenticação, consulta pública e operações de evento/cartela.
- O hub SignalR é `/hubs/bingo`; clientes entram nos grupos do evento e, opcionalmente, da rodada. Os eventos e as regras de reconexão estão em [docs/API.md](docs/API.md#signalr).

As rotas administrativas exigem a sessão autenticada e só permitem acessar eventos da própria empresa. As rotas públicas não retornam CPF, matrícula, e-mail, token de acesso nem dados internos de colaboradores.

## Validação

Na raiz da solução:

```powershell
dotnet test BingoCompany.sln --no-restore -v minimal
dotnet build BingoCompany.Api/BingoCompany.Api.csproj --no-restore -v minimal
```

No frontend:

```powershell
cd .\BingoCompany.Frontend
npm test
npm run build
```

## Publicação na VPS

O procedimento de produção, incluindo Nginx, systemd, variáveis de ambiente e release por tag, está em [deploy/vps/README.md](deploy/vps/README.md).
