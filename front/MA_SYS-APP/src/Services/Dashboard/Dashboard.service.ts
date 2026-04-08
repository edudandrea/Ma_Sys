import { Injectable } from '@angular/core';
import { environment } from '../../app/environments/environment.prod';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DashboardService{
  private apiUrl = `${environment.apiUrl}/Dashboard`;

constructor(private http: HttpClient) { }

getDashboard(): Observable<any> {
  return this.http.get<any>(this.apiUrl);
}

}
