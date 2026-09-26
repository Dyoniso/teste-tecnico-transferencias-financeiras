import {
  Injectable,
} from '@angular/core';

import {
  Transfer,
} from '../models/api.models';

export type TransferOperationMode =
  | 'immediate'
  | 'scheduled';

export interface TransferFlowResult {
  success: boolean;
  mode: TransferOperationMode;
  sourceAccountId: number;
  destinationAccountId?: number;
  transfer?: Transfer;
  errorMessage?: string;
  generatedAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class TransferResultStoreService {
  private readonly storageKey =
    'financial-transfer-result';

  save(result: TransferFlowResult): void {
    sessionStorage.setItem(
      this.storageKey,
      JSON.stringify(result),
    );
  }

  read(): TransferFlowResult | null {
    const value = sessionStorage.getItem(
      this.storageKey,
    );

    if (!value) {
      return null;
    }

    try {
      return JSON.parse(value) as TransferFlowResult;
    } catch {
      return null;
    }
  }

  clear(): void {
    sessionStorage.removeItem(this.storageKey);
  }
}