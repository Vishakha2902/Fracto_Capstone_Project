

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DoctorService } from '../../../core/services/doctor.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-manage-doctors',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-doctors.component.html',
  styleUrls: ['./manage-doctors.component.scss']
})
export class ManageDoctorsComponent implements OnInit {
  doctors: any[] = [];
  loading = true;
  error = '';

  // form model
  model: any = { 
    name: '', 
    city: '', 
    specializationId: 1, 
    rating: 4.0, 
    startTime: '09:00:00', 
    endTime: '17:00:00', 
    slotDurationMinutes: 30 
  };

  editingDoctor: any = null; // if set, we're in edit mode

  constructor(private doctorService: DoctorService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading = true;
    this.doctorService.getAll().subscribe({
      next: (res: any) => { 
        this.doctors = res; 
        this.loading = false; 
      },
      error: (err) => { 
        console.error(err); 
        this.error = 'Load failed'; 
        this.loading = false; 
      }
    });
  }

  add() {
    this.doctorService.add(this.model).subscribe({
      next: () => { 
        this.resetForm();
        this.load(); 
      },
      error: (err) => { 
        console.error(err); 
        alert('Add failed'); 
      }
    });
  }

  edit(d: any) {
    this.editingDoctor = d;
    this.model = { ...d }; // copy values to form
  }

  update() {
    if (!this.editingDoctor) return;

    this.doctorService.update(this.editingDoctor.id, this.model).subscribe({
      next: () => {
        this.resetForm();
        this.editingDoctor = null;
        this.load();
      },
      error: (err) => {
        console.error(err);
        alert('Update failed');
      }
    });
  }

  deleteDoctor(id: number) {
    if (confirm('Are you sure you want to delete this doctor?')) {
      this.doctorService.delete(id).subscribe({
        next: () => this.load(),
        error: (err) => {
          console.error(err);
          alert('Delete failed');
        }
      });
    }
  }

  resetForm() {
    this.model = { 
      name: '', 
      city: '', 
      specializationId: 1, 
      rating: 4.0, 
      startTime: '09:00:00', 
      endTime: '17:00:00', 
      slotDurationMinutes: 30 
    };
  }
}
