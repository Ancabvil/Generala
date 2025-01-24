import { CommonModule, NgClass } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';
import { ImageModule } from 'primeng/image';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import Swal from 'sweetalert2';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [MenubarModule, ImageModule, RouterModule, CommonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent implements OnInit, OnDestroy {
  nickname: string | null = null; // nombre del usuario
  userId: number | null = null; // id del usuario
  private subscriptions: Subscription = new Subscription();

  constructor(
    public authService: AuthService,
    public router: Router,
  ) { }

  items: MenuItem[] = [];

  ngOnInit() {
    // usuario logueado
    const user = this.authService.getUser();
    this.nickname = user ? user.nickname : null;
    this.userId = user ? user.userId : null;
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  authClick() {
    // Cerrar sesión
    if (this.authService.isAuthenticated()) {
      Swal.fire({ // Cuadro de diálogo
        title: "Has cerrado sesión con éxito",
        text: `¡Hasta pronto ${this.nickname}!`,
        icon: 'success',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didClose: () => {
          this.authService.logout(),
            this.router.navigate(['/login'])
            
        }
      });

      // Iniciar sesión
    } else {
      this.router.navigate(['/login']);
    }
  }

}
