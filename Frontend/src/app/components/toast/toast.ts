import {
  Component,
  inject,
} from '@angular/core';

import {
  LucideCircleCheck,
  LucideCircleX,
  LucideInfo,
  LucideTriangleAlert,
  LucideX,
} from '@lucide/angular';

import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  imports: [
    LucideCircleCheck,
    LucideCircleX,
    LucideInfo,
    LucideTriangleAlert,
    LucideX,
  ],
  templateUrl: './toast.html',
  styleUrl: './toast.scss',
})
export class Toast {
  protected readonly toast = inject(ToastService);
}