/* tslint:disable:no-unused-variable */

import { TestBed, inject } from '@angular/core/testing';
import { AlunosService } from './Alunosservice';

describe('Service: AlunosService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AlunosService]
    });
  });

  it('should ...', inject([AlunosService], (service: AlunosService) => {
    expect(service).toBeTruthy();
  }));
});
