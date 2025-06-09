export class Token {
  constructor(public readonly value: string) {
    if (!value) throw new Error('Token vazio');
  }
}
