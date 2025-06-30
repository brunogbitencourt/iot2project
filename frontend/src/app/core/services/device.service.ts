// src/app/core/services/device.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map }        from 'rxjs/operators';

export interface DeviceDto {
  deviceId: number;
  name:     string;
}

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private readonly base = 'http://localhost:5000/api/Devices';  // ou '/api/devices'

  constructor(private http: HttpClient) {}

  getAll(): Observable<DeviceDto[]> {
    return this.http.get<any[]>(`${this.base}`)
      .pipe(
        map(list => list.map(d => ({
          deviceId: d.DeviceId,
          name:     d.Name
        })))
      );
  }
}
