import {
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';

import {
  Router,
  RouterLink,
} from '@angular/router';

import {
  forkJoin,
} from 'rxjs';

import {
  LucideArrowLeft,
  LucideCalendarClock,
  LucideCircleCheck,
  LucideCircleX,
  LucideHouse,
  LucidePrinter,
  LucideRotateCcw,
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

import {
  TransferFlowResult,
  TransferResultStoreService,
} from '../../services/transfer-result-store.service';

@Component({
  selector: 'app-transfer-result',
  imports: [
    RouterLink,
    LucideArrowLeft,
    LucideCalendarClock,
    LucideCircleCheck,
    LucideCircleX,
    LucideHouse,
    LucidePrinter,
    LucideRotateCcw,
  ],
  templateUrl: './transfer-result.html',
  styleUrl: './transfer-result.scss',
})
export class TransferResult implements OnInit {
  private readonly store =
    inject(TransferResultStoreService);

  private readonly api =
    inject(FinancialApiService);

  private readonly router =
    inject(Router);

  private readonly toast =
    inject(ToastService);

  protected readonly result =
    signal<TransferFlowResult | null>(null);

  protected readonly sourceAccount =
    signal<Account | null>(null);

  protected readonly destinationAccount =
    signal<Account | null>(null);

  protected readonly loading = signal(true);

  ngOnInit(): void {
    const result = this.store.read();

    if (!result) {
      void this.router.navigate(['/']);
      return;
    }

    this.result.set(result);
    this.loadAccounts(result);
  }

  protected printReceipt(): void {
    window.print();
  }

  protected currency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  }

  protected date(value: string | null): string {
    if (!value) {
      return 'Não informado';
    }

    return new Intl.DateTimeFormat(
      'pt-BR',
      {
        dateStyle: 'short',
        timeStyle: 'medium',
      },
    ).format(new Date(value));
  }

  protected retryLink(
    result: TransferFlowResult,
  ): string[] {
    return [
      '/contas',
      String(result.sourceAccountId),
      result.mode === 'scheduled'
        ? 'agendar'
        : 'transferir',
    ];
  }

  private loadAccounts(
    result: TransferFlowResult,
  ): void {
    if (!result.destinationAccountId) {
      this.loading.set(false);
      return;
    }

    forkJoin({
      source: this.api.getAccount(
        result.sourceAccountId,
      ),
      destination: this.api.getAccount(
        result.destinationAccountId,
      ),
    }).subscribe({
      next: ({ source, destination }) => {
        this.sourceAccount.set(source);
        this.destinationAccount.set(destination);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);

        this.toast.error(
          'Comprovante incompleto',
          getApiErrorMessage(error),
        );
      },
    });
  }
}