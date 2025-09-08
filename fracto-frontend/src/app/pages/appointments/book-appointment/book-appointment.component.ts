import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { DoctorService } from '../../../core/services/doctor.service';
import { AppointmentService } from '../../../core/services/appointment.service';

@Component({
  selector: 'app-book-appointment',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './book-appointment.component.html',
  styleUrls: ['./book-appointment.component.scss']
})
export class BookAppointmentComponent implements OnInit {
  doctorId = 0;
  doctor: any = null;
  date = '';
  slots: string[] = [];
  selectedSlot = '';
  message = '';
  error = '';

  constructor(
    private route: ActivatedRoute,
    private doctorService: DoctorService,
    private apptService: AppointmentService,
    private router: Router
  ) {}

  ngOnInit() {
    this.doctorId = Number(this.route.snapshot.paramMap.get('id'));
    this.doctorService.getById(this.doctorId).subscribe(d => this.doctor = d);
  }

  loadSlots() {
    this.error = '';
    if (!this.date) { this.error = 'Please pick a date'; return; }
    this.doctorService.getById(this.doctorId).subscribe({
      next: () => {
        this.doctorService.getTimeslots(this.doctorId, this.date).subscribe({
          next: (s: any) => this.slots = s,
          error: () => this.error = 'Could not load slots'
        });
      },
      error: () => this.error = 'Doctor not found'
    });
  }

  book() {
    this.error = '';
    if (!this.selectedSlot) { this.error = 'Please select a slot'; return; }
    const userId = Number(localStorage.getItem('userId') || '0');
    if (!userId) { this.error = 'You must be logged in to book'; return; }

    const payload = {
      userId,
      doctorId: this.doctorId,
      appointmentDate: this.date,
      timeSlot: this.selectedSlot
    };

    this.apptService.book(payload).subscribe({
      next: () => {
        this.message = 'Appointment booked successfully';
        setTimeout(()=> this.router.navigate(['/my-appointments']), 1200);
      },
      error: (err) => { console.error(err); this.error = err?.error || 'Booking failed'; }
    });
  }
}
