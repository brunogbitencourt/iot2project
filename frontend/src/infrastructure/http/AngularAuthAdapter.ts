// src/infrastructure/http/AngularAuthAdapter.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AuthPort } from '../../application/ports/AuthPort';
import { Credentials } from '../../domain/entities/Credentials';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AngularAuthAdapter implements AuthPort {
  constructor(private http: HttpClient) {}

  login(credentials: Credentials): Promise<{ access: string; refresh: string }> {
    // firstValueFrom garante Promise<T> (não T|undefined)
    return firstValueFrom(
      this.http.post<{ token: string; refreshToken: string }>(
        `${environment.apiUrl}/auth/login`,
        { username: credentials.email, password: credentials.password }
      )
    ).then(data => {
      // aqui data já é do tipo {token: string; refreshToken: string}
      const { token, refreshToken } = data;
      if (!token || !refreshToken) {
        throw new Error('Resposta de login inválida');
      }
      return { access: token, refresh: refreshToken };
    });
  }
}
