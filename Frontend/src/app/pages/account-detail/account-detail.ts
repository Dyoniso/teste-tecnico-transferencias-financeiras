import {
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';

import {
  ActivatedRoute,
  Router,
  RouterLink,
} from '@angular/router';

import {
  finalize,
  forkJoin,
} from 'rxjs';

import {
  LucideArrowLeft,
  LucideBadgeDollarSign,
  LucideCalendarClock,
  LucideChevronRight,
  LucideCircleAlert,
  LucideCircleCheck,
  LucideLandmark,
  LucideRefreshCw,
  LucideSend,
  LucideTrendingDown,
  LucideWalletCards,
  LucideX,
} from '@lucide/angular';

import {
  Account,
  Transfer,
} from '../../models/api.models';

import {
  FinancialApiService,
  getApiErrorMessage,
} from '../../services/financial-api.service';

import {
  ToastService,
} from '../../services/toast.service';

@Component({
  selector: 'app-account-detail',
  imports: [
    RouterLink,
    LucideArrowLeft,
    LucideBadgeDollarSign,
    LucideCalendarClock,
    LucideChevronRight,
    LucideCircleAlert,
    LucideCircleCheck,
    LucideLandmark,
    LucideRefreshCw,
    LucideSend,
    LucideTrendingDown,
    LucideWalletCards,
    LucideX,
  ],
  templateUrl: './account-detail.html',
  styleUrl: './account-detail.scss',
})
export class AccountDetail implements OnInit {
  private readonly route =
    inject(ActivatedRoute);

  private readonly router =
    inject(Router);

  private readonly api =
    inject(FinancialApiService);

  private readonly toast =
    inject(ToastService);

  protected readonly account =
    signal<Account | null>(null);

  protected readonly transfers =
    signal<Transfer[]>([]);

  protected readonly loading = signal(true);

  protected readonly error =
    signal<string | null>(null);

  protected readonly showOverdraft =
    signal(false);

  private accountId = 0;

  ngOnInit(): void {
    const id = Number(
      this.route.snapshot.paramMap.get('id'),
    );

    if (
      !Number.isInteger(id) ||
      id <= 0
    ) {
      this.toast.error(
        'Conta inválida',
        'Não foi possível identificar a conta informada.',
      );

      void this.router.navigate(['/']);

      return;
    }

    this.accountId = id;
    this.loadAccount();
  }

  protected loadAccount(): void {
    this.loading.set(true);
    this.error.set(null);

    forkJoin({
      account: this.api.getAccount(this.accountId),
      transfers: this.api.getTransferHistory(
        this.accountId,
      ),
    })
      .pipe(
        finalize(() => {
          this.loading.set(false);
        }),
      )
      .subscribe({
        next: ({ account, transfers }) => {
          this.account.set(account);
          this.transfers.set(transfers);
        },
        error: (error: unknown) => {
          const message =
            getApiErrorMessage(error);

          this.error.set(message);

          this.toast.error(
            'Não foi possível carregar a conta',
            message,
          );
        },
      });
  }

  protected transferDate(
    transfer: Transfer,
  ): string {
    return new Intl.DateTimeFormat(
      'pt-BR',
      {
        dateStyle: 'short',
        timeStyle: 'short',
      },
    ).format(new Date(transfer.createdAt));
  }

  protected scheduledDate(
    transfer: Transfer,
  ): string | null {
    if (!transfer.scheduledAt) {
      return null;
    }

    return new Intl.DateTimeFormat(
      'pt-BR',
      {
        dateStyle: 'short',
        timeStyle: 'short',
      },
    ).format(new Date(transfer.scheduledAt));
  }

  protected isIncoming(
    transfer: Transfer,
  ): boolean {
    return transfer.destinationAccountId ===
      this.accountId;
  }

  protected transferType(
    transfer: Transfer,
  ): string {
    if (transfer.scheduledAt) {
      return this.isIncoming(transfer)
        ? 'Agendada para receber'
        : 'Agendada para enviar';
    }

    return this.isIncoming(transfer)
      ? 'Recebida'
      : 'Enviada';
  }

  protected relatedAccountId(
    transfer: Transfer,
  ): number {
    return this.isIncoming(transfer)
      ? transfer.sourceAccountId
      : transfer.destinationAccountId;
  }

  protected relatedAccountName(
    transfer: Transfer,
  ): string {
    const name = this.isIncoming(transfer)
      ? transfer.sourceAccountName
      : transfer.destinationAccountName;

    return name?.trim() ||
      `Conta #${this.relatedAccountId(transfer)}`;
  }

  protected transferStatusLabel(
    status: string | null,
  ): string {
    switch (status?.toLowerCase()) {
      case 'scheduled':
        return 'Agendada';
      case 'processing':
        return 'Processando';
      case 'completed':
        return 'Concluída';
      case 'failed':
        return 'Falhou';
      case 'cancelled':
        return 'Cancelada';
      default:
        return status || 'Não informado';
    }
  }

  protected transferStatusClass(
    status: string | null,
  ): string {
    return `transfer-status-${
      status?.toLowerCase() ?? 'unknown'
    }`;
  }

  protected toggleOverdraft(): void {
    this.showOverdraft.update(
      (visible) => !visible,
    );
  }

  protected currency(
    value: number | null | undefined,
  ): string {
    return new Intl.NumberFormat(
      'pt-BR',
      {
        style: 'currency',
        currency: 'BRL',
      },
    ).format(value ?? 0);
  }

  protected isActive(
    status: string | null,
  ): boolean {
    const normalized =
      status?.trim().toLowerCase();

    return (
      normalized === 'active' ||
      normalized === 'ativo' ||
      normalized === '1'
    );
  }

  protected statusLabel(
    status: string | null,
  ): string {
    const normalized =
      status?.trim().toLowerCase();

    switch (normalized) {
      case 'active':
      case 'ativo':
      case '1':
        return 'Conta ativa';

      case 'blocked':
      case 'bloqueado':
      case 'bloqueada':
      case '2':
        return 'Conta bloqueada';

      case 'inactive':
      case 'inativo':
      case 'inativa':
      case '3':
        return 'Conta inativa';

      default:
        return status || 'Status não informado';
    }
  }

  protected statusClass(
    status: string | null,
  ): string {
    const normalized =
      status?.trim().toLowerCase();

    if (
      normalized === 'active' ||
      normalized === 'ativo' ||
      normalized === '1'
    ) {
      return 'status-active';
    }

    if (
      normalized === 'blocked' ||
      normalized === 'bloqueado' ||
      normalized === 'bloqueada' ||
      normalized === '2'
    ) {
      return 'status-blocked';
    }

    return 'status-inactive';
  }

  protected overdraftUsed(
    account: Account,
  ): number {
    return account.balance < 0
      ? Math.abs(account.balance)
      : 0;
  }

  protected overdraftAvailable(
    account: Account,
  ): number {
    return Math.max(
      account.overdraftLimit -
        this.overdraftUsed(account),
      0,
    );
  }

  protected overdraftPercentage(
    account: Account,
  ): number {
    if (account.overdraftLimit <= 0) {
      return 0;
    }

    const percentage =
      (
        this.overdraftUsed(account) /
        account.overdraftLimit
      ) * 100;

    return Math.min(
      Math.max(percentage, 0),
      100,
    );
  }
}