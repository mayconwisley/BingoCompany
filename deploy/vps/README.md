# Publicação na VPS sem Docker

O domínio público é `https://mcnwly.com.br/bingo/`. O Nginx atende os arquivos estáticos, encaminha `/bingo/api` e `/bingo/hubs` para a API local e preserva o suporte a SignalR. O deploy publica diretamente em `/opt/bingo/api` e `/opt/bingo/frontend`, no mesmo padrão do AssinaFlux.

## Responsabilidades e limites do ambiente

| Componente | Responsabilidade | Não deve fazer |
| --- | --- | --- |
| Nginx | TLS, cabeçalhos de segurança, arquivos estáticos, proxy REST e WebSocket do SignalR. | Expor PostgreSQL, PgBouncer ou a porta interna da API. |
| `bingo-api.service` | Executar a API em `127.0.0.1:5138`, aplicar migrations no início e reiniciar após falha. | Conter segredos no arquivo unit ou executar como `root`. |
| PostgreSQL | Fonte persistente de eventos, cartelas, sorteios e auditoria. | Ser recriado ou limpo como forma de resolver migration/erro de deploy. |
| PgBouncer | Pool local de conexões entre API e PostgreSQL. | Criar usuário/banco ou ficar exposto à internet. |
| GitHub Actions | Validar CI, publicar artefatos versionados e reiniciar a API. | Substituir backup, observação pós-deploy ou revisão da migration. |

Os endpoints `/healthz` e `/readyz` são atendidos pela API na raiz da porta local, e **não** são encaminhados pelo arquivo Nginx atual. Para a VPS, execute-os em `http://127.0.0.1:5138`; não os torne públicos sem necessidade e sem definir política de acesso/monitoramento.

## Preparação única da VPS

Instale .NET SDK/runtime 10, Nginx, PostgreSQL, `rsync` e Certbot. Crie os diretórios de publicação usados pelo usuário `ubuntu`:

```bash
sudo install -d -o ubuntu -g ubuntu /opt/bingo/frontend /opt/bingo/api
```

Crie o usuário e o banco diretamente no PostgreSQL, não pelo PgBouncer. A API de produção se conecta ao pooler local na porta `6432` e não executa bootstrap de banco:

```bash
sudo -u postgres psql -p 5432
```

No prompt do PostgreSQL:

```sql
CREATE ROLE bingo LOGIN PASSWORD 'defina-uma-senha-forte';
CREATE DATABASE bingo OWNER bingo;
```

Registre o banco e o usuário `bingo` na configuração existente do PgBouncer, seguindo o mesmo método de autenticação usado pelo AssinaFlux. Depois recarregue o pooler:

```bash
sudo systemctl reload pgbouncer
```

Copie `api.env.example` para `/opt/bingo/.env`, preencha os segredos e limite a leitura ao usuário de publicação:

```bash
sudo cp deploy/vps/api.env.example /opt/bingo/.env
sudo chown ubuntu:ubuntu /opt/bingo/.env
sudo chmod 600 /opt/bingo/.env
```

Gere `BingoJwtKey` com pelo menos 32 bytes aleatórios (por exemplo, `openssl rand -base64 48`). Em produção, mantenha também `Authentication__CookiePath=/bingo`, as duas origens HTTPS em `Cors__AllowedOrigins__0` e `Cors__AllowedOrigins__1`, e os hosts em `AllowedHosts`, como no arquivo de exemplo. A API não inicia em produção sem a chave JWT.

O arquivo de Nginx fornecido já referencia o certificado de `mcnwly.com.br`. Antes de ativá-lo, obtenha o certificado. Se a porta 80 estiver livre, uma opção é usar o modo standalone:

```bash
sudo certbot certonly --standalone -d mcnwly.com.br -d www.mcnwly.com.br
```

Em seguida, instale a unidade e a configuração Nginx, validando ambas antes de ativá-las:

```bash
sudo cp deploy/vps/bingo-api.service /etc/systemd/system/bingo-api.service
sudo cp deploy/vps/mcnwly.com.br.nginx.conf /etc/nginx/sites-available/mcnwly.com.br
sudo ln -s /etc/nginx/sites-available/mcnwly.com.br /etc/nginx/sites-enabled/mcnwly.com.br
sudo systemctl daemon-reload
sudo nginx -t
sudo systemctl enable bingo-api
sudo systemctl reload nginx
```

A configuração Nginx incluída já contém os blocos `listen 443 ssl`, HSTS, CSP e outros cabeçalhos de proteção. Após copiá-la, valide com `sudo nginx -t` antes de recarregar. Não exponha a porta da API (`5138`) ou do PgBouncer (`6432`) na internet; ambas devem continuar vinculadas a `127.0.0.1`. A API recebe `X-Forwarded-Proto` do Nginx e, por isso, reconhece corretamente a requisição HTTPS.

### Pré-checagem antes do primeiro deploy

Execute estas verificações antes de criar a primeira release:

