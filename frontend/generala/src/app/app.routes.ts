import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { MenuComponent } from './pages/menu/menu.component';

export const routes: Routes = [
    { path: '', component: HomeComponent }, // Ruta pantalla inicio home
    { path: 'login', component: LoginComponent }, // Ruta login
    { path: 'register', component: RegisterComponent }, // Ruta registro
    { path: 'menu', component: MenuComponent} //Ruta Menu
];
