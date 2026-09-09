import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MonthlyFinancePage } from './monthly-finance-page';

describe('MonthlyFinancePage', () => {
  let component: MonthlyFinancePage;
  let fixture: ComponentFixture<MonthlyFinancePage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MonthlyFinancePage],
    }).compileComponents();

    fixture = TestBed.createComponent(MonthlyFinancePage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
