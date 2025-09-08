import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class RatingService {
  private api = `${environment.apiUrl}/ratings`;
  constructor(private http: HttpClient) {}
  rate(doctorId: number, score: number) { return this.http.post(this.api, { doctorId, score }); }
  getAverage(doctorId: number) { return this.http.get<{ doctorId: number; average: number }>(`${this.api}/doctor/${doctorId}/avg`); }
  getDoctorRatings(doctorId: number) { return this.http.get(`${this.api}/doctor/${doctorId}`); }
}
