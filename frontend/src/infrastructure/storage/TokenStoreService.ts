import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TokenStoreService {
  private ACCESS_KEY  = 'access_token';
  private REFRESH_KEY = 'refresh_token';

  /** Salva os tokens no localStorage */
  save(access: string, refresh: string): void {
    localStorage.setItem(this.ACCESS_KEY, access);
    localStorage.setItem(this.REFRESH_KEY, refresh);
  }

  /** Retorna o access token ou null */
  getAccess(): string | null {
    return localStorage.getItem(this.ACCESS_KEY);
  }

  /** Retorna o refresh token ou null */
  getRefresh(): string | null {
    return localStorage.getItem(this.REFRESH_KEY);
  }

  /** Remove ambos tokens (por logout, por exemplo) */
  clear(): void {
    localStorage.removeItem(this.ACCESS_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
  }
}
