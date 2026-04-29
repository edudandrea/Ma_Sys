# Deploy no Railway

## Arquitetura recomendada

- `frontend`: servico Node/Angular SSR publico
- `backend`: servico .NET API
- `volume`: anexado ao `backend` para banco SQLite e logos

O frontend faz proxy de `/api` para o backend no servidor SSR. Isso mantem o navegador no mesmo dominio do frontend e evita problemas com CORS e URLs de API.

## O que ja ficou preparado no codigo

- `back/MA_Sys.API/Dockerfile`: build e execucao da API em producao
- `front/MA_SYS-APP/Dockerfile`: build e execucao do Angular SSR
- `back/MA_Sys.API/.railwayignore`: evita enviar `bin` e `obj` do Windows para o build Linux
- `front/MA_SYS-APP/.railwayignore`: evita enviar artefatos locais como `dist` e `node_modules`
- `back/MA_Sys.API/Program.cs`: suporta `DATABASE_PATH` e expoe `/health`
- `back/MA_Sys.API/Controllers/AcademiasController.cs`: suporta `UPLOADS_ROOT`
- `front/MA_SYS-APP/src/server.ts`: proxy de `/api` para `BACKEND_URL` e expoe `/health`
- `front/MA_SYS-APP/package.json`: `npm start` sobe o servidor SSR de producao

## 1. Subir o repositorio

- Envie o projeto para um repositorio GitHub
- No Railway, crie um projeto novo
- Crie dois servicos vazios:
- `frontend`
- `backend`

## 2. Configurar o backend

No servico `backend`:

1. Conecte o mesmo repositorio GitHub
2. Em `Settings > Root Directory`, defina:
- `/back/MA_Sys.API`
3. Gere um dominio publico para a API
4. Crie um volume e monte no caminho:
- `/data`

### Variaveis do backend

Defina estas variaveis no servico `backend`:

```env
PORT=8080
DATABASE_PATH=/data/martialartssys.db
UPLOADS_ROOT=/data/uploads
Jwt__Key=gere-uma-chave-grande-e-segura
Jwt__Issuer=MA_Sys.API
Jwt__Audience=MA_Sys.APP
MercadoPago__AccessToken=seu-token-se-houver
Cors__AllowedOrigins__0=https://SEU-FRONTEND.up.railway.app
```

Se usar dominio customizado no frontend, troque `Cors__AllowedOrigins__0` para ele.

### Healthcheck do backend

Em `Settings > Healthcheck Path`:

```txt
/health
```

## 3. Configurar o frontend

No servico `frontend`:

1. Conecte o mesmo repositorio GitHub
2. Em `Settings > Root Directory`, defina:
- `/front/MA_SYS-APP`
3. Gere um dominio publico

### Variaveis do frontend

Defina estas variaveis no servico `frontend`:

```env
PORT=3000
BACKEND_URL=http://backend.railway.internal:8080
```

`backend` precisa ser exatamente o nome do servico da API no Railway. Se voce usar outro nome, ajuste o hostname.

O Railway precisa iniciar o frontend com o servidor SSR gerado no build. Se `npm start` apontar para `ng serve`, o healthcheck tende a falhar porque esse comando e de desenvolvimento.

### Healthcheck do frontend

Em `Settings > Healthcheck Path`:

```txt
/health
```

## 4. Ordem de deploy

1. Faca deploy do `backend`
2. Confirme que `/health` responde `200`
3. Faca deploy do `frontend`
4. Acesse o dominio do `frontend`

## 5. Banco e uploads

Como o sistema usa SQLite e upload de logo:

- o volume no `backend` e obrigatorio
- sem volume, o banco e as logos podem sumir em novo deploy

## 6. Primeiro acesso

No primeiro uso em producao:

1. Abra o frontend
2. Cadastre o primeiro usuario
3. Esse primeiro usuario vira `SuperAdmin`

## 7. Checklist de teste pos-deploy

- [ ] Login funciona pelo dominio do frontend
- [ ] Dashboard abre sem erro
- [ ] Cadastro de academia salva
- [ ] Upload de logo aparece no topo
- [ ] Cadastro publico de aluno abre pelo link da academia
- [ ] Pagamentos carregam
- [ ] Fluxo de caixa lista movimentos
- [ ] Reinicie o backend e confira se banco e logos permaneceram

## 8. Pontos de atencao

- Railway nao persiste armazenamento local sem volume
- no backend, nao envie `bin` e `obj` para o Railway; arquivos gerados no Windows podem quebrar o `dotnet publish` no Linux
- o hostname privado `backend.railway.internal` so funciona entre servicos do mesmo projeto/ambiente
- se mudar o nome do servico `backend`, atualize `BACKEND_URL`
- se ativar dominio customizado, revise `Cors__AllowedOrigins__0`
