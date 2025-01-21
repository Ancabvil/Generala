import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable, catchError, forkJoin, lastValueFrom, map } from 'rxjs';
import { Result } from '../models/result';
import { environment } from '../../environments/environment';
import { Image } from '../models/image';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private readonly BASE_URL = environment.apiUrl;

  private readonly USER_KEY = 'user';
  private readonly TOKEN_KEY = 'jwtToken';

  jwt: string;

  constructor(private http: HttpClient) { }

  async get<T = void>(path: string, params: any = {}, responseType = null): Promise<Result<T>> {
    const url = `${this.BASE_URL}${path}`;
    const request$ = this.http.get(url, {
      params: new HttpParams({ fromObject: params }),
      headers: this.getHeader(),
      responseType: responseType,
      observe: 'response',
    });

    return this.sendRequest<T>(request$);
  }

  async post<T = void>(path: string, body: Object = {}, contentType = null): Promise<Result<T>> {
    const url = `${this.BASE_URL}${path}`;
    const request$ = this.http.post(url, body, {
      headers: this.getHeader(contentType),
      observe: 'response'
    });

    return this.sendRequest<T>(request$);
  }

  async put<T = void>(path: string, body: Object = {}, contentType = null): Promise<Result<T>> {
    const url = `${this.BASE_URL}${path}`;
    const request$ = this.http.put(url, body, {
      headers: this.getHeader(contentType),
      observe: 'response'
    });

    return this.sendRequest<T>(request$);
  }

  async delete<T = void>(path: string, params: any = {}): Promise<Result<T>> {
    const url = `${this.BASE_URL}${path}`;
    const request$ = this.http.delete(url, {
      params: new HttpParams({ fromObject: params }),
      headers: this.getHeader(),
      observe: 'response'
    });

    return this.sendRequest<T>(request$);
  }

  private async sendRequest<T = void>(request$: Observable<HttpResponse<any>>): Promise<Result<T>> {
    let result: Result<T>;

    try {
      const response = await lastValueFrom(request$);
      const statusCode = response.status;

      if (response.ok) {
        const data = response.body as T;

        if (data == undefined) {
          result = Result.success(statusCode);
        } else {
          result = Result.success(statusCode, data);
        }
      } else {
        result = result = Result.error(statusCode, response.statusText);
      }
    } catch (exception) {
      if (exception instanceof HttpErrorResponse) {
        result = Result.error(exception.status, exception.statusText);
      } else {
        result = Result.error(-1, exception.message);
      }
    }

    return result;
  }

  private getHeader(accept = null, contentType = null): HttpHeaders {
    let header: any = { 'Authorization': `Bearer ${this.jwt}` };
    // Para cuando haya que poner un JWT

     console.log("JWT: ", this.jwt)

    if (accept)
      header['Accept'] = accept;

    if (contentType)
      header['Content-Type'] = contentType;

    return new HttpHeaders(header);
  }




//



  async getUser(id: number): Promise<User> {
    const request: Observable<Object> =
      this.http.get(`${this.BASE_URL}User/${id}`);

    const dataRaw: any = await lastValueFrom(request);

    const user: User = {
      userId: id,
      nickname: dataRaw.nickname,
      email: dataRaw.email,
      is_banned: dataRaw.is_banned,
      role: dataRaw.role,
      avatar: dataRaw.avatar

    };
    return user;
  }

  // devuelve todos los usuarios
  async allUser(): Promise<User[]> {
    const request: Observable<Object> = this.http.get(`${this.BASE_URL}User/allUsers`);
    const dataRaw: any = await lastValueFrom(request);

    const users: User[] = [];

    for (const u of dataRaw) {
      const user: User = {
        userId: u.userId,
        nickname: u.nickname,
        email: u.email,
        is_banned: u.is_banned,
        role: u.role,
        avatar: u.avatar
      }
      users.push(user);
    }
    return users;
  }


  // Elimina usuario
  deleteUser(idUser: number): Observable<any> {
    const url = (`${this.BASE_URL}User/deleteUser/${idUser}`);
    return this.http.delete(url, { responseType: 'text' });
  }

  // actualizar info de usuario
  updateUser(user: any): Observable<any> {
    const headers = this.getHeader(); // para q me lea el token del usuario actual
    return this.http.put(`${this.BASE_URL}User/modifyUser`, user, { headers, responseType: 'text' });
  }


  // Modificar rol del usuario
  modifyRole(userId: number, newRole: string): Observable<any> {
    const headers = this.getHeader('application/json', 'application/json');
    const body = {
        userId: userId,
        newRole: newRole
    }
    return this.http.put(`${this.BASE_URL}User/modifyUserRole`, body, { headers })
  }

}