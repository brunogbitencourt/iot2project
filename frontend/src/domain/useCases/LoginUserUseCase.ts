import { AuthPort } from '@/application/ports/AuthPort';
import { Credentials } from '../entities/Credentials';

export class LoginUserUseCase {
  constructor(private authPort: AuthPort) {}

  async execute(email: string, password: string): Promise<string> {
    const creds = new Credentials(email, password);
    const token = await this.authPort.login(creds);
    return token.access;  // retorna apenas o access token
  }
}
