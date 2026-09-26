import {
  Component,
  input,
  output,
} from '@angular/core';
import { RouterLink } from '@angular/router';

import { Account } from '../../models/api.models';

@Component({
  selector: 'app-account-list',
  imports: [RouterLink],
  templateUrl: './account-list.html',
})
export class AccountList {
  readonly accounts = input.required<Account[]>();

  readonly edit = output<Account>();
  readonly remove = output<Account>();

  protected currency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  }
}