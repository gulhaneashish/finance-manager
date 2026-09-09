import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditCardPaymentForm } from './credit-card-payment-form';

describe('CreditCardPaymentForm', () => {
  let component: CreditCardPaymentForm;
  let fixture: ComponentFixture<CreditCardPaymentForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreditCardPaymentForm],
    }).compileComponents();

    fixture = TestBed.createComponent(CreditCardPaymentForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
