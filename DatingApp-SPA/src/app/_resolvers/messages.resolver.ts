import { AuthService } from './../_services/auth.service';
import { Message } from './../_models/message';
import { catchError } from 'rxjs/operators';
import { AlertifyService } from '../_services/alertify.service';
import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/user.service';
import { Observable, of } from 'rxjs';

@Injectable()
export class MessagesResolver implements Resolve<Message[]> {
    pageNumber = 1;
    pageSize = 10;
    messageContainer = 'Unread';
    constructor(private auth: AuthService, private userService: UserService, private router: Router, private alertify: AlertifyService) {}
    resolve(route: ActivatedRouteSnapshot): Observable<Message[]> {
        // tslint:disable-next-line: no-string-literal
        return this.userService.getMessages(this.auth.decodedToken.nameid, this.pageNumber, this.pageSize, this.messageContainer).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving messages');
                this.router.navigate(['/']);
                return of(null);
            })
        );
    }
}
