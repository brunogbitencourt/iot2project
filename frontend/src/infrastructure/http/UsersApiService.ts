// src/infrastructure/http/UsersApiService.ts
import { Injectable } from '@angular/core';
import { HttpClient }   from '@angular/common/http';
import { environment }  from '../../environments/environment';
import { Observable }   from 'rxjs';

export interface UserDto {
  UserId: number;
  FullName: string;
  Email: string;
  UserProfileId: number;
  CreatedAt: string;
  UpdatedAt: string;
  IsDeleted: boolean;
}

// src/infrastructure/http/UsersApiService.ts
export interface CreateUserRequest {
  FullName: string;
  Email: string;
  Password: string;
  UserProfileId: number;
}

export interface UpdateUserRequest {
  FullName?: string;
  Email?: string;
  Password?: string;
  UserProfileId?: number;
}

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private base = `${environment.apiUrl}/api/users`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.base);
  }

  create(dto: CreateUserRequest): Observable<UserDto> {
    return this.http.post<UserDto>(this.base, dto);
  }

  update(id: number, dto: UpdateUserRequest): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
