import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DoctorService } from '../../../core/services/doctor.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-doctor-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './doctor-list.component.html',
  styleUrls: ['./doctor-list.component.scss']
})
export class DoctorListComponent implements OnInit {
  doctors: any[] = [];
  loading = true;
  error = '';

  constructor(private doctorService: DoctorService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading = true;
    this.doctorService.getAll().subscribe({
      next: (res: any) => { this.doctors = res; this.loading = false; },
      error: (err) => { console.error(err); this.error = 'Could not load doctors'; this.loading = false; }
    });
  }
    //  fallback when doctor image not found
  onImgError(event: Event) {
    (event.target as HTMLImageElement).src = '/assets/default.png';
  }
}
