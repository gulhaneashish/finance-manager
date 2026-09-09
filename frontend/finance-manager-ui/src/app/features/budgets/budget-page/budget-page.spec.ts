import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BudgetPage } from './budget-page';

describe('BudgetPage', () => {
  let component: BudgetPage;
  let fixture: ComponentFixture<BudgetPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BudgetPage],
    }).compileComponents();

    fixture = TestBed.createComponent(BudgetPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
