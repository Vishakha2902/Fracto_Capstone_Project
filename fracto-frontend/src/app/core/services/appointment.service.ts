

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'environments/environment';
import { Observable } from 'rxjs';

// App-level models (adjust if you already have these elsewhere)
export interface Appointment {
  id?: number;
  userId: number;
  doctorId: number;
  appointmentDate: string; // ISO date string e.g. '2025-09-04'
  timeSlot: string;        // e.g. '10:00-10:30'
  status?: 'Booked' | 'Pending' | 'Approved' | 'Cancelled' | string;
}

export interface AdminAppointment {
  id: number;
  userId: number;
  doctorId: number;
  appointmentDate: string;
  timeSlot: string;
  status: 'Booked' | 'Pending' | 'Approved' | 'Cancelled' | string;
}

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private api = `${environment.apiUrl}/appointments`;

  constructor(private http: HttpClient) {}

  /** User/Admin: book an appointment */
  book(appt: Appointment): Observable<{ id: number; status: string }> {
    return this.http.post<{ id: number; status: string }>(this.api, appt);
  }

  /** User/Admin: cancel appointment (matches PUT /appointments/cancel/{id}) */
  cancel(id: number): Observable<void> {
    return this.http.put<void>(`${this.api}/cancel/${id}`, {});
  }

  /** User/Admin: list appointments for a user */
  byUser(userId: number): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.api}/user/${userId}`);
  }

  // -------- Admin endpoints --------

  /** Admin: get all appointments */
  getAll(): Observable<AdminAppointment[]> {
    return this.http.get<AdminAppointment[]>(`${this.api}`);
  }

  /** Admin: get pending appointments (requires GET /appointments/pending on backend) */
  getPending(): Observable<AdminAppointment[]> {
    return this.http.get<AdminAppointment[]>(`${this.api}/pending`);
  }

  /** Admin: approve an appointment */
  approve(id: number): Observable<void> {
    return this.http.put<void>(`${this.api}/approve/${id}`, {});
  }
}
