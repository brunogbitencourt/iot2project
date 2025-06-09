import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h2>Usuários</h2>
    <p>Aqui você verá a lista de usuários.</p>
  `
})
export class UsuariosComponent {}
