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
  LucideShieldCheck,
  LucideTrendingDown,
  LucideWalletCards,
  LucideX,
} from '@lucide/angular';

import {
  Account,
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
    LucideShieldCheck,
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

    this.api
      .getAccount(this.accountId)
      .pipe(
        finalize(() => {
          this.loading.set(false);
        }),
      )
      .subscribe({
        next: (account: Account) => {
          this.account.set(account);
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