import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-planta-industrial',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h2>Planta Industrial</h2>
    <p>Aqui você verá o painel da planta industrial.</p>
  `
})
export class PlantaIndustrialComponent {}
