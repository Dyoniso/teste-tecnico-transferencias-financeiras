import {
  Component,
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
  CreateTransferPayload,
  ScheduleTransferPayload,
} from '../../models/api.models';

@Component({
  selector: 'app-transfer-form',
  imports: [ReactiveFormsModule],
  templateUrl: './transfer-form.html',
})
export class TransferForm {
  private readonly formBuilder =
    inject(NonNullableFormBuilder);

  readonly sourceAccount =
    input.required<Account>();

  readonly accounts =
    input.required<Account[]>();

  readonly transfer =
    output<CreateTransferPayload>();

  readonly schedule =
    output<ScheduleTransferPayload>();

  protected readonly mode =
    this.formBuilder.control<'now' | 'scheduled'>('now');

  protected readonly form = this.formBuilder.group({
    destinationAccountId: [
      0,
      [Validators.required, Validators.min(1)],
    ],
    amount: [
      0,
      [Validators.required, Validators.min(0.01)],
    ],
    scheduledAt: [''],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    const payload = {
      sourceAccountId: this.sourceAccount().id,
      destinationAccountId:
        value.destinationAccountId,
      amount: value.amount,
    };

    if (this.mode.value === 'scheduled') {
      if (!value.scheduledAt) return;

      this.schedule.emit({
        ...payload,
        scheduledAt: new Date(
          value.scheduledAt,
        ).toISOString(),
      });

      return;
    }

    this.transfer.emit(payload);
  }
}