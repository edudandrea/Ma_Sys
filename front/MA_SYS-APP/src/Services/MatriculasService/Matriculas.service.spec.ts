/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { MatriculasService } from './Matriculas.service';

describe('Service: Matriculas', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MatriculasService]
    });
  });

  it('should ...', inject([MatriculasService], (service: MatriculasService) => {
    expect(service).toBeTruthy();
  }));
});
