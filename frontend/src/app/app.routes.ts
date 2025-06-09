// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { LoginComponent }           from '../presentation/components/login/login.component';
import { AppComponent }             from './app.component';
import { MainComponent }            from '../presentation/components/main/main.component';
import { DispositivosComponent }    from '../presentation/components/dispositivos/dispositivos.component';
import { UsuariosPageComponent }    from '../presentation/components/usuarios/usuarios-page.component';
import { PlantaIndustrialComponent } from '../presentation/components/planta-industrial/planta-industrial.component';

export const routes: Routes = [
  // Raiz -> login ou main direto? Se quiser pular login, vamos direto ao main:
  { path: '', redirectTo: 'main', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },

  {
    path: 'main',
    component: MainComponent,
    children: [
      { path: '', redirectTo: 'dispositivos', pathMatch: 'full' },
      { path: 'dispositivos', component: DispositivosComponent },
      { path: 'usuarios',     component: UsuariosPageComponent },
      { path: 'planta-industrial', component: PlantaIndustrialComponent }
    ]
  },

  // Qualquer outra URL:
  { path: '**', redirectTo: 'main' }
];
