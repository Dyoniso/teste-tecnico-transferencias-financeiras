# Transferências Financeiras

Aplicação para gerenciamento de pessoas, contas bancárias e transferências financeiras, permitindo consultar saldos e realizar transferências imediatas ou agendadas.

<img width="1241" height="377" alt="image" src="https://github.com/user-attachments/assets/9aa27f58-8d27-4f00-afb3-e408ebc6a205" />

## Executando o projeto

É necessário possuir **Docker Desktop** com o **Docker Compose** instalado e em execução.

Na pasta raiz do projeto, execute:

```bash
docker compose up -d --build
```

Para verificar se os serviços foram inicializados corretamente:

```bash
docker compose ps
```

Após a inicialização, acesse:

- **Aplicação Web:** http://localhost/
- **Documentação da API - Swagger:** http://localhost/swagger/

## Encerrando a aplicação

Para interromper os containers:

```bash
docker compose down
```

Os dados armazenados no PostgreSQL permanecem preservados através do volume:

```text
postgres_data
```

Caso também seja necessário remover os dados persistidos localmente:

```bash
docker compose down -v
```

## Tecnologias e arquitetura

A aplicação foi construída com **Angular 22** no frontend, **ASP.NET Core 10** no backend, **PostgreSQL** para persistência e **Kafka** para processamento assíncrono.

Todo o ambiente é executado com **Docker Compose**, utilizando **Nginx** como ponto de entrada da aplicação.

### Backend

O backend segue uma arquitetura baseada em **MVC**, com separação entre:

- **Controllers**, responsáveis por receber e validar as requisições;
- **Services**, responsáveis pelas regras de negócio;
- **Repositories**, responsáveis pelo acesso aos dados;
- **DTOs**, utilizados nos contratos de entrada e saída da API.

A implementação também aplica princípios de **SOLID**, **Clean Code** e **Dependency Injection**, buscando manter baixo acoplamento e responsabilidades bem definidas.

As transferências agendadas são processadas de forma assíncrona através do **Kafka**, utilizando o padrão **Outbox** para garantir maior confiabilidade na publicação dos eventos.

### Frontend

O frontend foi desenvolvido em **Angular 22**, com estrutura baseada em componentes, páginas e serviços.

A aplicação permite:

- cadastrar pessoas e contas;
- consultar saldo, cheque especial e status da conta;
- realizar transferências imediatas;
- agendar transferências;
- acompanhar transferências enviadas, recebidas e agendadas.

A comunicação com o backend é realizada através da API REST, utilizando modelos tipados e tratamento centralizado de mensagens e erros.

### Infraestrutura

- **PostgreSQL** para persistência dos dados;
- **Kafka** para processamento assíncrono;
- **Kafka UI** para inspeção de tópicos, consumidores e mensagens;
- **Nginx** como gateway da aplicação;
- **Docker Compose** para orquestração dos serviços;
- **Swagger** para documentação e testes da API.


