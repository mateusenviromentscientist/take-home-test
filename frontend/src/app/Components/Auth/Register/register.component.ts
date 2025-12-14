import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../Services/auth-service';



@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    const { email, password } = this.form.getRawValue();

    this.auth.register({ email, password }).subscribe({
      next: (msg) => {
        this.loading.set(false);
        const text =
          typeof msg === 'string' && msg.trim().length > 0
            ? msg
            : 'Usuário criado com sucesso. Faça login.';

        this.success.set(text);
        setTimeout(() => this.router.navigateByUrl('/login'), 500);
      },
      error: (err) => {
        this.loading.set(false);

        const msg =
          err?.error && typeof err.error === 'string'
            ? err.error
            : err?.message ?? 'Falha ao registrar.';

        this.error.set(msg);
      },
    });
  }

  get email() { return this.form.controls.email; }
  get password() { return this.form.controls.password; }
}
