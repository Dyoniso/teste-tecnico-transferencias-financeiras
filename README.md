# Transferências Financeiras

Aplicação para cadastro de pessoas e contas, consulta de saldo e realização de transferências imediatas ou agendadas.

## Implementações

- API REST em ASP.NET Core 10.
- Interface web em Angular 22.
- PostgreSQL para persistência dos dados.
- Kafka para processamento assíncrono das transferências agendadas.
- Outbox para publicação confiável de eventos.
- Controle de saldo, cheque especial, status da conta e limites de transferência.
- Histórico de transferências enviadas, recebidas e agendadas por conta.
- Nginx como ponto único de acesso para frontend, API, Swagger e Kafka UI.
- Docker Compose para executar toda a aplicação.

## Executar com Docker

É necessário ter Docker Desktop com Docker Compose instalado e em execução.

1. Abra um terminal na pasta raiz do projeto.
2. Construa as imagens e inicie os serviços:

   ```bash
   docker compose up -d --build
   ```

3. Confira se os containers estão ativos:

   ```bash
   docker compose ps
   ```

4. Acesse os serviços:

   - Aplicação: http://localhost/
   - Swagger da API: http://localhost/swagger/
   - Kafka UI: http://localhost/kafka-ui/

Para encerrar a aplicação:

```bash
docker compose down
```

Os dados do PostgreSQL ficam preservados no volume `postgres_data`. Para apagar também os dados locais, use `docker compose down -v`.

## Endpoint de histórico

O histórico de uma conta pode ser consultado em:

```http
GET /api/transfer-history/accounts/{accountId}
```

A resposta reúne as transferências em que a conta é origem ou destino, ordenadas da mais recente para a mais antiga.