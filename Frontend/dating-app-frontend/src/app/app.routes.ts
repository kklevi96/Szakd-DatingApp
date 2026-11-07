import { Routes } from '@angular/router';
import { Login } from './components/auth/login/login';
import { Home } from './components/home/home';
import { authGuard } from './core/guards/auth-guard';
import { guestGuard } from './core/guards/guest-guard';
import { Register } from './components/auth/register/register';
import { Logout } from './components/auth/logout/logout';
import { BecomeAdmin } from './components/auth/become-admin/become-admin';
import { Admin } from './components/admin/admin';
import { AuthGuardAdmin } from './core/guards/auth-guard-admin';

export const routes: Routes = [
  { path: 'login', component: Login, canActivate: [guestGuard] },
  {
    path: 'home',
    component: Home,
    canActivate: [authGuard]
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'register', component: Register, canActivate: [guestGuard] },
  { path: 'logout', component: Logout },
  { path: 'become-admin', component: BecomeAdmin, canActivate: [authGuard] },
  { path: 'admin', component: Admin, canActivate: [AuthGuardAdmin] },
];
