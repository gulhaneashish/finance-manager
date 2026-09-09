import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NetWorthPage } from './net-worth-page';

describe('NetWorthPage', () => {
  let component: NetWorthPage;
  let fixture: ComponentFixture<NetWorthPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NetWorthPage],
    }).compileComponents();

    fixture = TestBed.createComponent(NetWorthPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
