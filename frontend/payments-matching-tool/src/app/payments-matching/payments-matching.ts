import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { FileSelectEvent, FileUploadModule } from 'primeng/fileupload';
import { MessageModule } from 'primeng/message';
import { MatchResultsPanel } from './match-results-panel/match-results-panel';
import { PaymentMatchingService } from './payment-matching.service';
import { PaymentMatchRow, ResolutionSide, ResultFilter, RunMatchResponse } from './models';

@Component({
  selector: 'app-payments-matching',
  imports: [ButtonModule, CardModule, FileUploadModule, MatchResultsPanel, MessageModule],
  templateUrl: './payments-matching.html',
  styleUrl: './payments-matching.scss',
})
export class PaymentsMatching {
  private readonly paymentMatchingService = inject(PaymentMatchingService);

  protected readonly systemFile = signal<File | null>(null);
  protected readonly providerFile = signal<File | null>(null);
  protected readonly loading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly result = signal<RunMatchResponse | null>(null);
  protected readonly filter = signal<ResultFilter>('all');

  protected readonly summary = computed(() => this.result()?.summary ?? null);
  protected readonly items = computed(() => this.result()?.items ?? []);

  protected readonly canRun = computed(() => !!this.systemFile() && !!this.providerFile() && !this.loading());

  onSystemFileSelected(event: FileSelectEvent): void {
    this.systemFile.set(event.files[0] ?? null);
  }

  onProviderFileSelected(event: FileSelectEvent): void {
    this.providerFile.set(event.files[0] ?? null);
  }

  onFilterChange(filter: ResultFilter): void {
    this.filter.set(filter);
  }

  reset(): void {
    this.systemFile.set(null);
    this.providerFile.set(null);
    this.result.set(null);
    this.filter.set('unresolved');
    this.errorMessage.set(null);
  }

  runMatch(): void {
    this.errorMessage.set(null);

    const systemFile = this.systemFile();
    const providerFile = this.providerFile();

    if (!systemFile || !providerFile) {
      this.errorMessage.set('Please select both a System CSV and a Provider CSV before running the match.');
      return;
    }

    this.loading.set(true);
    this.paymentMatchingService.runMatch(systemFile, providerFile).subscribe({
      next: (response) => {
        this.result.set(response);
        this.filter.set('all');
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(
          err.error?.message ?? 'Something went wrong while running the match. Please try again.',
        );
        this.loading.set(false);
      },
    });
  }

  onResolveItem(event: { item: PaymentMatchRow; side: ResolutionSide }): void {
    this.paymentMatchingService.resolve(event.item.id, event.side).subscribe({
      next: (updated) => this.applyResolvedItem(updated),
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(err.error?.message ?? 'Could not save the resolution. Please try again.');
      },
    });
  }

  private applyResolvedItem(updated: PaymentMatchRow): void {
    const current = this.result();
    if (!current) {
      return;
    }

    this.result.set({
      ...current,
      items: current.items.map((item) => (item.id === updated.id ? updated : item)),
    });
  }
}
