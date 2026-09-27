# Transferências Financeiras

Aplicação para gerenciamento de pessoas, contas bancárias e transferências financeiras, permitindo consultar saldos e realizar transferências imediatas ou agendadas.

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

O **Kafka UI** também está disponível em:

```text
http://localhost/kafka-ui/
```

Ele pode ser utilizado como ferramenta auxiliar para inspeção dos tópicos, consumidores e mensagens processadas pelo Kafka.

## Utilizando a aplicação

A principal forma de utilização do sistema é através da interface web:

```text
http://localhost/
```

A aplicação permite acessar as funcionalidades de cadastro de pessoas e contas, consultar informações financeiras e realizar ou agendar transferências.

Para consultar todos os endpoints disponíveis, parâmetros, contratos de entrada e saída e códigos HTTP, utilize a documentação interativa do Swagger:

```text
http://localhost/swagger/
```

O Swagger também permite executar requisições diretamente contra a API durante testes e validações.

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

Assim, quem abrir o README consegue rodar e testar o projeto antes mesmo de entrar nos detalhes de arquitetura.

## Tecnologias e arquitetura

A solução é composta por frontend, backend, banco de dados e processamento assíncrono, executados de forma integrada através do Docker Compose.

### Backend

O backend foi desenvolvido em **ASP.NET Core 10**, seguindo uma organização baseada em **MVC** e separação de responsabilidades entre as principais camadas da aplicação.

A estrutura utiliza:

- **MVC**, para organização dos endpoints e fluxo das requisições.
- **SOLID**, buscando reduzir acoplamento e facilitar manutenção, evolução e testes.
- **Clean Code**, com responsabilidades bem definidas e código orientado à legibilidade e manutenção.
- **Repository Pattern**, isolando o acesso e persistência dos dados.
- **Service Layer**, concentrando regras de negócio e validações.
- **Dependency Injection**, utilizada para desacoplar controllers, serviços, repositórios e demais componentes.
- **DTOs**, utilizados para separar os contratos de entrada e saída dos modelos de persistência.

O backend também implementa processamento assíncrono utilizando **Kafka** e o padrão **Outbox**, aumentando a confiabilidade na publicação e processamento dos eventos relacionados às transferências agendadas.

### Frontend

O frontend foi desenvolvido em **Angular 22** e fornece a interface principal de interação com o sistema.

#### Funcionalidades de negócio

Através da aplicação web é possível:

- cadastrar pessoas;
- criar e visualizar contas;
- consultar saldo disponível;
- consultar limite de cheque especial;
- visualizar o status da conta;
- realizar transferências imediatas;
- agendar transferências para datas futuras;
- acompanhar transferências enviadas e recebidas;
- acompanhar transferências agendadas.

#### Implementações técnicas

O frontend utiliza:

- **Angular 22**;
- arquitetura baseada em componentes;
- serviços para comunicação com a API;
- interfaces e modelos tipados para os contratos da aplicação;
- separação entre páginas, componentes e serviços;
- tratamento centralizado de mensagens e erros;
- navegação entre as funcionalidades da aplicação;
- integração com a API REST através de requisições HTTP;
- layout responsivo e interface inspirada em aplicações financeiras.

### Infraestrutura

A aplicação utiliza:

- **PostgreSQL** para persistência dos dados;
- **Kafka** para processamento assíncrono;
- **Kafka UI** como interface auxiliar para inspeção dos tópicos, consumidores e mensagens;
- **Nginx** como ponto de entrada da aplicação;
- **Docker Compose** para execução e integração dos serviços.

## Implementações de negócio

- Cadastro e gerenciamento de pessoas.
- Cadastro e gerenciamento de contas.
- Consulta de saldo.
- Controle de cheque especial.
- Controle de status da conta.
- Validação de limites de transferência.
- Transferências imediatas entre contas.
- Agendamento de transferências para datas futuras.
- Consulta de transferências enviadas e recebidas.
- Consulta de transferências agendadas.
- Processamento automático das transferências agendadas.

## Implementações técnicas

- API REST em **ASP.NET Core 10**.
- Interface web em **Angular 22**.
- Persistência utilizando **PostgreSQL**.
- Arquitetura **MVC** no backend.
- Separação entre **Controllers, Services, Repositories e DTOs**.
- Aplicação dos princípios **SOLID**.
- Aplicação de práticas de **Clean Code**.
- Uso de **Dependency Injection**.
- Processamento assíncrono utilizando **Kafka**.
- Implementação do padrão **Outbox** para publicação confiável de eventos.
- **Nginx** como ponto único de entrada da aplicação.
- Documentação interativa da API utilizando **Swagger**.
- Ambiente containerizado utilizando **Docker Compose**.