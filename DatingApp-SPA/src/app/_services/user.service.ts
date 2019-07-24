import { Message } from './../_models/message';
import { PaginatedResult } from './../_models/pagination';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) {}
  getUsers(page?, itemsPerPage?, userParams?, likesParam?): Observable<PaginatedResult<User[]>> {
    const paginationResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();
    let params = new HttpParams();
    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }
    if (userParams != null) {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }
    if (likesParam === 'Likers') {
      params = params.append('likers', 'true');
    }
    if (likesParam === 'Likees') {
      params = params.append('likees', 'true');
    }
    return this.http.get<User[]>(this.baseUrl + 'users', { observe: 'response', params })
    .pipe(
      map(response => {
        paginationResult.result = response.body;
        if (response.headers.get('Pagination') != null) {
          paginationResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginationResult;
      })
    );
  }
  getUser(id): Observable<User> {
    return this.http.get<User>(this.baseUrl + 'users/' + id);
  }
  updateUser(id: number, user: User) {
    return this.http.put(this.baseUrl + 'users/' + id, user);
  }
  setMainPhoto(userID: number, id: number) {
    return this.http.post(this.baseUrl + 'users/' + userID + '/photos/' + id + '/setMain', {});
  }
  deletePhoto(userID: number, id: number) {
    return this.http.delete(this.baseUrl + 'users/' + userID + '/photos/' + id);
  }
  sendLike(userID: number, recipientID: number) {
    return this.http.post(this.baseUrl + 'users/' + userID + '/like/' + recipientID, {});
  }
  getMessages(id: number, page?, itemsPerPage?, messageContainer?) {
    const paginatedResult: PaginatedResult<Message[]> = new PaginatedResult<Message[]>();
    let params = new HttpParams();
    params = params.append('MessageContainer', messageContainer);
    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }
    return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages', {observe: 'response', params})
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if(response.headers.get('Pagination') !== null) {
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }
          return paginatedResult;
        })
      );
  }
  getMessageThread(id: number, recipientID: number) {
    return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages/thread/' + recipientID);
  }
  sendMessage(id: number, message: Message) {
    return this.http.post(this.baseUrl + 'users/' + id + '/messages', message);
  }
  deleteMessage(id: number, userID: number){
    return this.http.post(this.baseUrl + 'users/' + userID + '/messages/' + id, {});
  }
  markAsRead(userID: number, messageID: number){
    this.http.post(this.baseUrl + 'users/' + userID + '/messages/' + messageID + '/read', {}).subscribe();
  }
}
