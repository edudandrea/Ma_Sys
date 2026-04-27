import { Injectable } from "@angular/core";
import { CanActivate, Router } from "@angular/router"; 
import { AuthService } from "./Auth.service";

@Injectable({
    providedIn: 'root'
})

export class AuthGuard implements CanActivate{

    constructor(private auth: AuthService, 
                private router: Router){}

    canActivate(): boolean{
        if (typeof window === 'undefined')
            return true;

        if(!this.auth.isLogged()){
            this.router.navigate(['/login']);
            return false;
        }
        return true;
    }        
    
}


