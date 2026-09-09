import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoanPaymentForm } from './loan-payment-form';

describe('LoanPaymentForm', () => {
  let component: LoanPaymentForm;
  let fixture: ComponentFixture<LoanPaymentForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanPaymentForm],
    }).compileComponents();

    fixture = TestBed.createComponent(LoanPaymentForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
