/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { Authinterceptor } from './auth.interceptor';

describe('Service: Auth.interceptor', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [Authinterceptor]
    });
  });

  it('should ...', inject([Authinterceptor], (service: Authinterceptor) => {
    expect(service).toBeTruthy();
  }));
});
