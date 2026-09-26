import {
  Component,
  inject,
  output,
} from '@angular/core';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

@Component({
  selector: 'app-transfer-query',
  imports: [ReactiveFormsModule],
  templateUrl: './transfer-query.html',
})
export class TransferQuery {
  private readonly formBuilder =
    inject(NonNullableFormBuilder);

  readonly search = output<string>();

  protected readonly form = this.formBuilder.group({
    id: ['', Validators.required],
  });

  protected submit(): void {
    if (this.form.invalid) return;

    this.search.emit(this.form.getRawValue().id);
  }
}