```bash
sudo systemctl status postgresql pgbouncer nginx --no-pager
sudo -u postgres psql -p 5432 -d bingo -c "SELECT current_database(), current_user;"
sudo test -r /opt/bingo/.env && sudo stat -c '%a %U:%G %n' /opt/bingo/.env
sudo nginx -t
```

O arquivo de ambiente deve aparecer com permissão `600` e dono `ubuntu`. Confira, sem copiar os valores para terminal/log, que `BingoJwtKey`, `DBBingoUser`, `DBBingoPass`, `ConnectionStrings__Bingo`, `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, `Authentication__CookiePath` e `Database__EnsureDatabaseExists=false` foram preenchidos. A última configuração impede que a aplicação tente criar banco em produção; o banco e o usuário devem existir previamente.

Faça um backup restaurável **antes** de toda release que contenha migration:

```bash
backup_dir=/var/backups/bingo
sudo install -d -o ubuntu -g ubuntu "$backup_dir"
pg_dump -h 127.0.0.1 -p 6432 -U bingo -Fc -d bingo -f "$backup_dir/bingo-$(date +%F-%H%M%S).dump"
```

Proteja o diretório de backup, valide periodicamente uma restauração em banco isolado e defina retenção conforme a política da empresa. Um dump criado não equivale a um backup validado.

## Segredos e variáveis do GitHub

Configure os secrets `VPS_HOST`, `VPS_PORT`, `VPS_USER` e `VPS_SSH_KEY`, seguindo o mesmo padrão do AssinaFlux. O usuário da VPS precisa ter escrita em `/opt/bingo/frontend` e `/opt/bingo/api`, além de permissão restrita de `sudo systemctl restart bingo-api`.

## Release

Atualize `<Version>` em `BingoCompany.Api/BingoCompany.Api.csproj`, execute as validações e publique uma tag com exatamente a mesma versão:

```bash
git tag v1.0.0
git push origin v1.0.0
```

O workflow só continua se o CI tiver sido aprovado para o mesmo commit, a tag tiver o formato `vX.Y.Z` e corresponder exatamente ao `<Version>` da API. Após publicar os artefatos em `/opt/bingo`, ele reinicia a API, anexa os pacotes e seus hashes SHA-256 à GitHub Release.

## Validação pós-publicação

Assim que o workflow concluir, valide a publicação em camadas. Não considere a release concluída apenas porque o processo de cópia terminou.

```bash
sudo systemctl status bingo-api --no-pager
sudo journalctl -u bingo-api -n 150 --no-pager
curl --fail --silent --show-error http://127.0.0.1:5138/healthz
curl --fail --silent --show-error http://127.0.0.1:5138/readyz
curl --fail --silent --show-error --location https://mcnwly.com.br/bingo/
curl --fail --silent --show-error --location https://mcnwly.com.br/bingo/api/application/info
```

Interprete os checks desta forma:

- `healthz` confirma que o processo ASP.NET responde; ele não valida banco.
- `readyz` confirma que a API consegue conectar ao PostgreSQL; se falhar, não libere o uso do sistema.
- A URL pública confirma TLS, Nginx e o frontend. O endpoint `application/info` confirma o caminho do proxy REST.
- Para SignalR, faça um teste manual no navegador: abra uma página pública do evento, inicie uma rodada de ensaio e confirme que telão e operação recebem a atualização. Esse teste detecta problema de WebSocket/proxy que um `curl` não cobre.

Em seguida, verifique no navegador: login de organizador, criação/consulta de cartela em conta de teste, marcação compatível com o modo escolhido, atualização do telão e auditoria. Não use dados pessoais reais para o teste técnico.

## Procedimento de incidente e recuperação

### API não inicia ou `readyz` falha

1. Preserve as evidências: anote versão da tag, horário e saída de `systemctl status`/`journalctl`.
2. Verifique conectividade local com PostgreSQL/PgBouncer e a presença das variáveis em `/opt/bingo/.env`; não imprima segredos em tickets, chat ou logs.
3. Verifique se a migration da release foi aplicada e se a incompatibilidade é de aplicação, banco ou configuração.
4. Mantenha o tráfego bloqueado se `readyz` falhar. Reiniciar repetidamente não corrige credencial, migration ou indisponibilidade do banco.

### Reverter uma release

Reverter arquivos da API pode ser seguro somente quando a migration já aplicada for compatível com a versão anterior. **Nunca** execute rollback/destruição de schema por impulso. Primeiro, avalie a migration e restaure o tráfego apenas para uma versão comprovadamente compatível. Se a migration não for reversível sem perda, corrija por uma nova migration de avanço ou restaure um backup em um plano controlado e aprovado.

Depois de qualquer reversão, execute novamente todos os comandos de validação pós-publicação e um teste de SignalR. Registre a causa, a versão afetada, o backup disponível e a decisão tomada.

### Sorteio em andamento

Se houver incidente enquanto uma rodada está em curso, não tente alterar diretamente o banco e não reproduza comandos de sorteio por script. Oriente o operador a recarregar a operação quando o serviço voltar, conferir a última pedra persistida e continuar somente a partir do estado oficial. A auditoria e a proteção de concorrência existem para preservar exatamente esse histórico.
