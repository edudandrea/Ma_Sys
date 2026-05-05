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
import { DashboardComponent } from '../Dashboard/Dashboard.component';
import { PagamentosComponent } from '../Pagamentos/Pagamentos.component';
import { MatriculasComponent } from '../Matriculas/Matriculas.component';
import { CadastroAlunosComponent } from '../CadastroAlunos/CadastroAlunos.component';
import { FluxoCaixaComponent } from '../FluxoCaixa/FluxoCaixa.component';
import { TurmasComponent } from '../Turmas/Turmas.component';
import { TreinosComponent } from '../Treinos/Treinos.component';
import { MensalidadesSistemaComponent } from '../MensalidadesSistema/MensalidadesSistema.component';
import { FiliadosComponent } from '../Filiados/Filiados.component';
import { RelatoriosComponent } from '../Relatorios/Relatorios.component';
import { FederacoesComponent } from '../Federacoes/Federacoes.component';


export const routes: Routes = [

    
    { path: 'login', component: LoginComponent}, 
    { path: ':academia/cadastro', component: CadastroAlunosComponent},    
    
    {
        path: '',
        component: LayoutComponent,
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            
            { path: 'dashboard', component: DashboardComponent },  
            { path: 'academias', component: AcademiasComponent },  
            { path: 'federacoes', component: FederacoesComponent },
            { path: 'filiados', component: FiliadosComponent },          
            { path: 'alunos', component: AlunosComponent },
            { path: 'modalidades', component: ModalidadesComponent },
            { path: 'professores', component: ProfessoresComponent },
            { path: 'planos', component: PlanosComponent },
            { path: 'pagamentos', component: PagamentosComponent },
            { path: 'mensalidades-sistema', component: MensalidadesSistemaComponent },
            { path: 'turmas', component: TurmasComponent },
            { path: 'treinos', component: TreinosComponent },
            { path: 'fluxo-caixa', component: FluxoCaixaComponent },
            { path: 'relatorios', component: RelatoriosComponent },
            { path: 'usuarios', component: UsuariosComponent },
            { path: 'matriculas', component: MatriculasComponent },
        ]
    },    

    
    { path: '**', redirectTo: 'login'}
];


