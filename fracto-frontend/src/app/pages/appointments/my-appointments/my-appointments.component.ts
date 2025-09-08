import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppointmentService } from '../../../core/services/appointment.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-my-appointments',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './my-appointments.component.html',
  styleUrls: ['./my-appointments.component.scss']
})
export class MyAppointmentsComponent implements OnInit {
  appts: any[] = [];
  loading = true;
  error = '';

  constructor(private apptService: AppointmentService) {}

  ngOnInit() {
    const userId = Number(localStorage.getItem('userId') || '0');
    if (!userId) { this.error = 'Not logged in'; this.loading = false; return; }
    this.apptService.byUser(userId).subscribe({
      next: (res: any) => { this.appts = res; this.loading = false; },
      error: (err) => { console.error(err); this.error = 'Could not load appointments'; this.loading = false; }
    });
  }

  cancel(id: number) {
    if (!confirm('Cancel this appointment?')) return;
    this.apptService.cancel(id).subscribe({
      next: () => { this.appts = this.appts.filter(a => a.id !== id); },
      error: (err) => { console.error(err); alert('Cancel failed'); }
    });
  }
}
