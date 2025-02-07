import { CommonModule } from '@angular/common';
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
  nickname: string | null = null;
  userId: number | null = null;
  private subscriptions: Subscription = new Subscription();

  constructor(
    public authService: AuthService,
    private router: Router
  ) { }

  items: MenuItem[] = [];

  ngOnInit() {
    this.updateUserData();
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }


  private updateUserData() {
    const user = this.authService.getUser();
    this.nickname = user ? user.nickname : null;
    this.userId = user ? user.userId : null;
  }

  navigateToLogin() {
    this.router.navigate(['/login']);
  }

 
  async logout() {
    const confirmLogout = await Swal.fire({
      title: "¿Estás seguro?",
      text: "¿Quieres cerrar sesión?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonText: "Sí, cerrar sesión",
      cancelButtonText: "Cancelar"
    });

    if (confirmLogout.isConfirmed) {
      try {
        await this.authService.logout(); 
        this.updateUserData(); 

        Swal.fire({
          title: "Sesión cerrada",
          text: `¡Hasta pronto, ${this.nickname || "usuario"}!`,
          icon: 'success',
          timer: 2000,
          showConfirmButton: false
        });

        this.router.navigate(['/login']);

      } catch (error) {
        console.error("Error al cerrar sesión:", error);
        Swal.fire({
          title: "Error",
          text: "Hubo un problema al cerrar sesión.",
          icon: "error",
          confirmButtonText: "Vale"
        });
      }
    }
  }
}
