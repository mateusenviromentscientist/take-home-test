import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { tap, Observable } from 'rxjs';
import { environment } from '../enviroment';
import { UserSession } from '../Models/Requests/Auth/user-session';
import { CreateUserRequest } from '../Models/Requests/Auth/create-user-request';
import { GetTokenRequest } from '../Models/Requests/Auth/get-token-request';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.baseUrl}api/auth`;
  private readonly STORAGE_KEY = 'access_token';

  private _session = signal<UserSession | null>(this.load());

  readonly session = computed(() => this._session());
  readonly isAuthenticated = computed(() => !!this.getToken());

  constructor(private http: HttpClient) {}

  register(payload: CreateUserRequest): Observable<string> {
    return this.http.post(`${this.baseUrl}/register`, payload, { responseType: 'text' });
  }

  login(payload: GetTokenRequest): Observable<string> {
    const token = this.getToken();

    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    });

    return this.http
      .post(`${this.baseUrl}/login`, payload, { headers, responseType: 'text' })
      .pipe(tap(t => this.setToken(t)));
  }

  logout(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    this._session.set(null);
  }

  getToken(): string | null {
  const raw = this._session()?.token;

  if (!raw) return null;
  if (typeof raw !== 'string') {
    const anyRaw = raw as any;
    return typeof anyRaw?.token === 'string' ? anyRaw.token : null;
  }
  const match = raw.match(/([A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+)/);
  return match ? match[1] : raw;
}



  private setToken(token: string): void {
  const cleanToken = token.replace(/"/g, '').trim();
  localStorage.setItem(this.STORAGE_KEY, cleanToken);
  this._session.set({ token: cleanToken });
 }


  private load(): UserSession | null {
    const token = localStorage.getItem(this.STORAGE_KEY);
    return token ? ({ token } as UserSession) : null;
  }
}
