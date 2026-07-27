import { Routes } from '@angular/router';
import { MatchDetail } from './payments-matching/match-detail/match-detail';
import { MatchHistory } from './payments-matching/match-history/match-history';
import { PaymentsMatching } from './payments-matching/payments-matching';

export const routes: Routes = [
  { path: '', component: PaymentsMatching },
  { path: 'history', component: MatchHistory },
  { path: 'history/:batchId', component: MatchDetail },
  { path: '**', redirectTo: '' },
];
