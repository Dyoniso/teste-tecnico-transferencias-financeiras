import {
  HttpClient,
  HttpErrorResponse,
} from '@angular/common/http';

import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import {
  Account,
  CreateAccountPayload,
  CreateTransferPayload,
  Person,
  PersonPayload,
  ProblemDetails,
  ScheduleTransferPayload,
  Transfer,
  UpdateAccountPayload,
} from '../models/api.models';

@Injectable({
  providedIn: 'root',
})
export class FinancialApiService {
  private readonly http = inject(HttpClient);

  private readonly personsUrl = '/api/persons';
  private readonly accountsUrl = '/api/accounts';
  private readonly transfersUrl = '/api/transfers';

  // =====================================================
  // Pessoas
  // =====================================================

  /**
   * Lista todas as pessoas cadastradas.
   */
  listPersons(): Observable<Person[]> {
    return this.http.get<Person[]>(this.personsUrl);
  }

  /**
   * Busca uma pessoa pelo identificador.
   */
  getPerson(id: number): Observable<Person> {
    return this.http.get<Person>(
      `${this.personsUrl}/${id}`,
    );
  }

  /**
   * Cadastra uma nova pessoa.
   */
  createPerson(
    payload: PersonPayload,
  ): Observable<Person> {
    return this.http.post<Person>(
      this.personsUrl,
      payload,
    );
  }

  /**
   * Atualiza uma pessoa.
   */
  updatePerson(
    id: number,
    payload: PersonPayload,
  ): Observable<Person> {
    return this.http.put<Person>(
      `${this.personsUrl}/${id}`,
      payload,
    );
  }

  /**
   * Exclui uma pessoa.
   */
  deletePerson(id: number): Observable<void> {
    return this.http.delete<void>(
      `${this.personsUrl}/${id}`,
    );
  }

  // =====================================================
  // Contas
  // =====================================================

  /**
   * Lista todas as contas cadastradas.
   */
  listAccounts(): Observable<Account[]> {
    return this.http.get<Account[]>(
      this.accountsUrl,
    );
  }

  /**
   * Busca uma conta pelo identificador.
   */
  getAccount(id: number): Observable<Account> {
    return this.http.get<Account>(
      `${this.accountsUrl}/${id}`,
    );
  }

  /**
   * Cria uma conta e vincula a uma pessoa.
   */
  createAccount(
    payload: CreateAccountPayload,
  ): Observable<Account> {
    return this.http.post<Account>(
      this.accountsUrl,
      payload,
    );
  }

  /**
  * Atualiza a pessoa vinculada, o limite de cheque
  * especial e o status da conta.
   */
  updateAccount(
    id: number,
    payload: UpdateAccountPayload,
  ): Observable<Account> {
    return this.http.put<Account>(
      `${this.accountsUrl}/${id}`,
      payload,
    );
  }

  /**
   * Exclui uma conta.
   */
  deleteAccount(id: number): Observable<void> {
    return this.http.delete<void>(
      `${this.accountsUrl}/${id}`,
    );
  }

  // =====================================================
  // Transferências
  // =====================================================

  /**
   * Realiza uma transferência imediatamente.
   */
  transfer(
    payload: CreateTransferPayload,
  ): Observable<Transfer> {
    return this.http.post<Transfer>(
      this.transfersUrl,
      payload,
    );
  }

  /**
   * Agenda uma transferência.
   */
  scheduleTransfer(
    payload: ScheduleTransferPayload,
  ): Observable<Transfer> {
    return this.http.post<Transfer>(
      `${this.transfersUrl}/scheduled`,
      payload,
    );
  }

  /**
   * Consulta uma transferência pelo UUID.
   */
  getTransfer(id: string): Observable<Transfer> {
    return this.http.get<Transfer>(
      `${this.transfersUrl}/${id}`,
    );
  }

  /**
   * Cancela uma transferência agendada.
   */
  cancelTransfer(id: string): Observable<Transfer> {
    return this.http.post<Transfer>(
      `${this.transfersUrl}/${id}/cancel`,
      {},
    );
  }
}

/**
 * Converte os diferentes formatos de erro
 * retornados pelo backend em uma mensagem.
 */
export function getApiErrorMessage(
  error: unknown,
): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Ocorreu um erro inesperado. Tente novamente.';
  }

  if (error.status === 0) {
    return 'Não foi possível conectar ao backend. Verifique se a API está em execução.';
  }

  const response: unknown = error.error;

  if (typeof response === 'string') {
    const message = response.trim();

    if (message) {
      return message;
    }
  }

  if (isProblemDetails(response)) {
    if (
      typeof response.message === 'string' &&
      response.message.trim()
    ) {
      return response.message.trim();
    }

    const validationMessage =
      getFirstValidationMessage(response.errors);

    if (validationMessage) {
      return validationMessage;
    }

    if (
      typeof response.detail === 'string' &&
      response.detail.trim()
    ) {
      return response.detail.trim();
    }

    if (
      typeof response.title === 'string' &&
      response.title.trim()
    ) {
      return response.title.trim();
    }
  }

  return getMessageByStatus(error.status);
}

/**
 * Verifica se a resposta possui o formato
 * ProblemDetails retornado pelo ASP.NET.
 */
function isProblemDetails(
  value: unknown,
): value is ProblemDetails {
  return (
    typeof value === 'object' &&
    value !== null
  );
}

/**
 * Retorna a primeira mensagem de validação
 * encontrada no ProblemDetails.
 */
function getFirstValidationMessage(
  errors:
    | Record<string, string[]>
    | undefined,
): string | null {
  if (!errors) {
    return null;
  }

  for (const messages of Object.values(errors)) {
    if (
      Array.isArray(messages) &&
      typeof messages[0] === 'string'
    ) {
      return messages[0];
    }
  }

  return null;
}

/**
 * Mensagens alternativas baseadas no código HTTP.
 */
function getMessageByStatus(status: number): string {
  switch (status) {
    case 400:
      return 'Os dados informados são inválidos. Verifique os campos e tente novamente.';

    case 404:
      return 'O registro solicitado não foi encontrado.';

    case 409:
      return 'A operação não pôde ser realizada devido a um conflito.';

    case 422:
      return 'A operação não atende às regras de negócio.';

    case 500:
      return 'O servidor encontrou um erro interno. Tente novamente mais tarde.';

    default:
      return `Não foi possível concluir a operação (${status}).`;
  }
}