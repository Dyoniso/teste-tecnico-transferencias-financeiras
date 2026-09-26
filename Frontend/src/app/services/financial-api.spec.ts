import { TestBed } from '@angular/core/testing';
import { FinancialApi } from './financial-api';

describe('FinancialApi', () => {
  let service: FinancialApi;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FinancialApi);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
