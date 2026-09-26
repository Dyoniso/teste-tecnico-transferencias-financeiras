import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';

import {
  provideHttpClient,
} from '@angular/common/http';

import {
  provideRouter,
  withViewTransitions,
} from '@angular/router';

import {
  provideLucideConfig,
} from '@lucide/angular';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),

    provideRouter(
      routes,
      withViewTransitions(),
    ),

    provideLucideConfig({
      size: 20,
    }),
  ],
};