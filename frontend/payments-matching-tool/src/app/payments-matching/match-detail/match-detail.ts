import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { MatchResultsPanel } from '../match-results-panel/match-results-panel';
import { MatchBatchDetail, PaymentMatchRow, ResolutionSide, ResultFilter } from '../models';
import { PaymentMatchingService } from '../payment-matching.service';

@Component({
  selector: 'app-match-detail',
  imports: [ButtonModule, CardModule, CommonModule, MatchResultsPanel, MessageModule, RouterLink],
  templateUrl: './match-detail.html',
  styleUrl: './match-detail.scss',
})
export class MatchDetail {
  private readonly paymentMatchingService = inject(PaymentMatchingService);

  readonly batchId = input.required<string>();

  protected readonly detail = signal<MatchBatchDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly filter = signal<ResultFilter>('all');
  protected readonly resolvingIds = signal<ReadonlySet<string>>(new Set());

  protected readonly summary = computed(() => this.detail()?.summary ?? null);
  protected readonly items = computed(() => this.detail()?.items ?? []);
  protected readonly truncated = computed(() => {
    const d = this.detail();
    return !!d && d.totalItems > d.items.length;
  });

  constructor() {
    effect(() => {
      const batchId = this.batchId();
      this.loading.set(true);
      this.errorMessage.set(null);

      this.paymentMatchingService.getBatchDetail(batchId).subscribe({
        next: (detail) => {
          this.detail.set(detail);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage.set(err.error?.message ?? 'Could not load this batch. Please try again.');
          this.loading.set(false);
        },
      });
    });
  }

  onFilterChange(filter: ResultFilter): void {
    this.filter.set(filter);
  }

  onResolveItem(event: { item: PaymentMatchRow; side: ResolutionSide }): void {
    const itemId = event.item.id;
    if (this.resolvingIds().has(itemId)) {
      return;
    }

    this.setResolving(itemId, true);
    this.paymentMatchingService.resolve(itemId, event.side).subscribe({
      next: (updated) => {
        this.applyResolvedItem(updated);
        this.setResolving(itemId, false);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(err.error?.message ?? 'Could not save the resolution. Please try again.');
        this.setResolving(itemId, false);
      },
    });
  }

  private setResolving(itemId: string, resolving: boolean): void {
    const next = new Set(this.resolvingIds());
    if (resolving) {
      next.add(itemId);
    } else {
      next.delete(itemId);
    }
    this.resolvingIds.set(next);
  }

  private applyResolvedItem(updated: PaymentMatchRow): void {
    const current = this.detail();
    if (!current) {
      return;
    }

    this.detail.set({
      ...current,
      items: current.items.map((item) => (item.id === updated.id ? updated : item)),
    });
  }
}
