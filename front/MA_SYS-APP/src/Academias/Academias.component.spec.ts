/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { AcademiasComponent } from './Academias.component';

describe('AcademiasComponent', () => {
  let component: AcademiasComponent;
  let fixture: ComponentFixture<AcademiasComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AcademiasComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AcademiasComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
