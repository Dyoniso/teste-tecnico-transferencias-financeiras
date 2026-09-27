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
  LucideSave,
  LucideX,
} from '@lucide/angular';

import {
  Person,
  PersonPayload,
} from '../../models/api.models';

@Component({
  selector: 'app-person-form',
  imports: [
    ReactiveFormsModule,
    LucideSave,
    LucideX,
  ],
  templateUrl: './person-form.html',
})
export class PersonForm {
  private readonly formBuilder =
    inject(NonNullableFormBuilder);

  readonly person = input<Person | null>(null);
  readonly save = output<PersonPayload>();
  readonly close = output<void>();

  readonly form = this.formBuilder.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    document: ['', Validators.maxLength(20)],
    birthDate: [''],
    email: ['', [Validators.email, Validators.maxLength(150)]],
    phone: ['', Validators.maxLength(20)],
    zipCode: ['', Validators.maxLength(10)],
    street: ['', Validators.maxLength(150)],
    number: ['', Validators.maxLength(20)],
    complement: ['', Validators.maxLength(100)],
    neighborhood: ['', Validators.maxLength(100)],
    city: ['', Validators.maxLength(100)],
    state: ['', Validators.maxLength(2)],
  });

  constructor() {
    effect(() => {
      const person = this.person();

      if (!person) {
        this.form.reset();
        return;
      }

      this.form.patchValue({
        name: person.name ?? '',
        document: person.document ?? '',
        birthDate: person.birthDate ?? '',
        email: person.email ?? '',
        phone: person.phone ?? '',
        zipCode: person.zipCode ?? '',
        street: person.street ?? '',
        number: person.number ?? '',
        complement: person.complement ?? '',
        neighborhood: person.neighborhood ?? '',
        city: person.city ?? '',
        state: person.state ?? '',
      });
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    this.save.emit({
      name: value.name,
      document: value.document || null,
      birthDate: value.birthDate || null,
      email: value.email || null,
      phone: value.phone || null,
      zipCode: value.zipCode || null,
      street: value.street || null,
      number: value.number || null,
      complement: value.complement || null,
      neighborhood: value.neighborhood || null,
      city: value.city || null,
      state: value.state
        ? value.state.toUpperCase()
        : null,
    });
  }
}