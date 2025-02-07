import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { FriendService } from '../../services/friend.service';
import { WebsocketService } from '../../services/websocket.service';
import { environment } from '../../../environments/environment';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { FooterComponent } from "../../components/footer/footer.component";
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Friendship } from '../../models/friendship';
import { FriendRequest } from '../../models/friendrequest';
import { User } from '../../models/user';
import Swal from 'sweetalert2';
import { GameService } from '../../services/game.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [NavbarComponent, FooterComponent, CommonModule, FormsModule], 
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent implements OnInit {
  user: any;
  avatarUrl: string = 'default-avatar.png';
  friends: Friendship[] = [];
  filteredFriends: Friendship[] = [];
  searchQuery: string = '';
  filteredUsers: User[] = [];
  searchQueryUsers: string = '';

  allUsers: any[] = []; 
  friendRequests: FriendRequest[] = []; 

  friendRequestsSent: FriendRequest[] = []; 
  gameInvites: any[] = [];

  totalPlayersOnline: number = 0;
  activeGames: number = 0;
  playersInGames: number = 0;

  private subscriptions: Subscription = new Subscription();

  constructor(
    private authService: AuthService,
    private friendService: FriendService,
    private websocketService: WebsocketService,
    private gameService: GameService
  ) {}

  ngOnInit(): void {
    this.user = this.authService.getUser();
    this.avatarUrl = this.user?.image ? `${environment.apiImg}${this.user.image}` : 'default-avatar.png';
  
    if (this.user?.userId) {
      this.friendService.getFriends(this.user.userId).subscribe(friends => {
        this.friends = friends.filter(friend => friend.friendId !== this.user.userId);
        this.updateOnlineFriends(); 
        this.filteredFriends = [...this.friends];
      });
  
      this.friendService.getFriendRequests(this.user.userId).subscribe(requests => {
        this.friendRequests = requests;
      });
  
      this.friendService.getSentFriendRequests(this.user.userId).subscribe(sentRequests => {
        this.friendRequestsSent = sentRequests;
      });
  
      this.getAllUsers();
      this.getGameStatistics();
      this.getGameInvites();

      this.subscriptions.add(
        this.websocketService.onlineUsers$.subscribe(onlineUsers => {
          this.updateOnlineUsersCount(onlineUsers);
          this.updateOnlineFriends();
        })
      );
    }
  }
  
  updateOnlineUsersCount(onlineUsers: any[]): void {
    this.totalPlayersOnline = onlineUsers.length;
  }
  
  updateOnlineFriends(): void {
    this.websocketService.onlineUsers$.subscribe(onlineUsers => {
      this.friends.forEach(friend => {
        friend.friendStatus = onlineUsers.some(user => user.userId === friend.friendId) ? 'conectado' : 'desconectado';
      });
    });
  }
  

  getGameStatistics(): void {
    this.gameService.getGameStats().subscribe(stats => {
      this.totalPlayersOnline = stats.totalPlayersOnline;
      this.activeGames = stats.activeGames;
      this.playersInGames = stats.playersInGames;
    });
  }


  getAllUsers(): void {
    this.friendService.getAllUsers().subscribe(users => {
      this.allUsers = users.filter(user => user.userId !== this.user.userId);
      this.filteredUsers = [...this.allUsers]; 
    });
  }


  // Filtro de búsqueda de amigos
  searchFriends(): void {
    const normalizeText = (text: string): string => 
      text.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
    
    const search = normalizeText(this.searchQuery);

    this.filteredFriends = this.friends.filter(friend =>
      normalizeText(friend.friendNickname).includes(search)
    );
  }

  // Filtro de búsqueda
  searchUsers(): void {
    const normalizeText = (text: string): string => 
      text ? text.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase() : '';
  
    const search = normalizeText(this.searchQueryUsers);
  
    this.filteredUsers = this.allUsers.filter(user => {
      const nickname = normalizeText(user.nickname);
      return nickname.includes(search);
    });
  }
  

  // Enviar solicitud de amistad
  sendFriendRequest(receiverId: number): void {
    if (!this.user || !this.user.userId) return; 
  
    this.friendService.sendFriendRequest(this.user.userId, receiverId).subscribe(() => {
  
      this.friendService.getSentFriendRequests(this.user.userId).subscribe(sentRequests => {
        this.friendRequestsSent = sentRequests;
      });
  
      Swal.fire({ 
        title: "Petición de amistad enviada.",
        icon: 'success',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
      });
    });
  }

  //aceptar
  acceptFriendRequest(requestId: number): void {
    this.friendService.acceptFriendRequest(requestId).subscribe(() => {

      this.friendRequests = this.friendRequests.filter(req => req.id !== requestId);
      this.friendService.getFriends(this.user.userId).subscribe(friends => {

        this.friends = friends.filter(friend => friend.friendId !== this.user.userId);
        this.filteredFriends = [...this.friends]; 
      });
    });
  }

  //rechazar
  rejectFriendRequest(requestId: number): void {
    this.friendService.rejectFriendRequest(requestId).subscribe(() => {
      this.friendRequests = this.friendRequests.filter(req => req.id !== requestId);
    });
  }

  //para comprobar si son amigos o hay una solicitud
  getFriendshipStatus(userId: number): string {
    if (!this.user || !this.user.userId) return "";
  
    //amigos
    if (this.friends.some(friend => friend.friendId === userId)) {
      return "Son amigos";
    }
  
    //ha enviado
    const sentRequest = this.friendRequestsSent.some(request => 
      request.receiverId === userId && !request.isAccepted
    );
    if (sentRequest) {
      return "Solicitud enviada";
    }
  
    //ha recibido
    const receivedRequest = this.friendRequests.some(request => 
      request.receiverId === this.user.userId && request.senderId === userId && !request.isAccepted
    );
    if (receivedRequest) {
      return "Hay una solicitud pendiente";
    }
    return "Agregar";
  }
  
  
  confirmRemoveFriend(friendId: number): void {
    Swal.fire({
      title: "¿Eliminar amigo?",
      text: "¿Estás seguro de que quieres eliminar a este amigo?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#dc3545",
      cancelButtonColor: "#6c757d",
      confirmButtonText: "Sí, eliminar"
    }).then((result) => {
      if (result.isConfirmed) {
        this.removeFriend(friendId);
      }
    });
  }
  
  //eliminar amigo
  removeFriend(friendId: number): void {
    if (!this.user || !this.user.userId) return;
  
    this.friendService.removeFriendship(this.user.userId, friendId).subscribe(() => {
      this.friends = this.friends.filter(friend => friend.friendId !== friendId);
      this.filteredFriends = [...this.friends];
  
      Swal.fire("Eliminado", "El amigo ha sido eliminado.", "success");
    });
  }


  getGameInvites(): void {
    
  }

  acceptGameInvite(inviteId: number): void {
    
  }

  rejectGameInvite(inviteId: number): void {
    
  }
  

  //boton de matchmaking
  goToMatchmaking(): void {
  
  }
  
  ngOnDestroy(): void {
    this.subscriptions.unsubscribe(); 
  }

}
