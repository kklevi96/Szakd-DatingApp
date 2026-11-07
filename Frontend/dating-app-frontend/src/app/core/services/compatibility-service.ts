import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Compatibility } from '../models/compatibility.models';

@Injectable({ providedIn: 'root' })
export class CompatibilityService {
  constructor(private http: HttpClient) {}
  private baseUrl = environment.apiUrl + '/compatibilities';

  generateMy(): Observable<{ createdCount: number }> {
    return this.http.post<{ createdCount: number }>(this.baseUrl + '/generate-my', {});
  }

  getMy(): Observable<Compatibility[]> {
    return this.http.get<Compatibility[]>(this.baseUrl + '/my');
  }
}
