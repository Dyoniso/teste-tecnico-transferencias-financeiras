import {
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { finalize, forkJoin } from 'rxjs';

import { AccountForm } from '../../components/account-form/account-form';
import { AccountList } from '../../components/account-list/account-list';
import { PersonForm } from '../../components/person-form/person-form';
import { PersonList } from '../../components/person-list/person-list';

import {
  Account,
  Person,
  PersonPayload,
} from '../../models/api.models';

import {
  FinancialApiService,
  getApiErrorMessage,
} from '../../services/financial-api.service';

@Component({
  selector: 'app-dashboard',
  imports: [
    PersonList,
    PersonForm,
    AccountList,
    AccountForm,
  ],
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  private readonly api = inject(FinancialApiService);

  protected readonly activeTab =
    signal<'persons' | 'accounts'>('persons');

  protected readonly persons = signal<Person[]>([]);
  protected readonly accounts = signal<Account[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  protected readonly personModal = signal(false);
  protected readonly accountModal = signal(false);

  protected readonly editingPerson =
    signal<Person | null>(null);

  protected readonly editingAccount =
    signal<Account | null>(null);

  protected readonly selectedPersonId =
    signal<number | null>(null);

  ngOnInit(): void {
    this.loadData();
  }

  protected loadData(): void {
    this.loading.set(true);
    this.error.set(null);

    forkJoin({
      persons: this.api.listPersons(),
      accounts: this.api.listAccounts(),
    })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: ({ persons, accounts }) => {
          this.persons.set(persons);
          this.accounts.set(accounts);
        },
        error: (error) => {
          this.error.set(getApiErrorMessage(error));
        },
      });
  }

  protected openPerson(person: Person | null = null): void {
    this.editingPerson.set(person);
    this.personModal.set(true);
  }

  protected openAccount(
    account: Account | null = null,
    personId: number | null = null,
  ): void {
    this.editingAccount.set(account);
    this.selectedPersonId.set(personId);
    this.accountModal.set(true);
  }

  protected savePerson(payload: PersonPayload): void {
    const person = this.editingPerson();

    const request = person
      ? this.api.updatePerson(person.id, payload)
      : this.api.createPerson(payload);

    request.subscribe({
      next: () => {
        this.personModal.set(false);
        this.success.set('Pessoa salva com sucesso.');
        this.loadData();
      },
      error: (error) => {
        this.error.set(getApiErrorMessage(error));
      },
    });
  }

  protected saveAccount(
    result: {
      create?: any;
      update?: any;
    },
  ): void {
    const account = this.editingAccount();

    const request = account && result.update
      ? this.api.updateAccount(account.id, result.update)
      : this.api.createAccount(result.create);

    request.subscribe({
      next: () => {
        this.accountModal.set(false);
        this.success.set('Conta salva com sucesso.');
        this.loadData();
      },
      error: (error) => {
        this.error.set(getApiErrorMessage(error));
      },
    });
  }

  protected deletePerson(person: Person): void {
    const confirmed = window.confirm(
      `Deseja realmente excluir ${person.name}?`,
    );

    if (!confirmed) return;

    this.api.deletePerson(person.id).subscribe({
      next: () => {
        this.success.set('Pessoa excluída.');
        this.loadData();
      },
      error: (error) => {
        this.error.set(getApiErrorMessage(error));
      },
    });
  }

  protected deleteAccount(account: Account): void {
    const confirmed = window.confirm(
      `Deseja excluir a conta #${account.id}?`,
    );

    if (!confirmed) return;

    this.api.deleteAccount(account.id).subscribe({
      next: () => {
        this.success.set('Conta excluída.');
        this.loadData();
      },
      error: (error) => {
        this.error.set(getApiErrorMessage(error));
      },
    });
  }
}