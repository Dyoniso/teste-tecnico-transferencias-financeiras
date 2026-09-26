import {
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';

import { TransferForm } from '../../components/transfer-form/transfer-form';
import { TransferQuery } from '../../components/transfer-query/transfer-query';

import {
  Account,
  CreateTransferPayload,
  ScheduleTransferPayload,
  Transfer,
} from '../../models/api.models';

import {
  FinancialApiService,
  getApiErrorMessage,
} from '../../services/financial-api.service';

@Component({
  selector: 'app-account-detail',
  imports: [
    RouterLink,
    TransferForm,
    TransferQuery,
  ],
  templateUrl: './account-detail.html',
})
export class AccountDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(FinancialApiService);

  protected readonly account =
    signal<Account | null>(null);

  protected readonly accounts = signal<Account[]>([]);
  protected readonly transferResult =
    signal<Transfer | null>(null);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  ngOnInit(): void {
    this.loadAccount();
  }

  protected loadAccount(): void {
    const id = Number(
      this.route.snapshot.paramMap.get('id'),
    );

    this.loading.set(true);

    forkJoin({
      account: this.api.getAccount(id),
      accounts: this.api.listAccounts(),
    })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: ({ account, accounts }) => {
          this.account.set(account);
          this.accounts.set(accounts);
        },
        error: (error) => {
          this.error.set(getApiErrorMessage(error));
        },
      });
  }

  protected transfer(
    payload: CreateTransferPayload,
  ): void {
    this.clearMessages();

    this.api.transfer(payload).subscribe({
      next: (transfer) => {
        this.transferResult.set(transfer);
        this.success.set(
          'Transferência realizada com sucesso.',
        );
        this.loadAccount();
      },
      error: (error) => {
        this.error.set(getApiErrorMessage(error));
      },
    });
  }

  protected schedule(
    payload: ScheduleTransferPayload,
  ): void {
    this.clearMessages();

    this.api.scheduleTransfer(payload).subscribe({
      next: (transfer) => {
        this.transferResult.set(transfer);
        this.success.set(
          'Transferência agendada com sucesso.',
        );
      },
      error: (error) => {
        this.error.set(getApiErrorMessage(error));
      },
    });
  }

  protected searchTransfer(id: string): void {
    this.clearMessages();

    this.api.getTransfer(id).subscribe({
      next: (transfer) => {
        this.transferResult.set(transfer);
      },
      error: (error) => {
        this.error.set(getApiErrorMessage(error));
      },
    });
  }

  protected cancelTransfer(): void {
    const transfer = this.transferResult();

    if (!transfer) return;

    this.api.cancelTransfer(transfer.id).subscribe({
      next: (updatedTransfer) => {
        this.transferResult.set(updatedTransfer);
        this.success.set(
          'Agendamento cancelado com sucesso.',
        );
      },
      error: (error) => {
        this.error.set(getApiErrorMessage(error));
      },
    });
  }

  protected currency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  }

  protected date(value: string | null): string {
    if (!value) return 'Não informado';

    return new Intl.DateTimeFormat('pt-BR', {
      dateStyle: 'short',
      timeStyle: 'short',
    }).format(new Date(value));
  }

  private clearMessages(): void {
    this.error.set(null);
    this.success.set(null);
  }
}