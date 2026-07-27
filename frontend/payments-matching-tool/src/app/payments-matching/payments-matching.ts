import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { PaymentMatchingService } from './payment-matching.service';
import { RunMatchResponse } from './models';

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

  onSystemFileSelected(event: Event): void {
    this.systemFile.set(this.extractFile(event));
  }

  onProviderFileSelected(event: Event): void {
    this.providerFile.set(this.extractFile(event));
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

  private extractFile(event: Event): File | null {
    const input = event.target as HTMLInputElement;
    return input.files?.[0] ?? null;
  }
}
