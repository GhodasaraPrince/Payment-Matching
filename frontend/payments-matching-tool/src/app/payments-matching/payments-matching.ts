import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { PaymentMatchingService } from './payment-matching.service';
import { PaymentMatchRow, ResolutionSide, ResultFilter, RunMatchResponse } from './models';

@Component({
  selector: 'app-payments-matching',
  imports: [],
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
  protected readonly filter = signal<ResultFilter>('unresolved');

  protected readonly summary = computed(() => this.result()?.summary ?? null);
  protected readonly items = computed(() => this.result()?.items ?? []);
  protected readonly filteredItems = computed(() => {
    const filter = this.filter();
    return this.items().filter((item) => {
      if (filter === 'resolved') return item.resolved;
      if (filter === 'unresolved') return !item.resolved;
      return true;
    });
  });

  onSystemFileSelected(event: Event): void {
    this.systemFile.set(this.extractFile(event));
  }

  onProviderFileSelected(event: Event): void {
    this.providerFile.set(this.extractFile(event));
  }

  onFilterChange(event: Event): void {
    this.filter.set((event.target as HTMLSelectElement).value as ResultFilter);
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
        this.filter.set('unresolved');
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

  resolve(item: PaymentMatchRow, resolutionSide: ResolutionSide): void {
    this.paymentMatchingService.resolve(item.id, resolutionSide).subscribe({
      next: (updated) => this.applyResolvedItem(updated),
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(err.error?.message ?? 'Could not save the resolution. Please try again.');
      },
    });
  }

  protected formatAmount(amount: number | null): string {
    return amount === null ? '-' : amount.toFixed(2);
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

  private extractFile(event: Event): File | null {
    const input = event.target as HTMLInputElement;
    return input.files?.[0] ?? null;
  }
}
