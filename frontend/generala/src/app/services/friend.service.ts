import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { Friendship } from '../models/friendship';
import { FriendRequest } from '../models/friendrequest';
import { catchError, map, tap } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class FriendService {
  private apiUrl = `${environment.apiUrl}friend-requests`;  
  private friendshipUrl = `${environment.apiUrl}friendships`;

  constructor(private http: HttpClient, private injector: Injector) {}

  //Obtener lista de amigos
  getFriends(userId: number): Observable<Friendship[]> {
    return this.http.get<any[]>(`${this.friendshipUrl}/${userId}`).pipe(
      map(friends => friends.map(friend => ({
        id: friend.id,
        friendId: friend.id,
        friendNickname: friend.nickname,
        friendAvatar: friend.image 
          ? `${environment.apiImg}${friend.image}` 
          : 'default-avatar.png',
        friendStatus: "desconectado"
      }))),
      catchError(() => of([]))
    );
  }

  //obtener usuarios
  getAllUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}User/allUsers`).pipe(
      tap(users => console.log("Usuarios obtenidos desde la API:", users)), 
      map(users => users.map(user => ({
        userId: user.userId,  
        nickname: user.nickname,
        avatarUrl: user.image 
          ? `${environment.apiImg}${user.image}` 
          : 'default-avatar.png'
      }))),
      catchError(() => {
        console.error("Error obteniendo los usuarios.");
        return of([]);
      })
    );
  }
  

  //Obtener amigos
  getFriendships(): Observable<Friendship[]> {
    return this.http.get<Friendship[]>(this.friendshipUrl);
  }

  //recoger solicitudes enviadas a nosotros
  getFriendRequests(userId: number): Observable<FriendRequest[]> {
    return this.http.get<FriendRequest[]>(`${this.apiUrl}/pending/${userId}`);
  }

  //recoger solicitudes enviadas por nosotros
  getSentFriendRequests(userId: number): Observable<FriendRequest[]> {
    return this.http.get<FriendRequest[]>(`${this.apiUrl}/sent/${userId}`);
  }
  
  //enviar
  sendFriendRequest(senderId: number, receiverId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/send?senderId=${senderId}&receiverId=${receiverId}`, {});
  }

  //acepatar
  acceptFriendRequest(requestId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/accept?requestId=${requestId}`, {});
  }

  //rechazar
  rejectFriendRequest(requestId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/reject/${requestId}`);
  }

  //elimniar amistad
  removeFriendship(userId: number, friendId: number): Observable<any> {
    return this.http.delete(`${this.friendshipUrl}/remove?userId=${userId}&friendId=${friendId}`, { responseType: 'text' });
  }
  
  
  
}
