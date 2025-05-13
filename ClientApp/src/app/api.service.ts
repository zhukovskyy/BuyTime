import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private apiUrl = 'https://localhost:5002/api/telegram'; // ← заміни, якщо інша адреса

  constructor(private http: HttpClient) { }

  sendTelegramWebhook(body: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/webhook`, body);
  }

  // Приклад методу GET, якщо буде потрібно
  getSomething(): Observable<any> {
    return this.http.get(`${this.apiUrl}/test`);
  }
}
