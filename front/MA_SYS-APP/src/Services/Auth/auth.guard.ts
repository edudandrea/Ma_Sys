import { Injectable } from "@angular/core";
import { CanActivate, CanActivateChild, Router } from "@angular/router";
import { AuthService } from "./Auth.service";

@Injectable({
    providedIn: 'root'
})

export class AuthGuard implements CanActivate, CanActivateChild{

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

    canActivateChild(): boolean {
        return this.canActivate();
    }
    
}


