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

export interface MatchBatchSummary {
  id: string;
  createdAtUtc: string;
  systemFileName: string;
  providerFileName: string;
  summary: MatchSummary;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface MatchBatchDetail {
  batchId: string;
  createdAtUtc: string;
  systemFileName: string;
  providerFileName: string;
  summary: MatchSummary;
  items: PaymentMatchRow[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
