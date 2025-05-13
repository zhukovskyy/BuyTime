import { Component } from '@angular/core';
import { ApiService } from '../api.service';
import { CommonModule } from '@angular/common';  // Додаємо CommonModule

@Component({
  selector: 'app-telegram',
  standalone: true,
  imports: [CommonModule],  // Додаємо CommonModule в imports
  templateUrl: './telegram.component.html',
  styleUrls: ['./telegram.component.css']
})
export class TelegramComponent {
  response: any;

  constructor(private apiService: ApiService) { }

  sendWebhook() {
    const testBody = {
      message: {
        chat: {
          id: 123456789
        },
        text: 'Привіт з Angular!'
      }
    };

    this.apiService.sendTelegramWebhook(testBody).subscribe({
      next: (res) => {
        console.log('Відповідь з сервера:', res);
        this.response = res;
      },
      error: (err) => {
        console.error('Помилка при запиті:', err);
        this.response = 'Помилка';
      }
    });
  }
}
