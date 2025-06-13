// src/infrastructure/http/DevicesApiService.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable }   from 'rxjs';
import { environment }  from '../../environments/environment';

export interface DeviceDto {
  DeviceId: number;
  UserId: number;
  IsDeleted: boolean;
  CreatedAt: string;
  UpdatedAt: string;
  ConnectedPort: string;
  Name: string;
  Type: string;
  Category: string;
  Unit: string;
  MqttTopic: string;
  KafkaTopic: string;
}

export interface CreateDeviceRequest {
  UserId: number;
  ConnectedPort: string;
  Name: string;
  Type: string;
  Category: string;
  Unit: string;
  MqttTopic: string;
  KafkaTopic: string;
}

export interface UpdateDeviceRequest extends CreateDeviceRequest {}

@Injectable({ providedIn: 'root' })
export class DevicesApiService {
  private base = `${environment.apiUrl}/api/devices`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<DeviceDto[]> {
    return this.http.get<DeviceDto[]>(this.base);
  }

  getById(id: number): Observable<DeviceDto> {
    return this.http.get<DeviceDto>(`${this.base}/${id}`);
  }

  create(dto: CreateDeviceRequest): Observable<DeviceDto> {
    return this.http.post<DeviceDto>(this.base, dto);
  }

  update(id: number, dto: UpdateDeviceRequest): Observable<DeviceDto> {
    return this.http.put<DeviceDto>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
