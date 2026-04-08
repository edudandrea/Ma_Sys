/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { ModalidadesService } from './Modalidades.service';

describe('Service: Modalidades', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ModalidadesService]
    });
  });

  it('should ...', inject([ModalidadesService], (service: ModalidadesService) => {
    expect(service).toBeTruthy();
  }));
});
