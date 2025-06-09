export class Credentials {
  constructor(
    public readonly email: string,
    public readonly password: string
  ) {
    if (!email.includes('@')) throw new Error('E-mail inválido');
    if (password.length < 6) throw new Error('Senha muito curta');
  }
}
