import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-main',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="layout">
      <header class="navbar">
        <nav class="nav">
          <a routerLink="dispositivos" routerLinkActive="active">Dispositivos</a>
          <a routerLink="usuarios"       routerLinkActive="active">Usuários</a>
          <a routerLink="planta-industrial" routerLinkActive="active">Planta Industrial</a>
          <a routerLink="visualization"   routerLinkActive="active">Visualização</a>
          </nav>
      </header>
      <main class="content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styleUrls: ['./main.component.scss']
})
export class MainComponent {}
