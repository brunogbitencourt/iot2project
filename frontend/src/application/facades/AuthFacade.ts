import { Inject, Injectable } from '@angular/core';
import { AUTH_PORT, AuthPort } from '../ports/AuthPort';
import { LoginUserUseCase } from '../../domain/useCases/LoginUserUseCase';
import { Token } from '../../domain/valueObjects/Token';

@Injectable({ providedIn: 'root' })
export class AuthFacade {
  constructor(@Inject(AUTH_PORT) private authAdapter: AuthPort) {}

  /** Retorna os tokens embrulhados em Value Objects */
  async signIn(
    email: string,
    password: string
  ): Promise<{ access: Token; refresh: Token }> {
    const { access, refresh } = await this.authAdapter.login({ email, password });
    return {
      access: new Token(access),
      refresh: new Token(refresh)
    };
  }
}
