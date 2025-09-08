export interface Doctor {
  id?: number;
  name: string;
  city: string;
  specializationId: number;
  rating: number;
  startTime: string;
  endTime: string;
  slotDurationMinutes: number;
  profileImagePath?: string;
}
