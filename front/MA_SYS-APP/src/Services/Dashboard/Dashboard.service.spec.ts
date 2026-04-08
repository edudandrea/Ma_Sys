/* tslint:disable:no-unused-variable */

import { TestBed, waitForAsync, inject } from '@angular/core/testing';
import { DashboardService } from './Dashboard.service';

describe('Service: DashboardService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DashboardService]
    });
  });

  it('should ...', inject([DashboardService], (service: DashboardService) => {
    expect(service).toBeTruthy();
  }));
});
