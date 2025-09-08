
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Doctor } from '../../models/doctor';
import { Observable } from 'rxjs';
import { environment } from 'environments/environment'; // ✅ fixed correct import

@Injectable({ providedIn: 'root' })
export class DoctorService {
  private api = `${environment.apiUrl}/doctors`;

  constructor(private http: HttpClient) {}

  /** Get all doctors */
  getAll(): Observable<Doctor[]> {
    return this.http.get<Doctor[]>(this.api);
  }

  /** Get doctor by Id */
  getById(id: number): Observable<Doctor> {
    return this.http.get<Doctor>(`${this.api}/${id}`);
  }

  /** Add a doctor */
  add(doctor: Doctor) {
    return this.http.post(this.api, doctor);
  }

   /** Update a doctor */
  update(id: number, doctor: Doctor) {
    return this.http.put(`${this.api}/${id}`, doctor);
  }

  /** Delete a doctor */
  delete(id: number) {
    return this.http.delete(`${this.api}/${id}`);
  }

  /**  FIX: Get available timeslots for a doctor */
  getTimeslots(id: number, date: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.api}/${id}/timeslots?date=${date}`);
  }
}
