# Publicação na VPS sem Docker

O domínio público é `https://mcnwly.com.br/bingo/`. O Nginx atende os arquivos estáticos, encaminha `/bingo/api` e `/bingo/hubs` para a API local e preserva o suporte a SignalR. O deploy publica diretamente em `/opt/bingo/api` e `/opt/bingo/frontend`, no mesmo padrão do AssinaFlux.

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

Instale a unidade e a configuração Nginx, validando ambas antes de ativá-las:

```bash
sudo cp deploy/vps/bingo-api.service /etc/systemd/system/bingo-api.service
sudo cp deploy/vps/mcnwly.com.br.nginx.conf /etc/nginx/sites-available/mcnwly.com.br
sudo ln -s /etc/nginx/sites-available/mcnwly.com.br /etc/nginx/sites-enabled/mcnwly.com.br
sudo systemctl daemon-reload
sudo nginx -t
sudo systemctl enable bingo-api
sudo systemctl reload nginx
```

Em seguida, obtenha o certificado TLS com Certbot e inclua os blocos `listen 443 ssl` no virtual host. A API recebe `X-Forwarded-Proto` do Nginx, portanto reconhece corretamente a requisição HTTPS.

## Segredos e variáveis do GitHub

Configure os secrets `VPS_HOST`, `VPS_PORT`, `VPS_USER` e `VPS_SSH_KEY`, seguindo o mesmo padrão do AssinaFlux. O usuário da VPS precisa ter escrita em `/opt/bingo/frontend` e `/opt/bingo/api`, além de permissão restrita de `sudo systemctl restart bingo-api`.

## Release

Atualize `<Version>` em `BingoCompany.Api/BingoCompany.Api.csproj`, execute as validações e publique uma tag com exatamente a mesma versão:

```bash
git tag v1.0.0
git push origin v1.0.0
```

O workflow só continua se o CI tiver sido aprovado para o mesmo commit, a tag tiver o formato `vX.Y.Z` e corresponder exatamente ao `<Version>` da API. Após publicar os artefatos em `/opt/bingo`, ele reinicia a API, anexa os pacotes e seus hashes SHA-256 à GitHub Release.
