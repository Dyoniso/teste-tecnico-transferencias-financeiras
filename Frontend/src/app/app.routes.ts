import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/dashboard/dashboard').then(
        (component) => component.Dashboard,
      ),
    title: 'Pessoas e contas',
  },
  {
    path: 'contas/:id',
    loadComponent: () =>
      import(
        './pages/account-detail/account-detail'
      ).then(
        (component) => component.AccountDetail,
      ),
    title: 'Detalhes da conta',
  },
  {
    path: '**',
    redirectTo: '',
  },
];