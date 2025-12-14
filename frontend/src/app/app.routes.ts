import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./Components/Auth/Login/login.component')
        .then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./Components/Auth/Register/register.component')
        .then(m => m.RegisterComponent),
  },
  {
    path: 'loans',
    loadComponent: () =>
      import('../app/Components/Loans/GetAllLoans/get-all-loans.component')
        .then(m => m.LoansComponent),
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  { path: '**', redirectTo: 'login' },
];
