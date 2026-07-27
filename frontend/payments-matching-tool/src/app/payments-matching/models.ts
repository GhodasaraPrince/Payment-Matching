export type MatchStatus = 'Matched' | 'OnlySystem' | 'OnlyProvider' | 'AmountMismatch';
export type ResolutionSide = 'System' | 'Provider';
export type ResultFilter = 'unresolved' | 'resolved' | 'all';

export interface MatchSummary {
  total: number;
  matched: number;
  onlySystem: number;
  onlyProvider: number;
  amountMismatch: number;
}

export interface PaymentMatchRow {
  id: string;
  orderId: string;
  currency: string;
  systemAmount: number | null;
  providerAmount: number | null;
  status: MatchStatus;
  resolved: boolean;
  resolutionSide: ResolutionSide | null;
  resolvedAtUtc: string | null;
}

export interface RunMatchResponse {
  batchId: string;
  summary: MatchSummary;
  items: PaymentMatchRow[];
}
