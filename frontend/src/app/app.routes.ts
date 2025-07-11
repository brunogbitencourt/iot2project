// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { LoginComponent }           from '../presentation/components/login/login.component';
import { AppComponent }             from './app.component';
import { MainComponent }            from '../presentation/components/main/main.component';
import { DevicesPageComponent }    from '../presentation/components/devices/devices-page.component';
import { UsuariosPageComponent }    from '../presentation/components/usuarios/usuarios-page.component';
import { IndustrialPlantComponent } from '../presentation/components/industrial-plant/industrial-plant.component';
import { VisualizationPageComponent } from '../presentation/components/visualization-page/visualization-page.component';
//
export const routes: Routes = [
  // Raiz -> login ou main direto? Se quiser pular login, vamos direto ao main:
  { path: '', redirectTo: 'main', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },

  {
    path: 'main',
    component: MainComponent,
    children: [
      { path: '', redirectTo: 'dispositivos', pathMatch: 'full' },
      { path: 'dispositivos', component: DevicesPageComponent },
      { path: 'usuarios',     component: UsuariosPageComponent },
      { path: 'planta-industrial', component: IndustrialPlantComponent },
      { path: 'visualization', component: VisualizationPageComponent }
    ]
  },

  // Qualquer outra URL:
  { path: '**', redirectTo: 'main' }
];
