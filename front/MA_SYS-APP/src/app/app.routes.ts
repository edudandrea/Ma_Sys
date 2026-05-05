import { RouterModule, Routes } from '@angular/router';
import { AlunosComponent } from '../Alunos/Alunos.component';
import { NgModule } from '@angular/core';
import { ModalidadesComponent } from '../Modalidades/Modalidades.component';
import { ProfessoresComponent } from '../Professores/Professores.component';
import { AcademiasComponent } from '../Academias/Academias.component';
import { LoginComponent } from '../Login/Login.component';
import { AuthGuard } from '../Services/Auth/auth.guard';
import { RoleGuard } from '../Services/Auth/role.guard';
import { LayoutComponent } from '../Layout/Layout.component';
import { UsuariosComponent } from '../Usuarios/Usuarios.component';
import { PlanosComponent } from '../Planos/Planos.component';
import { DashboardComponent } from '../Dashboard/Dashboard.component';
import { DashboardFederacaoComponent } from '../DashboardFederacao/DashboardFederacao.component';
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
import { PagamentoFiliadosComponent } from '../PagamentoFiliados/PagamentoFiliados.component';


export const routes: Routes = [

    
    { path: 'login', component: LoginComponent}, 
    { path: ':academia/cadastro', component: CadastroAlunosComponent},    
    { path: 'federacao/:federacaoId/pagamento', component: PagamentoFiliadosComponent },
    
    {
        path: '',
        component: LayoutComponent,
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            
            { path: 'dashboard', component: DashboardComponent, canActivate: [RoleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Academia'] } },  
            { path: 'dashboard-federacao', component: DashboardFederacaoComponent, canActivate: [RoleGuard], data: { roles: ['Federacao'] } },
            { path: 'academias', component: AcademiasComponent, canActivate: [RoleGuard], data: { roles: ['Admin', 'SuperAdmin'] } },  
            { path: 'federacoes', component: FederacoesComponent, canActivate: [RoleGuard], data: { roles: ['Admin', 'SuperAdmin'] } },
            { path: 'filiados', component: FiliadosComponent, canActivate: [RoleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Federacao'] } },          
            { path: 'alunos', component: AlunosComponent, canActivate: [RoleGuard], data: { roles: ['Academia', 'SuperAdmin'] } },
            { path: 'modalidades', component: ModalidadesComponent, canActivate: [RoleGuard], data: { roles: ['Academia', 'SuperAdmin'] } },
            { path: 'professores', component: ProfessoresComponent, canActivate: [RoleGuard], data: { roles: ['Academia', 'SuperAdmin'] } },
            { path: 'planos', component: PlanosComponent, canActivate: [RoleGuard], data: { roles: ['Academia', 'SuperAdmin', 'Federacao'] } },
            { path: 'pagamentos', component: PagamentosComponent, canActivate: [RoleGuard], data: { roles: ['Academia', 'SuperAdmin', 'Federacao'] } },
            { path: 'mensalidades-sistema', component: MensalidadesSistemaComponent, canActivate: [RoleGuard], data: { roles: ['Admin', 'SuperAdmin'] } },
            { path: 'turmas', component: TurmasComponent, canActivate: [RoleGuard], data: { roles: ['Academia', 'SuperAdmin'] } },
            { path: 'treinos', component: TreinosComponent, canActivate: [RoleGuard], data: { roles: ['Academia', 'SuperAdmin'] } },
            { path: 'fluxo-caixa', component: FluxoCaixaComponent, canActivate: [RoleGuard], data: { roles: ['Academia', 'SuperAdmin', 'Federacao'] } },
            { path: 'relatorios', component: RelatoriosComponent, canActivate: [RoleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Academia'] } },
            { path: 'usuarios', component: UsuariosComponent, canActivate: [RoleGuard], data: { roles: ['Admin', 'SuperAdmin'] } },
            { path: 'matriculas', component: MatriculasComponent, canActivate: [RoleGuard], data: { roles: ['Academia', 'SuperAdmin'] } },
        ]
    },    

    
    { path: '**', redirectTo: 'login'}
];


