/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { AcademiasService } from './Academias.service';

describe('Service: Academias', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AcademiasService]
    });
  });

  it('should ...', inject([AcademiasService], (service: AcademiasService) => {
    expect(service).toBeTruthy();
  }));
});
