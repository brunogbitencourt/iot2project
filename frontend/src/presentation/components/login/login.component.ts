// src/presentation/components/login/login.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthFacade } from '../../../application/facades/AuthFacade';
import { TokenStoreService } from '../../../infrastructure/storage/TokenStoreService';
import { Router } from '@angular/router';
import { AUTH_PORT } from '../../../application/ports/AuthPort';
import { AngularAuthAdapter } from '../../../infrastructure/http/AngularAuthAdapter';
import { AuthInterceptor } from '../../interceptors/AuthInterceptor';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  providers: [
    // Garante que, dentro deste componente e seus descendentes,
    // o AUTH_PORT está disponível e resolve para AngularAuthAdapter
    { provide: AUTH_PORT, useClass: AngularAuthAdapter },
    // E registra também o interceptor localmente
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ]
})
export class LoginComponent {
  form: FormGroup;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private auth: AuthFacade,
    private store: TokenStoreService,
    private router: Router
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  async onSubmit() {
    if (this.form.invalid) return;
    this.loading = true; this.error = null;
    try {
      const { access, refresh } = await this.auth.signIn(
        this.form.value.email!,
        this.form.value.password!
      );
      this.store.save(access.value, refresh.value);
      this.router.navigate(['/dashboard']);
    } catch (e: any) {
      this.error = e.message || 'Falha no login';
    } finally {
      this.loading = false;
    }
  }
}
