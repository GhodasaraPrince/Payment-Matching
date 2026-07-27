import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { PrimeTemplate } from 'primeng/api';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { MatchBatchSummary } from '../models';
import { PaymentMatchingService } from '../payment-matching.service';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-match-history',
  imports: [CardModule, CommonModule, MessageModule, PrimeTemplate, TableModule],
  templateUrl: './match-history.html',
  styleUrl: './match-history.scss',
})
export class MatchHistory implements OnInit {
  private readonly paymentMatchingService = inject(PaymentMatchingService);
  private readonly router = inject(Router);

  protected readonly pageSize = PAGE_SIZE;
  protected readonly batches = signal<MatchBatchSummary[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadPage(0);
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    this.loadPage(event.first ?? 0);
  }

  openBatch(batch: MatchBatchSummary): void {
    this.router.navigate(['/history', batch.id]);
  }

  private loadPage(first: number): void {
    this.loading.set(true);
    const page = Math.floor(first / PAGE_SIZE) + 1;

    this.paymentMatchingService.listBatches(page, PAGE_SIZE).subscribe({
      next: (result) => {
        this.batches.set(result.items);
        this.totalRecords.set(result.totalCount);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(err.error?.message ?? 'Could not load match history. Please try again.');
        this.loading.set(false);
      },
    });
  }
}
