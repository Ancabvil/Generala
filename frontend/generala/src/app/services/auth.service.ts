import { Injectable } from '@angular/core';
import { AuthRequest } from '../models/auth-request';
import { AuthResponse } from '../models/auth-response';
import { Result } from '../models/result';
import { ApiService } from './api.service';
import { User } from '../models/user';
import { WebsocketService } from './websocket.service';
import { Observable } from 'rxjs';
import { Friendship } from '../models/friendship';
import { FriendService } from './friend.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly USER_KEY = 'user';
  private readonly TOKEN_KEY = 'jwtToken';

  constructor(
    private api: ApiService,
    private websocketService: WebsocketService
  ) {
    const token = this.getToken();
    if (token) {
      this.api.jwt = token;
      this.websocketService.connect(token);
    }
  }

  async signup(formData: any): Promise<Result<any>> {
    return this.api.post<any>('Auth/Signup', formData);
  }

  async login(authData: AuthRequest, rememberMe: boolean): Promise<Result<AuthResponse>> {
    const result = await this.api.post<AuthResponse>('Auth/login', authData);

    if (result.success) {
      const { accessToken, user } = result.data;
      this.api.jwt = accessToken;

      if (rememberMe) {
        localStorage.setItem(this.TOKEN_KEY, accessToken);
        localStorage.setItem(this.USER_KEY, JSON.stringify(user));
      } else {
        sessionStorage.setItem(this.TOKEN_KEY, accessToken);
        sessionStorage.setItem(this.USER_KEY, JSON.stringify(user));
      }

      //conectar websocket
      this.websocketService.disconnect();
      setTimeout(() => {
        this.websocketService.connect(accessToken);
      }, 500);
    }

    return result;
  }


  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  async logout(): Promise<void> {
    const token = this.getToken();
    if (!token) return;

    await this.api.post('Auth/logout', {}); 

    sessionStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.TOKEN_KEY);
    sessionStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.USER_KEY);

    this.websocketService.disconnect(); 
  }

  getUser(): User | null {
    const user = localStorage.getItem(this.USER_KEY) || sessionStorage.getItem(this.USER_KEY);
    return user ? JSON.parse(user) : null;
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY) || sessionStorage.getItem(this.TOKEN_KEY);
  }

  isAdmin(): boolean {
    const user = this.getUser();
    return user?.role === "Admin"; 
  }

  
}
