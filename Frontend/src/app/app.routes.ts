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
    path: 'contas/:id/transferir',
    loadComponent: () =>
      import(
        './pages/transfer-operation/transfer-operation'
      ).then(
        (component) => component.TransferOperation,
      ),
    data: {
      mode: 'immediate',
    },
    title: 'Realizar transferência',
  },
  {
    path: 'contas/:id/agendar',
    loadComponent: () =>
      import(
        './pages/transfer-operation/transfer-operation'
      ).then(
        (component) => component.TransferOperation,
      ),
    data: {
      mode: 'scheduled',
    },
    title: 'Agendar transferência',
  },
  {
    path: 'contas/:id/resultado',
    loadComponent: () =>
      import(
        './pages/transfer-result/transfer-result'
      ).then(
        (component) => component.TransferResult,
      ),
    title: 'Resultado da transferência',
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