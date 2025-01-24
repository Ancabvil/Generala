import { NgIf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import Swal from 'sweetalert2';


@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, FormsModule, NgIf], 
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {

  myForm: FormGroup;
  readonly PARAM_KEY: string = 'redirectTo';
  private redirectTo: string = null;

  constructor(private formBuilder: FormBuilder,
    private http: HttpClient,
    private authService: AuthService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
  ) {
    this.myForm = this.formBuilder.group({
      nickname: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      image: [''] //avatar
    },
      { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/menu']);
    }

    // Obtiene la URL a la que el usuario quería acceder
    const queryParams = this.activatedRoute.snapshot.queryParamMap;

    if (queryParams.has(this.PARAM_KEY)) {
      this.redirectTo = queryParams.get(this.PARAM_KEY);
    }
  }

  // Validator personalizado
  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPasswordControl = form.get('confirmPassword');
    const confirmPassword = confirmPasswordControl?.value;

    if (password !== confirmPassword && confirmPasswordControl) {
      confirmPasswordControl.setErrors({ mismatch: true });
    } else if (confirmPasswordControl) {
      confirmPasswordControl.setErrors(null);
    }
  }

  async submit() {
    if (this.myForm.valid) {
      const formData = new FormData(); // Usamos FormData para incluir el archivo
      const formValues = this.myForm.value;
  
      // Añadimos los valores del formulario al FormData
      formData.append('nickname', formValues.nickname);
      formData.append('email', formValues.email);
      formData.append('password', formValues.password);
  
      // Solo añadimos la imagen si se seleccionó
      if (formValues.image) {
        formData.append('image', formValues.image);
      }
  
      try {
        // Llamamos al servicio para registrarse
        const signupResult = await this.authService.signup(formData);
  
        if (signupResult.success) {
          console.log('Registro exitoso', signupResult);

          const identifier = formValues.nickname || formValues.email;
          const authData = { identifier, password: formValues.password };
          //const authData = { email: formValues.email, nickname: formValues.nickname, password: formValues.password };
          const loginResult = await this.authService.login(authData, false);
  
          if (loginResult.success) {
            console.log('Inicio de sesión exitoso', loginResult);
  
            const user = this.authService.getUser();
            const nickname = user ? user.nickname : null;
  
            Swal.fire({
              title: "Te has registrado con éxito",
              text: `¡Hola, ${nickname}!`,
              icon: 'success',
              showConfirmButton: false,
              timer: 3000,
              timerProgressBar: true,
              didClose: () => this.redirect(),
            });
          } else {
            this.throwError("Error en el inicio de sesión");
          }
        } else {
          this.throwError("Error en el registro");
        }
      } catch (error) {
        this.throwError("Error en la comunicación con el servidor");
        console.error(error);
      }
    } else {
      this.throwError("Formulario no válido");
    }
  }

  // redirigir al usuario
  redirect() {
    if (this.redirectTo != null) {
      this.router.navigateByUrl(this.redirectTo);
    } else {
      this.router.navigate(['/menu']);
    }
  }

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.myForm.patchValue({ image: file });
    }
  }


  throwError(error: string) {
    Swal.fire({ // Cuadro de diálogo
      title: "Se ha producido un error",
      text: error,
      icon: "error",
      confirmButtonText: "Vale"
    });
  }


}
