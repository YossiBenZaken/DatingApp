import { AuthService } from '../_services/auth.service';
import { catchError } from 'rxjs/operators';
import { AlertifyService } from '../_services/alertify.service';
import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { Observable, of } from 'rxjs';

@Injectable()
export class MemberEditResolver implements Resolve<User> {
    constructor(private userService: UserService, private router: Router, private alertify: AlertifyService, private auth: AuthService) {}
    resolve(route: ActivatedRouteSnapshot): Observable<User> {
        // tslint:disable-next-line: no-string-literal
        return this.userService.getUser(this.auth.decodedToken.nameid).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving your data');
                this.router.navigate(['/members']);
                return of(null);
            })
        );
    }
}
