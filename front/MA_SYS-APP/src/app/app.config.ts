import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';

import { ToastrModule } from 'ngx-toastr';

import { ModalModule } from 'ngx-bootstrap/modal';

import { routes } from './app.routes';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { Authinterceptor } from '../Services/Auth/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [

    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: Authinterceptor,
      multi: true
    },      
    
    provideRouter(routes),
    
    provideAnimations(),

    importProvidersFrom(
      ModalModule.forRoot(),
      ToastrModule.forRoot({
        positionClass: 'toast-bottom-right',
        preventDuplicates: true,
        progressBar: true,
        closeButton: true,
        timeOut: 3000,
      }),
    ),    
  ],
};
