import { Injectable, Injector } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {
  private socket: WebSocket;
  private reconnectInterval = 5000;
  private isManualDisconnect = false;
  private authService: AuthService;

  connected = new Subject<void>();
  messageReceived = new Subject<any>();
  disconnected = new Subject<void>();
  

  private onlineUsers = new BehaviorSubject<any[]>([]);
  onlineUsers$ = this.onlineUsers.asObservable(); 

  constructor(private injector: Injector) {}

  private getAuthService() {
    if (!this.authService) {
      this.authService = this.injector.get(AuthService);
    }
    return this.authService;
  }

  connect(token: string) {
    if (this.socket && this.socket.readyState === WebSocket.OPEN) {
      
      return;
    }

    const socketUrl = `wss://localhost:7215/ws?token=${token}`;
    //console.log(`Conectando a WebSocket: ${socketUrl}`);

    this.socket = new WebSocket(socketUrl);

    this.socket.onopen = () => {
      console.log("WebSocket conectado.");
      this.connected.next();

      this.sendMessage("get_online_users", "");
    };

    this.socket.onmessage = (event) => {
      const message = JSON.parse(event.data);
    
      if (message.Type === "online_users") {
        const onlineUsers = JSON.parse(message.Content);
        console.log("Usuarios en línea actualizados:", onlineUsers);
        this.onlineUsers.next(onlineUsers);
      }
    
      this.messageReceived.next(message);
    };
    

    this.socket.onclose = (event) => {
      console.warn("WebSocket desconectado:", event.reason);
      this.disconnected.next();

      if (!this.isManualDisconnect) {
        this.reconnect();
      }
    };

    this.socket.onerror = (error) => {
      console.error("Error en WebSocket:", error);
      this.disconnected.next();
      this.reconnect();
    };
  }

  sendMessage(type: string, content: string) {
    if (this.isConnected()) {
      const message = JSON.stringify({ Type: type, Content: content });
      this.socket.send(message);
    } else {
      //console.warn("No se puede enviar, WebSocket no está conectado.");
    }
  }

  disconnect() {
    this.isManualDisconnect = true;
    if (this.socket) {
      this.socket.close(1000, 'Closed by client');
      this.socket = null;
    }
  }

  isConnected(): boolean {
    return this.socket && this.socket.readyState === WebSocket.OPEN;
  }

  private reconnect() {
    if (!this.isManualDisconnect) {

      setTimeout(() => {
        const token = localStorage.getItem('jwtToken') || sessionStorage.getItem('jwtToken');
        if (token) {
          this.connect(token);
        } else {
          console.warn("No hay token válido para reconectar WebSocket.");
        }
      }, this.reconnectInterval);
    }
  }
}
