import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MonthlyReportPage } from './monthly-report-page';

describe('MonthlyReportPage', () => {
  let component: MonthlyReportPage;
  let fixture: ComponentFixture<MonthlyReportPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MonthlyReportPage],
    }).compileComponents();

    fixture = TestBed.createComponent(MonthlyReportPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
