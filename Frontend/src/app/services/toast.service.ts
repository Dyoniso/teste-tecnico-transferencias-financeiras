import {
  Injectable,
  signal,
} from '@angular/core';

export type ToastType =
  | 'success'
  | 'error'
  | 'warning'
  | 'info';

export interface ToastMessage {
  id: number;
  type: ToastType;
  title: string;
  message: string;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private readonly itemsSignal =
    signal<ToastMessage[]>([]);

  readonly items = this.itemsSignal.asReadonly();

  private nextId = 1;

  success(
    title: string,
    message: string,
  ): void {
    this.show('success', title, message);
  }

  error(
    title: string,
    message: string,
  ): void {
    this.show('error', title, message, 7000);
  }

  warning(
    title: string,
    message: string,
  ): void {
    this.show('warning', title, message, 6000);
  }

  info(
    title: string,
    message: string,
  ): void {
    this.show('info', title, message);
  }

  remove(id: number): void {
    this.itemsSignal.update((items) =>
      items.filter((item) => item.id !== id),
    );
  }

  private show(
    type: ToastType,
    title: string,
    message: string,
    duration = 4500,
  ): void {
    const id = this.nextId++;

    this.itemsSignal.update((items) => [
      ...items,
      {
        id,
        type,
        title,
        message,
      },
    ]);

    setTimeout(() => {
      this.remove(id);
    }, duration);
  }
}