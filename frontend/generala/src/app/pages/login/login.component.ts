import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { CheckboxModule } from 'primeng/checkbox';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterModule, CheckboxModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  readonly PARAM_KEY: string = 'redirectTo';
  private redirectTo: string = null;

  email: string = '';
  nickname: string = '';
  password: string = '';
  rememberMe: boolean = false;
  jwt: string = '';


  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private authService: AuthService,
  
  ) { }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
    }

    // Obtiene la URL a la que el usuario quería acceder
    const queryParams = this.activatedRoute.snapshot.queryParamMap;

    if (queryParams.has(this.PARAM_KEY)) {
      this.redirectTo = queryParams.get(this.PARAM_KEY);
    }
  }

  async submit() {
    const authData = { email: this.email, nickname: this.nickname, password: this.password };
    const result = await this.authService.login(authData, this.rememberMe);

    if (result.success) {
      this.jwt = result.data.accessToken;
      console.log('Inicio de sesión exitoso', result);

      if (this.rememberMe) {
        localStorage.setItem('jwtToken', this.jwt);
      }


      const user = this.authService.getUser();
      const userId = user ? user.userId : null;



      const nickname = user ? user.nickname : null;
      
      Swal.fire({ // Cuadro de diálogo
        title: "Inicio de sesión con éxito",
        text: `¡Hola, ${nickname}!`,
        icon: 'success',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didClose: () => this.redirect()
      });

    } else {
      Swal.fire({ // Cuadro de diálogo
        title: "Usuario o contraseña incorrectos",
        icon: "error",
        confirmButtonText: "Vale"
      });
    }
  }

  // redirigir al usuario
  redirect() {
    if (this.redirectTo != null) {
      this.router.navigateByUrl(this.redirectTo);
    } else {
      this.router.navigate(['/']);
    }
  }

  // redirigir al usuario desde el registro
  redirectToSignup() {
    this.router.navigate(['/signup'], { queryParams: { redirectTo: this.redirectTo } });
  }
}