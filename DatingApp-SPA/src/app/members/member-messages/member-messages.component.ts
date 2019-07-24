import { AlertifyService } from './../../_services/alertify.service';
import { AuthService } from './../../_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { Message } from './../../_models/message';
import { Component, OnInit, Input } from '@angular/core';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() recipientID: number;
  messages: Message[];
  newMessage: any = {};

  constructor(private userService: UserService, private auth: AuthService, private alertify: AlertifyService) { }

  ngOnInit() {
    this.loadMessages();
  }

  loadMessages() {
    const currentUserID = +this.auth.decodedToken.nameid;
    this.userService.getMessageThread(this.auth.decodedToken.nameid, this.recipientID)
      .pipe(
        tap(m => {
          // tslint:disable-next-line: prefer-for-of
          for (let i = 0; i < m.length; i++) {
            if(m[i].isRead === false && m[i].recipientID === currentUserID) {
              this.userService.markAsRead(currentUserID, m[i].id);
            }
          }
        })
      )
      .subscribe(m => {
        this.messages = m;
      }, error => {
        this.alertify.error(error);
      });
  }
  sendMessage() {
    this.newMessage.recipientID = this.recipientID;
    this.userService.sendMessage(this.auth.decodedToken.nameid, this.newMessage).subscribe((m: Message) => {
      this.messages.unshift(m);
      this.newMessage.content = '';
    }, error => {
      this.alertify.error(error);
    });
  }

}
