import { Component } from '@angular/core';
import {
  RouterLink,
  RouterOutlet,
} from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  template: `
    <header class="app-header">
      <a routerLink="/" class="brand">
        <span class="brand-icon">TF</span>

        <span>
          <strong>Transferências</strong>
          <small>Gestão financeira</small>
        </span>
      </a>
    </header>

    <router-outlet />
  `,
})
export class App {}