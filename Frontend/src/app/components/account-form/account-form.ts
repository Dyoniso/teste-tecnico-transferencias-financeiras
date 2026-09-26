import {
  Component,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import {
  Account,
  AccountStatus,
  CreateAccountPayload,
  Person,
  UpdateAccountPayload,
} from '../../models/api.models';

export interface AccountFormResult {
  create?: CreateAccountPayload;
  update?: UpdateAccountPayload;
}

@Component({
  selector: 'app-account-form',
  imports: [ReactiveFormsModule],
  templateUrl: './account-form.html',
})
export class AccountForm {
  private readonly formBuilder =
    inject(NonNullableFormBuilder);

  readonly persons = input.required<Person[]>();
  readonly account = input<Account | null>(null);
  readonly selectedPersonId = input<number | null>(null);

  readonly save = output<AccountFormResult>();
  readonly close = output<void>();

  readonly status = AccountStatus;

  readonly form = this.formBuilder.group({
    personId: [0, [Validators.required, Validators.min(1)]],
    balance: [0, [Validators.required, Validators.min(0)]],
    overdraftLimit: [
      0,
      [Validators.required, Validators.min(0)],
    ],
    status: [AccountStatus.Active, Validators.required],
  });

  constructor() {
    effect(() => {
      const account = this.account();

      if (account) {
        this.form.patchValue({
          personId: account.personId,
          balance: account.balance,
          overdraftLimit: account.overdraftLimit,
          status: this.statusValue(account.status),
        });

        this.form.controls.personId.disable();
        this.form.controls.balance.disable();
        return;
      }

      this.form.controls.personId.enable();
      this.form.controls.balance.enable();

      this.form.reset({
        personId: this.selectedPersonId() ?? 0,
        balance: 0,
        overdraftLimit: 0,
        status: AccountStatus.Active,
      });
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    if (this.account()) {
      this.save.emit({
        update: {
          overdraftLimit: value.overdraftLimit,
          status: value.status,
        },
      });

      return;
    }

    this.save.emit({
      create: {
        personId: value.personId,
        balance: value.balance,
        overdraftLimit: value.overdraftLimit,
        status: value.status,
      },
    });
  }

  private statusValue(status: string | null): AccountStatus {
    const normalized = status?.toLowerCase();

    if (normalized === 'blocked') return AccountStatus.Blocked;
    if (normalized === 'inactive') return AccountStatus.Inactive;

    return AccountStatus.Active;
  }
}