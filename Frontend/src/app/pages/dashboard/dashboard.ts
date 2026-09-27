import {
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { finalize, forkJoin } from 'rxjs';
import {
  LucidePlus,
  LucideUsers,
  LucideWalletCards,
} from '@lucide/angular';

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
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-dashboard',
  imports: [
    PersonList,
    PersonForm,
    AccountList,
    AccountForm,
    LucidePlus,
    LucideUsers,
    LucideWalletCards,
  ],
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  private readonly api = inject(FinancialApiService);
  private readonly toast = inject(ToastService);

  protected readonly activeTab =
    signal<'persons' | 'accounts'>('persons');

  protected readonly persons = signal<Person[]>([]);
  protected readonly accounts = signal<Account[]>([]);
  protected readonly loading = signal(true);

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
          this.showRequestError(error);
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
        this.toast.success(
          'Pessoa salva',
          'Pessoa salva com sucesso.',
        );
        this.loadData();
      },
      error: (error) => {
        this.showRequestError(error);
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
        this.toast.success(
          'Conta salva',
          'Conta salva com sucesso.',
        );
        this.loadData();
      },
      error: (error) => {
        this.showRequestError(error);
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
        this.toast.success(
          'Pessoa excluída',
          'Pessoa excluída com sucesso.',
        );
        this.loadData();
      },
      error: (error) => {
        this.showRequestError(error);
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
        this.toast.success(
          'Conta excluída',
          'Conta excluída com sucesso.',
        );
        this.loadData();
      },
      error: (error) => {
        this.showRequestError(error);
      },
    });
  }

  private showRequestError(error: unknown): void {
    this.toast.error(
      'Não foi possível concluir a operação',
      getApiErrorMessage(error),
    );
  }
}