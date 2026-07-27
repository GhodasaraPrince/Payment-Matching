import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { PrimeTemplate } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { MatchBatchSummary } from '../models';
import { PaymentMatchingService } from '../payment-matching.service';

@Component({
  selector: 'app-match-history',
  imports: [CardModule, CommonModule, MessageModule, PrimeTemplate, TableModule],
  templateUrl: './match-history.html',
  styleUrl: './match-history.scss',
})
export class MatchHistory implements OnInit {
  private readonly paymentMatchingService = inject(PaymentMatchingService);
  private readonly router = inject(Router);

  protected readonly batches = signal<MatchBatchSummary[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.paymentMatchingService.listBatches().subscribe({
      next: (batches) => {
        this.batches.set(batches);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(err.error?.message ?? 'Could not load match history. Please try again.');
        this.loading.set(false);
      },
    });
  }

  openBatch(batch: MatchBatchSummary): void {
    this.router.navigate(['/history', batch.id]);
  }
}
