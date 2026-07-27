import { Component } from '@angular/core';
import { PaymentsMatching } from './payments-matching/payments-matching';

@Component({
  selector: 'app-root',
  imports: [PaymentsMatching],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
