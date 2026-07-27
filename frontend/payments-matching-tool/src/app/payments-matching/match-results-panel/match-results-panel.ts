import { Component, computed, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PrimeTemplate } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { MatchStatus, MatchSummary, PaymentMatchRow, ResolutionSide, ResultFilter } from '../models';

const STATUS_SEVERITY: Record<MatchStatus, 'success' | 'warn' | 'danger'> = {
  Matched: 'success',
  OnlySystem: 'warn',
  OnlyProvider: 'warn',
  AmountMismatch: 'danger',
};

@Component({
  selector: 'app-match-results-panel',
  imports: [ButtonModule, CardModule, FormsModule, PrimeTemplate, SelectModule, TableModule, TagModule],
  templateUrl: './match-results-panel.html',
  styleUrl: './match-results-panel.scss',
})
export class MatchResultsPanel {
  readonly summary = input<MatchSummary | null>(null);
  readonly items = input<PaymentMatchRow[]>([]);
  readonly filter = input<ResultFilter>('all');
  readonly resolvingIds = input<ReadonlySet<string>>(new Set());

  readonly filterChange = output<ResultFilter>();
  readonly resolveItem = output<{ item: PaymentMatchRow; side: ResolutionSide }>();

  protected readonly filterOptions: { label: string; value: ResultFilter }[] = [
    { label: 'Unresolved', value: 'unresolved' },
    { label: 'Resolved', value: 'resolved' },
    { label: 'All', value: 'all' },
  ];

  protected readonly filteredItems = computed(() => {
    const filter = this.filter();
    return this.items().filter((item) => {
      if (filter === 'resolved') return item.resolved;
      if (filter === 'unresolved') return !item.resolved;
      return true;
    });
  });

  onFilterChange(filter: ResultFilter): void {
    this.filterChange.emit(filter);
  }

  resolve(item: PaymentMatchRow, side: ResolutionSide): void {
    this.resolveItem.emit({ item, side });
  }

  protected isResolving(itemId: string): boolean {
    return this.resolvingIds().has(itemId);
  }

  protected formatAmount(amount: number | null): string {
    return amount === null ? '-' : amount.toFixed(2);
  }

  protected statusSeverity(status: MatchStatus): 'success' | 'warn' | 'danger' {
    return STATUS_SEVERITY[status];
  }
}
