import { Component } from '@angular/core';
import {
  RouterLink,
  RouterOutlet,
} from '@angular/router';

import { Toast } from './components/toast/toast';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    RouterLink,
    Toast,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}