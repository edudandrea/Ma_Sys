import { Injectable } from "@angular/core";
import { CanActivate, Router } from "@angular/router"; 
import { AuthService } from "./Auth.service";

@Injectable({
    providedIn: 'root'
})

export class AdminGuard implements CanActivate{

    constructor(private auth: AuthService, private router: Router){}

    canActivate(): boolean{
        const token = localStorage.getItem('token');

        if(!token) return false;

        const payload = JSON.parse(atob(token.split('.')[1]));

        if (payload.role !== 'Admin')
        {
            this.router.navigate(['/Alunos']);
            return false;
        }

        return true;
    }        
    
}