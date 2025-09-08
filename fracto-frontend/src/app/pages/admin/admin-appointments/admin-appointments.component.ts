import { Component, OnDestroy, OnInit } from '@angular/core';
import { AppointmentService } from '../../../core/services/appointment.service';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';

interface AdminAppointment {
  id: number;
  userId: number;
  doctorId: number;
  appointmentDate: string; // ISO string returned by API; use Date pipe in template
  timeSlot: string;
  status: 'Booked' | 'Pending' | 'Approved' | 'Cancelled' | string;
}

@Component({
  selector: 'app-admin-appointments',
  templateUrl: './admin-appointments.component.html',
  styleUrls: ['./admin-appointments.component.scss']
})
export class AdminAppointmentsComponent implements OnInit, OnDestroy {
  pending: AdminAppointment[] = [];
  all: AdminAppointment[] = [];

  loadingPending = false;
  loadingAll = false;

  actionBusy: Record<number, boolean> = {};
  errorPending = '';
  errorAll = '';
  actionError: Record<number, string> = {};

  private destroy$ = new Subject<void>();

  constructor(private apptSvc: AppointmentService) {}

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  load(): void {
    this.loadPending();
    this.loadAll();
  }

  loadPending(): void {
    this.errorPending = '';
    this.loadingPending = true;
    this.apptSvc.getPending()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.loadingPending = false))
      )
      .subscribe({
        next: (r: AdminAppointment[]) => (this.pending = r),
        error: (err) => (this.errorPending = this.readError(err))
      });
  }

  loadAll(): void {
    this.errorAll = '';
    this.loadingAll = true;
    this.apptSvc.getAll()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.loadingAll = false))
      )
      .subscribe({
        next: (r: AdminAppointment[]) => (this.all = r),
        error: (err) => (this.errorAll = this.readError(err))
      });
  }

  approve(id: number): void {
    this.actionError[id] = '';
    this.actionBusy[id] = true;
    this.apptSvc.approve(id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.actionBusy[id] = false))
      )
      .subscribe({
        next: () => this.load(), // reload both tables
        error: (err) => (this.actionError[id] = this.readError(err))
      });
  }

  cancel(id: number): void {
    this.actionError[id] = '';
    this.actionBusy[id] = true;
    this.apptSvc.cancel(id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.actionBusy[id] = false))
      )
      .subscribe({
        next: () => this.load(),
        error: (err) => (this.actionError[id] = this.readError(err))
      });
  }

  trackById(_: number, a: AdminAppointment): number {
    return a.id;
  }

  private readError(err: any): string {
    if (err?.error?.message) return err.error.message;
    if (typeof err?.error === 'string') return err.error;
    return 'Something went wrong. Please try again.';
  }
}
