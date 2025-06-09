import { Routes } from '@angular/router';
import { LoginComponent }                from '../presentation/components/login/login.component';
import { MainComponent }                 from '../presentation/components/main/main.component';
import { DispositivosComponent }         from '../presentation/components/dispositivos/dispositivos.component';
import { UsuariosComponent }             from '../presentation/components/usuarios/usuarios.component';
import { PlantaIndustrialComponent }     from '../presentation/components/planta-industrial/planta-industrial.component';

export const routes: Routes = [
  // Se ninguém apontar rota, vai direto p/ Main → Dispositivos
  { path: '', redirectTo: 'main', pathMatch: 'full' },

  // Login em /login se você quiser manter
  { path: 'login', component: LoginComponent },

  // Layout principal com menu
  {
    path: 'main',
    component: MainComponent,
    children: [
      // default filho: dispositivos
      { path: '', redirectTo: 'dispositivos', pathMatch: 'full' },
      { path: 'dispositivos', component: DispositivosComponent },
      { path: 'usuarios', component: UsuariosComponent },
      { path: 'planta-industrial', component: PlantaIndustrialComponent }
    ]
  },

  // qualquer outra URL cai no main
  { path: '**', redirectTo: 'main' }
];
