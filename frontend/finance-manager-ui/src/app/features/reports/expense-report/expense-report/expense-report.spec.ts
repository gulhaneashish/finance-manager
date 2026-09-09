import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExpenseReport } from './expense-report';

describe('ExpenseReport', () => {
  let component: ExpenseReport;
  let fixture: ComponentFixture<ExpenseReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExpenseReport],
    }).compileComponents();

    fixture = TestBed.createComponent(ExpenseReport);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
