import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Location } from '../models/location.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LocationService {
  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<Location[]>(environment.apiUrl + '/locations');
  }
}
