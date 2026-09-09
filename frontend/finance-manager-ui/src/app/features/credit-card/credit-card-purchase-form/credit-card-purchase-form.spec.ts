import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditCardPurchaseForm } from './credit-card-purchase-form';

describe('CreditCardPurchaseForm', () => {
  let component: CreditCardPurchaseForm;
  let fixture: ComponentFixture<CreditCardPurchaseForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreditCardPurchaseForm],
    }).compileComponents();

    fixture = TestBed.createComponent(CreditCardPurchaseForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
