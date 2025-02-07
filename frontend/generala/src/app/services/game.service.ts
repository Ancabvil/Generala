import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private apiUrl = `${environment.apiUrl}game`; 

  constructor(private http: HttpClient) {}

  
  getGameStats(): Observable<any> {
    return this.http.get<any>(this.apiUrl);
  }
}
