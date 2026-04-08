import { RouterModule, Routes } from '@angular/router';
import { AlunosComponent } from '../Alunos/Alunos.component';
import { NgModule } from '@angular/core';
import { ModalidadesComponent } from '../Modalidades/Modalidades.component';
import { ProfessoresComponent } from '../Professores/Professores.component';
import { AcademiasComponent } from '../Academias/Academias.component';
import { LoginComponent } from '../Login/Login.component';
import { AuthGuard } from '../Services/Auth/auth.guard';
import { LayoutComponent } from '../Layout/Layout.component';
import { UsuariosComponent } from '../Usuarios/Usuarios.component';
import { PlanosComponent } from '../Planos/Planos.component';
import { DashboardAdminComponent } from '../DashboardAdmin/DashboardAdmin.component';

export const routes: Routes = [

    
    { path: 'login', component: LoginComponent}, 
    
    
    {
        path: '',
        component: LayoutComponent,
        canActivate: [AuthGuard],
        children: [
            { path: 'dashboard', component: DashboardAdminComponent },            
            { path: 'academias', component: AcademiasComponent },            
            { path: 'alunos', component: AlunosComponent },
            { path: 'modalidades', component: ModalidadesComponent },
            { path: 'professores', component: ProfessoresComponent },
            { path: 'planos', component: PlanosComponent },
            { path: 'usuarios', component: UsuariosComponent },
        ]
    },    

    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: '**', redirectTo: 'login'}
];


