import { InjectionToken } from '@angular/core';
import { Credentials } from '../../domain/entities/Credentials';

export interface AuthPort {
  /** Faz login e retorna um objeto com os dois tokens */
  login(credentials: Credentials): Promise<{ access: string; refresh: string }>;
}

/** Token de DI para injetar implementações de AuthPort */
export const AUTH_PORT = new InjectionToken<AuthPort>('AuthPort');
