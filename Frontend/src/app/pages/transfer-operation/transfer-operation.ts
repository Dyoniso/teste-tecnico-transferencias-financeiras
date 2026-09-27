import {
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';

import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

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
  LucideCalendarClock,
  LucideSend,
  LucideShieldCheck,
  LucideWalletCards,
} from '@lucide/angular';

import {
  Account,
  CreateTransferPayload,
  ScheduleTransferPayload,
} from '../../models/api.models';

import {
  FinancialApiService,
  getApiErrorMessage,
} from '../../services/financial-api.service';

import {
  ToastService,
} from '../../services/toast.service';

import {
  TransferOperationMode,
  TransferResultStoreService,
} from '../../services/transfer-result-store.service';

@Component({
  selector: 'app-transfer-operation',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    LucideArrowLeft,
    LucideCalendarClock,
    LucideSend,
    LucideShieldCheck,
    LucideWalletCards,
  ],
  templateUrl: './transfer-operation.html',
  styleUrl: './transfer-operation.scss',
})
export class TransferOperation implements OnInit {
  private readonly formBuilder =
    inject(NonNullableFormBuilder);

  private readonly route =
    inject(ActivatedRoute);

  private readonly router =
    inject(Router);

  private readonly api =
    inject(FinancialApiService);

  private readonly toast =
    inject(ToastService);

  private readonly resultStore =
    inject(TransferResultStoreService);

  protected readonly sourceAccount =
    signal<Account | null>(null);

  protected readonly destinationAccounts =
    signal<Account[]>([]);

  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);

  protected readonly mode =
    this.route.snapshot.data[
      'mode'
    ] as TransferOperationMode;

  protected readonly form = this.formBuilder.group({
    destinationAccountId: [
      0,
      [
        Validators.required,
        Validators.min(1),
      ],
    ],
    amount: [
      0,
      [
        Validators.required,
        Validators.min(0.01),
      ],
    ],
    scheduledAt: [''],
  });

  ngOnInit(): void {
    const accountId = Number(
      this.route.snapshot.paramMap.get('id'),
    );

    if (!Number.isInteger(accountId)) {
      void this.router.navigate(['/']);
      return;
    }

    this.loadData(accountId);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();

      this.toast.warning(
        'Verifique os campos',
        'Informe a conta de destino e um valor válido.',
      );

      return;
    }

    const sourceAccount = this.sourceAccount();

    if (!sourceAccount) {
      return;
    }

    const value = this.form.getRawValue();

    if (
      value.destinationAccountId ===
      sourceAccount.id
    ) {
      this.toast.error(
        'Conta inválida',
        'A conta de destino deve ser diferente da conta de origem.',
      );

      return;
    }

    if (
      value.amount >
      sourceAccount.availableBalance
    ) {
      this.toast.error(
        'Saldo insuficiente',
        'O valor ultrapassa o saldo e o cheque especial disponíveis.',
      );

      return;
    }

    this.submitting.set(true);

    if (this.mode === 'scheduled') {
      this.submitScheduled(
        sourceAccount,
        value.destinationAccountId,
        value.amount,
        value.scheduledAt,
      );

      return;
    }

    this.submitImmediate(
      sourceAccount,
      value.destinationAccountId,
      value.amount,
    );
  }

  protected currency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  }

  protected accountLabel(account: Account): string {
    return `Conta #${account.id} — ${account.personName ?? 'Sem nome'}`;
  }

  private loadData(accountId: number): void {
    this.loading.set(true);

    forkJoin({
      source: this.api.getAccount(accountId),
      accounts: this.api.listAccounts(),
    })
      .pipe(
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: ({ source, accounts }) => {
          this.sourceAccount.set(source);

          this.destinationAccounts.set(
            accounts.filter(
              (account) =>
                account.id !== source.id &&
                this.isActive(account.status),
            ),
          );
        },
        error: (error: unknown) => {
          this.toast.error(
            'Não foi possível carregar',
            getApiErrorMessage(error),
          );
        },
      });
  }

  private submitImmediate(
    source: Account,
    destinationAccountId: number,
    amount: number,
  ): void {
    const payload: CreateTransferPayload = {
      sourceAccountId: source.id,
      destinationAccountId,
      amount,
    };

    this.api
      .transfer(payload)
      .pipe(
        finalize(() =>
          this.submitting.set(false),
        ),
      )
      .subscribe({
        next: (transfer) => {
          this.resultStore.save({
            success: true,
            mode: 'immediate',
            sourceAccountId: source.id,
            destinationAccountId,
            transfer,
            generatedAt: new Date().toISOString(),
          });

          void this.router.navigate([
            '/contas',
            source.id,
            'resultado',
          ]);
        },
        error: (error: unknown) => {
          this.navigateToError(
            source.id,
            destinationAccountId,
            getApiErrorMessage(error),
          );
        },
      });
  }

  private submitScheduled(
    source: Account,
    destinationAccountId: number,
    amount: number,
    scheduledAt: string,
  ): void {
    if (!scheduledAt) {
      this.submitting.set(false);

      this.toast.warning(
        'Data obrigatória',
        'Informe quando a transferência deve ser executada.',
      );

      return;
    }

    const date = new Date(scheduledAt);

    if (
      Number.isNaN(date.getTime()) ||
      date <= new Date()
    ) {
      this.submitting.set(false);

      this.toast.warning(
        'Data inválida',
        'O agendamento deve ser realizado para uma data futura.',
      );

      return;
    }

    const payload: ScheduleTransferPayload = {
      sourceAccountId: source.id,
      destinationAccountId,
      amount,
      scheduledAt: date.toISOString(),
    };

    this.api
      .scheduleTransfer(payload)
      .pipe(
        finalize(() =>
          this.submitting.set(false),
        ),
      )
      .subscribe({
        next: (transfer) => {
          this.resultStore.save({
            success: true,
            mode: 'scheduled',
            sourceAccountId: source.id,
            destinationAccountId,
            transfer,
            generatedAt: new Date().toISOString(),
          });

          void this.router.navigate([
            '/contas',
            source.id,
            'resultado',
          ]);
        },
        error: (error: unknown) => {
          this.navigateToError(
            source.id,
            destinationAccountId,
            getApiErrorMessage(error),
          );
        },
      });
  }

  private navigateToError(
    sourceAccountId: number,
    destinationAccountId: number,
    message: string,
  ): void {
    this.resultStore.save({
      success: false,
      mode: this.mode,
      sourceAccountId,
      destinationAccountId,
      errorMessage: message,
      generatedAt: new Date().toISOString(),
    });

    void this.router.navigate([
      '/contas',
      sourceAccountId,
      'resultado',
    ]);
  }

  private isActive(
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
}