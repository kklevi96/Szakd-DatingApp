import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Hobby } from '../models/hobby.models';

@Injectable({ providedIn: 'root' })
export class HobbyService {
  constructor(private http: HttpClient) {}
  private baseUrl = environment.apiUrl + '/hobbies';

  getAll(): Observable<Hobby[]> {
    return this.http.get<Hobby[]>(this.baseUrl);
  }
  getMy(): Observable<Hobby[]> {
    return this.http.get<Hobby[]>(this.baseUrl + '/my');
  }
  add(id: number): Observable<void> {
    return this.http.put<void>(this.baseUrl + '/' + id + '/add', {});
  }
  remove(id: number): Observable<void> {
    return this.http.delete<void>(this.baseUrl + '/' + id + '/remove');
  }
}
