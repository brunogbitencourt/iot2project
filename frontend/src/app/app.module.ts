// app.component.ts
import { Component }    from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { BrowserModule }from '@angular/platform-browser';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    BrowserModule,
    RouterOutlet
  ],
  template: `<router-outlet></router-outlet>`
})
export class AppComponent {}
