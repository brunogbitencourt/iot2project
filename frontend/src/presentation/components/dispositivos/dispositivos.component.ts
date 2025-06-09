import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dispositivos',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h2>Dispositivos</h2>
    <p>Aqui você verá a lista de dispositivos.</p>
  `
})
export class DispositivosComponent {}
